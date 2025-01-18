using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.SQL.Models;
using PersonalFinanceTracker.SQL;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using PersonalFinanceTracker.Model;
using AutoMapper;
using PersonalFinanceTracker.Model.Helpers;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceTracker.Service
{
    public class IncomeService
    {
        private readonly IRepository<Income> _incomeRepository;
        private readonly IMapper _mapper;
        private readonly EncryptionHelper _encryptionHelper;

        public IncomeService(IRepository<Income> incomeRepository, IMapper mapper, EncryptionHelper encryptionHelper)
        {
            _mapper = mapper;
            _incomeRepository=incomeRepository;
            _encryptionHelper=encryptionHelper;
        }

        public async Task<Income> Get(Guid? id)
        {
            if (!id.HasValue)
                throw new ArgumentNullException("id");

            var income = await _incomeRepository.GetAsync(new RepositoryModel<Income>()
            {
                Where = i => i.Id == id
            });
            return income == null ? throw new Exception("Income doesnot exists for the given Id") : income;
        }

        public async Task<PagedEntities<Income>> GetIncomes(Guid userId, DateTime? startDate, DateTime? endTime, Model.Query query)
        {
            var income = await _incomeRepository.FindByPageAsync(new RepositoryModel<Income>()
            {
                AsNoTrackingOptOut = true,
                Where = i => i.UserId == userId && (!startDate.HasValue || i.IncomeDate >= startDate.Value)
                        && (!endTime.HasValue || i.IncomeDate <= endTime.Value)
                        && (query == null || string.IsNullOrEmpty(query.Filter) || i.Name.ToLower().Contains(query.Filter.ToLower())),
                OrderBy = new OrderByModel<Income>()
                {
                    Ascending = false,
                    Expression = x => x.IncomeDate
                },
                Take = query?.Take ?? 10,
                Skip = query?.Skip ?? 0
            });


            return income;
        }

        public async Task<List<Income>> GetIncomeByQuery(Guid userId, DateTime? startDate, DateTime? endTime)
        {
            var incomeQuery = await _incomeRepository.FindByAsync(new RepositoryModel<Income>()
            {
                AsNoTrackingOptOut = true,
                Where = e => e.UserId == userId
                        && (!startDate.HasValue || e.IncomeDate >= startDate.Value)
                        && (!endTime.HasValue || e.IncomeDate <= endTime.Value),
                OrderBy = new OrderByModel<Income>() { Ascending = false, Expression = x => x.IncomeDate },
            });

            return incomeQuery.ToList();
        }

        public async Task<float> GetTotalIncomeAmount(Guid userId, DateTime? startDate, DateTime? endDate, string? category = null)
        {
            var query = await _incomeRepository.FindByAsync(new RepositoryModel<Income>()
            {
                Where = x => x.UserId == userId && x.IsExcludeTransaction == false
                        && (!startDate.HasValue || x.IncomeDate >= startDate.Value)
                        && (!endDate.HasValue || x.IncomeDate <= endDate.Value)
                        && (string.IsNullOrEmpty(category) || x.Category == category),
            });

            return query.ToList()?.Select(q => q.Amount)?.Sum() ?? 0;
        }

        public async Task<Income> AddIncome(IncomeIn incomeIn)
        {
            if (incomeIn == null)
                throw new Exception("Invalid Input - Income");

            var income = _mapper.Map<Income>(incomeIn);
            income.EncryptedAmount = incomeIn.Amount.ToString();

            await _incomeRepository.AddAsync(income);
            return income;
        }

        public async Task AddIncome(List<IncomeIn> incomeIn, Guid userId)
        {
            if (incomeIn == null || !incomeIn.Any())
                throw new Exception("Invalid Input - Income");

            var transactionIds = await _incomeRepository.FindByAsync(new RepositoryModel<Income, string>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId,
                Select = x => x.TransactionId
            });

            transactionIds = transactionIds.Distinct().Where(t => !string.IsNullOrEmpty(t)).ToList();
            incomeIn = incomeIn.Where(i => !transactionIds.Contains(i.TransactionId)).ToList();

            foreach (var income in incomeIn)
            {
                var incomeModel = _mapper.Map<Income>(income);
                incomeModel.EncryptedAmount = income.Amount.ToString();

                await _incomeRepository.AddAsync(incomeModel);
            }
        }

        public async Task Edit(Income incomeIn)
        {
            if (incomeIn == null)
                throw new Exception("Invalid Input - Income");

            var income = await _incomeRepository.GetAsync(new RepositoryModel<Income>()
            {
                Where = i => i.Id == incomeIn.Id
            });

            if (income == null)
                throw new Exception("Income not found for the given Id");

            income.UpdatedDateTime= DateTime.UtcNow;
            income.IncomeDate = incomeIn.IncomeDate;
            income.Category = incomeIn.Category;
            income.Notes = incomeIn.Notes;
            income.EncryptedAmount = incomeIn.EncryptedAmount;
            income.Name= incomeIn.Name;
            income.BankId = incomeIn.BankId;
            income.CurrencyCode = incomeIn.CurrencyCode;

            await _incomeRepository.UpdateAsync(income);
        }

        public async Task EditByTransaction(List<IncomeIn> incomeIn)
        {
            if (incomeIn == null)
                throw new Exception("Invalid Input - Income");

            var income = await _incomeRepository.FindByAsync(new RepositoryModel<Income>()
            {
                Where = i => incomeIn.Any(n => n.TransactionId == i.TransactionId)
            });

            if (income == null)
                throw new Exception("Income not found for the given Id");

            foreach (var incomeDb in income)
            {
                var newIncome = incomeIn.FirstOrDefault(i => i.TransactionId == incomeDb.TransactionId);

                incomeDb.UpdatedDateTime= DateTime.UtcNow;
                incomeDb.IncomeDate = newIncome.IncomeDate;
                incomeDb.Category = newIncome.Category;
                incomeDb.EncryptedAmount = newIncome.Amount.ToString();
                incomeDb.Name= newIncome.Name;
                incomeDb.BankId = newIncome.BankId;
                incomeDb.CurrencyCode = newIncome.CurrencyCode;

                if (!string.IsNullOrEmpty(newIncome.Notes))
                {
                    incomeDb.Notes = newIncome.Notes;
                }

                await _incomeRepository.UpdateAsync(incomeDb);
            }
        }

        public async Task Delete(Guid id)
        {
            if (id == null || id == Guid.Empty)
                throw new Exception("Invalid Input - Id");

            var income = await _incomeRepository.GetAsync(new RepositoryModel<Income>()
            {
                Where = i => i.Id == id
            });
            if (income == null)
                throw new Exception("Income not found for the given Id");

            await _incomeRepository.DeleteAsync(income);
        }

        public async Task DeleteByTransactionIds(Guid userId, List<string> transactionIds)
        {
            if (transactionIds == null)
                throw new Exception("Invalid Input - Id");

            var incomes = await _incomeRepository.FindByAsync(new RepositoryModel<Income>()
            {
                Where = i => i.UserId == userId && transactionIds.Contains(i.TransactionId)
            });

            foreach (var income in incomes)
            {
                await _incomeRepository.DeleteAsync(income);
            }
        }
    }
}
