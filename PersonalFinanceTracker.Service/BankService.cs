using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.Model.Helpers;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Service
{
    public class BanksService
    {
        public static PFTDbContext DbContext { get; set; }
        private readonly IRepository<Banks> _banksRepository;
        private readonly EncryptionHelper _encryptionHelper;

        public BanksService(PFTDbContext dbContext, IRepository<Banks> banksRepository, EncryptionHelper encrptionHelper)
        {
            DbContext = dbContext;
            _banksRepository = banksRepository;
            _encryptionHelper = encrptionHelper;
        }

        public async Task<Banks> Get(Guid id)
        {
            var bank = await _banksRepository.GetByIdAsync(id);
            if (bank == null)
                throw new Exception($"Bank not found for the given Id - {id}");

            return bank;
        }

        public async Task<List<BankBasicInfo>> GetUserBanks(Guid userId)
        {
            return await _banksRepository.FindByAsync(new RepositoryModel<Banks, BankBasicInfo>()
            {
                Where = b => b.UserId == userId && b.IsDeleted == false,
                Select = b => new BankBasicInfo()
                {
                    Id = b.Id,
                    Name = b.Name,
                    CurrencyCode = b.CurrencyCode
                }
            });
        }

        public async Task<List<BankAccountInfo>> GetUserBankAccountInfo(Guid userId)
        {
            var banks = await _banksRepository.FindByAsync(new RepositoryModel<Banks, BankAccountInfo>()
            {
                Where = b => b.UserId == userId && b.IsDeleted == false,
                Select = b => new BankAccountInfo()
                {
                    Id = b.Id,
                    AccountNumber  = b.AccountNumber
                }
            });

            foreach (var bank in banks)
            {
                try
                {
                    bank.AccountNumber = _encryptionHelper.Decrypt(bank.AccountNumber);
                }
                catch
                {

                }
            }

            return banks;
        }

        public async Task<PagedEntities<Banks>> GetBanks(Guid userId, Query query)
        {
            return await _banksRepository.FindByPageAsync(new RepositoryModel<Banks>
            {
                Where = b => b.UserId == userId && b.IsDeleted == false && (string.IsNullOrEmpty(query.Filter) || b.AccountNumber.Contains(query.Filter) || b.AccountHolderName.Contains(query.Filter)
                                    || b.IFSC.Contains(query.Filter) || b.Name.Contains(query.Filter)),
                OrderBy = new OrderByModel<Banks>()
                {
                    Ascending = true,
                    Expression = b => b.Name
                },
                Skip = query.Skip ?? 0,
                Take = query.Take ?? 10,
            });
        }

        public async Task<float> GetAvailableBalance(Guid userId)
        {
            var banks = await _banksRepository.FindByAsync(new RepositoryModel<Banks, string>()
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId && x.IsDeleted == false,
                Select = x => x.ClosingBalance
            }, true);

            return banks.Select(b => new
            {
                amount = float.TryParse(b, out var result) ? result : 0
            }).Sum(s => s.amount);
        }

        public async Task<Banks> AddBanks(Guid userId, BanksIn bank)
        {
            var newBank = new Banks()
            {
                UserId = userId,
                CreatedDateTime = DateTime.Now,
                Name = bank.Name,
                CurrencyCode = bank.CurrencyCode,
                IsDeleted = false,
                IsEncrypted = false,
                ClosingBalance = bank.ClosingBalance
            };
            await _banksRepository.AddAsync(newBank);
            return newBank;
        }

        public async Task<Banks> AddBanks(Guid userId, string accountHolderName,
            string bankName, string accountNumber, string ifsc, float? closingBalance = 0, Guid? plaidUserItemId = null,
            string accountType = null, string accountSubType = null, string currencyCode = "USD")
        {
            var newBank = new Banks()
            {
                UserId = userId,
                CreatedDateTime = DateTime.Now,
                AccountHolderName = accountHolderName,
                AccountNumber = accountNumber,
                IFSC = ifsc,
                CurrencyCode = currencyCode,
                ClosingBalance = closingBalance.HasValue ? closingBalance.Value.ToString() : "0",
                Name = bankName,
                PlaidItemId = plaidUserItemId,
                AccountType = accountType,
                AccountSubType = accountSubType,
                IsDeleted = false,
                IsEncrypted = false
            };
            await _banksRepository.AddAsync(newBank);
            return newBank;
        }

        public async Task Edit(Banks bankIn)
        {
            if (bankIn == null)
                throw new Exception("Invalid Input - bank");

            var bank = await _banksRepository.GetAsync(new RepositoryModel<Banks>()
            {
                Where = i => i.Id == bankIn.Id
            });

            if (bank == null)
                throw new Exception("Bank not found for the given Id");

            bank.UpdatedDateTime = DateTime.UtcNow;
            bank.AccountHolderName = bankIn.AccountHolderName;
            bank.AccountNumber = bankIn.AccountNumber;
            bank.Name = bankIn.Name;
            bank.IFSC = bankIn.IFSC;
            bank.IsDeleted = false;
            bank.IsEncrypted = false;
            bank.CurrencyCode = bankIn.CurrencyCode;
            bank.UpdatedDateTime = DateTime.UtcNow;
            bank.ClosingBalance = bankIn.ClosingBalance;

            await _banksRepository.UpdateAsync(bank);
        }

        public async Task Delete(Guid id)
        {
            if (id == null || id == Guid.Empty)
                throw new Exception("Invalid Input - Id");

            var bank = await _banksRepository.GetAsync(new RepositoryModel<Banks>()
            {
                Where = i => i.Id == id
            });

            if (bank == null)
                throw new Exception("Bank not found for the given Id");

            bank.IsDeleted = true;
            bank.UpdatedDateTime = DateTime.UtcNow;

            await _banksRepository.UpdateAsync(bank);
        }
    }
}
