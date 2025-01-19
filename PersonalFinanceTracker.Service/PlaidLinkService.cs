using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinanceTracker.SQL;
using System.Text.Json;
using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Service
{
    public class PlainLinkService
    {
        private readonly PlaidClient _plaidClient;
        private readonly ILogger<PlainLinkService> _logger;
        private readonly PlaidCredentials _credentials;
        private readonly PFTDbContext _dbContext;
        private readonly PlaidUserItemService _plaidUserItemService;
        private readonly BanksService _banksService;
        private readonly IncomeService _incomeService;
        private readonly ExpenseService _expenseService;

        public PlainLinkService(PlaidClient plaidClient,
            PFTDbContext dbContext,
            BanksService banksService,
            PlaidUserItemService plaidUserItemService,
            ILogger<PlainLinkService> logger,
            IncomeService incomeService,
            ExpenseService expenseService,
            IOptions<PlaidCredentials> credentials)
        {
            _plaidUserItemService = plaidUserItemService;
            _banksService = banksService;
            _plaidClient = plaidClient;
            _dbContext = dbContext;
            _logger = logger;
            _credentials = credentials.Value;
            _incomeService = incomeService;
            _expenseService = expenseService;
        }

        public async Task<string> CreateLinkToken(bool? fix)
        {
            try
            {
                _logger.LogInformation($"CreateLinkToken (): {fix ?? false}");
                var response = await _plaidClient.LinkTokenCreateAsync(
                new LinkTokenCreateRequest()
                {
                    AccessToken = fix == true ? _credentials.AccessToken : null,
                    User = new LinkTokenCreateRequestUser { ClientUserId = Guid.NewGuid().ToString(), },
                    ClientName = _credentials.ClientName ?? "MoneyPal",
                    Products = fix != true ? _credentials!.Products!.Split(',').Select(p => Enum.Parse<Products>(p, true)).ToArray() : Array.Empty<Products>(),
                    Language = Language.English,
                    Transactions = new LinkTokenTransactions()
                    {
                        DaysRequested = 730//Should be based on the user Plan
                    },
                    Webhook = _credentials.WebhookUrl,
                    CountryCodes = _credentials!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
                });

                if (response.Error is not null)
                    throw new Exception(response.Error.ErrorMessage);

                _logger.LogInformation($"CreateLinkToken OK: {JsonSerializer.Serialize(response.LinkToken)}");

                return response.LinkToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<PlaidCredentials> ExchangePublicToken(string publicToken, Guid userId)
        {
            var request = new ItemPublicTokenExchangeRequest()
            {
                PublicToken = publicToken
            };

            var response = await _plaidClient.ItemPublicTokenExchangeAsync(request);

            if (response.Error is not null)
                throw new Exception(response.Error.ErrorMessage);

            _credentials.AccessToken = response.AccessToken;
            _credentials.ItemId = response.ItemId;

            _logger.LogInformation($"ExchangePublicToken OK: {JsonSerializer.Serialize(_credentials)}");

            var plaidUserItem = await _plaidUserItemService.Add(userId, _credentials);
            await GetAccount(userId, _credentials, plaidUserItem.Id);

            return _credentials;
        }

        public async Task GetAccount(Guid userId, PlaidCredentials credentials, Guid userPlaidItemId)
        {
            var request = new Going.Plaid.Accounts.AccountsGetRequest()
            {
                AccessToken = credentials.AccessToken,
            };

            var response = await _plaidClient.AccountsGetAsync(request);
            if (response.Error is not null)
                throw new Exception(response.Error.ErrorMessage);

            var item = await _plaidClient.ItemGetAsync(new ItemGetRequest()
            {
                AccessToken = credentials.AccessToken,
            });

            var institutionId = item.Item.InstitutionId;
            var institution = await _plaidClient.InstitutionsGetByIdAsync(new Going.Plaid.Institutions.InstitutionsGetByIdRequest()
            {
                InstitutionId = institutionId,
                CountryCodes = new List<CountryCode> { CountryCode.Us }
            });

            var institutionName = institution?.Institution?.Name;
            foreach (var account in response.Accounts)
            {
                var balance = account?.Balances?.Current ?? 0;
                var bankName = !string.IsNullOrEmpty(institutionName) ? $"{institutionName} {account.Name}" : account.Name;
                var bank = await _banksService.AddBanks(userId, null, bankName,
                    account.AccountId, null, (float)balance,
                    userPlaidItemId, account.Type.ToString(), account.Subtype.ToString(), "USD");
            }

            await Transactions(userId, userPlaidItemId, credentials.AccessToken, string.Empty);
        }

        public async Task SyncTransactions(string itemId)
        {
            var plaidUserItem = await _plaidUserItemService.GetPlaidUserItem(itemId);

            if (plaidUserItem != null)
            {
                await Transactions(plaidUserItem.UserId, plaidUserItem.Id, plaidUserItem.AccessToken, plaidUserItem.Cursor);
            }
        }

        public async Task Transactions(Guid userId, Guid userPlaidItemId, string accessToken, string cursor)
        {
            var added = new List<Transaction>();
            var modified = new List<Transaction>();
            var removed = new List<RemovedTransaction>();
            var hasMore = true;
            var userBanks = await _banksService.GetUserBankAccountInfo(userId);
            while (hasMore)
            {
                const int numrequested = 100;
                var request = new Going.Plaid.Transactions.TransactionsSyncRequest()
                {
                    Cursor = cursor,
                    Count = numrequested,
                    AccessToken = accessToken,
                    Options = new TransactionsSyncRequestOptions()
                    {
                        IncludeLogoAndCounterpartyBeta = false,
                        IncludePersonalFinanceCategory = true,
                    }
                };

                var response = await _plaidClient.TransactionsSyncAsync(request);

                if (response?.Error?.ErrorCode == "TRANSACTIONS_SYNC_MUTATION_DURING_PAGINATION")
                {
                    cursor = await _plaidUserItemService.GetCursor(userPlaidItemId);
                    continue;
                }

                if (response.Error is not null)
                {
                    throw new Exception(response.Error.ErrorMessage);
                }

                added.AddRange(response!.Added);
                modified.AddRange(response.Modified);
                removed.AddRange(response.Removed);
                hasMore = response.HasMore;
                cursor = response.NextCursor;

                if (added.Any())
                    await ClassifyTransactions(added, userId, userBanks, false);

                if (modified.Any())
                    await ClassifyTransactions(modified, userId, userBanks, true);

                if (removed.Any())
                    await DeleteTrasactions(removed, userId);

            }

            await _plaidUserItemService.UpdateCursor(userId, userPlaidItemId, cursor);
        }

        private async Task ClassifyTransactions(List<Transaction> transactions, Guid userId, List<BankAccountInfo> userBanks, bool isUpdate = false)
        {
            //CategoryDetailed - find the usage and incorporate
            var rows = transactions
                .OrderBy(x => x.Date)
                .Select(x =>
                    new
                    {
                        Name = x.Name ?? string.Empty,
                        Amount = x.Amount ?? 0,
                        Date = x.Datetime.HasValue ? x.Datetime.Value.DateTime : x.Date.Value.ToDateTime(TimeOnly.MinValue),
                        Category = x?.PersonalFinanceCategory?.Primary,
                        PaidVia = x.PaymentChannel.ToString() ?? string.Empty,
                        TransactionId = x.TransactionId,
                        CurrencyCode = x.IsoCurrencyCode,
                        AccountNumber = x.AccountId
                    }
                )
                .ToList();

            var income = rows.Where(r => r.Amount < 0).Select(s => new IncomeIn()
            {
                CreatedDateTime = DateTime.UtcNow,
                Amount =  Math.Abs((float)s.Amount),
                Category = s.Category,
                Name = s.Name,
                IncomeDate = s.Date,
                BankId = userBanks.FirstOrDefault(b => b.AccountNumber == s.AccountNumber)?.Id,
                CurrencyCode = s.CurrencyCode,
                TransactionId= s.TransactionId,
                UserId = userId
            }).ToList();

            if (income.Any())
            {
                if (!isUpdate)
                {
                    await _incomeService.AddIncome(income, userId);
                }
                else
                {
                    await _incomeService.EditByTransaction(income);
                }
            }

            var expenses = rows.Where(r => r.Amount > 0).Select(s => new ExpenseIn()
            {
                CreatedDateTime = DateTime.UtcNow,
                UserId = userId,
                Amount = Math.Abs((float)s.Amount),
                Category = s.Category,
                CurrencyCode = s.CurrencyCode,
                Name = s.Name,
                Date = s.Date,
                BankId = userBanks.FirstOrDefault(b => b.AccountNumber == s.AccountNumber)?.Id,
                PaidVia = s.PaidVia,
                TransactionId= s.TransactionId,
            }).ToList();

            if (expenses.Any())
            {
                if (!isUpdate)
                {
                    await _expenseService.AddExpense(expenses, userId);
                }
                else
                {
                    await _expenseService.EditByTrasaction(expenses, userId);
                }
            }
        }

        private async Task DeleteTrasactions(List<RemovedTransaction> transactions, Guid userId)
        {
            var transactionIds = transactions.Select(t => t.TransactionId).ToList();
            await _incomeService.DeleteByTransactionIds(userId, transactionIds);
            await _expenseService.DeleteByTransaction(userId, transactionIds);
        }
    }
}
