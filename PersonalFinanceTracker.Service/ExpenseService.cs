using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Service
{
    public class ExpenseService
    {
        private readonly IRepository<Expenses> _expenseRepository;
        private readonly IMapper _mapper;

        public ExpenseService(PFTDbContext dbContext, IRepository<Expenses> expenseRepository, IMapper mapper)
        {
            _mapper = mapper;
            _expenseRepository = expenseRepository;
        }

        public async Task<Expenses> Get(Guid? id)
        {
            if (!id.HasValue)
                throw new ArgumentNullException("id");

            var expense = await _expenseRepository.GetByIdAsync(id);
            return expense == null ? throw new Exception("Expense doesnot exists for the given Id") : expense;
        }

        public async Task<PagedEntities<Expenses>> GetExpenses(Guid userId, DateTime? startDate, DateTime? endTime, Query query = null)
        {
            var expensesQuery = await _expenseRepository.FindByPageAsync(new RepositoryModel<Expenses>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId
                            && (!startDate.HasValue || x.Date >= startDate.Value)
                            && (!endTime.HasValue || x.Date <= endTime.Value)
                            && (query == null || string.IsNullOrEmpty(query.Filter) || x.Name.ToLower().Contains(query.Filter.ToLower())),
                OrderBy = new OrderByModel<Expenses>
                {
                    Ascending = false,
                    Expression = x => x.Date
                },
                Include = x => x.Include(y => y.Bank),
                Skip = query?.Skip ?? 0,
                Take  = query?.Take ?? 10,
            });
            return expensesQuery;
        }

        public async Task<List<Expenses>> GetExpensesByQuery(Guid userId, DateTime? startDate, DateTime? endTime)
        {
            var expensesQuery = await _expenseRepository.FindByAsync(new RepositoryModel<Expenses>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId && x.IsExcludeTransaction == false
                            && (!startDate.HasValue || x.Date >= startDate.Value)
                            && (!endTime.HasValue || x.Date <= endTime.Value),
                OrderBy = new OrderByModel<Expenses>
                {
                    Ascending = false,
                    Expression = x => x.Date
                }
            });

            return expensesQuery.ToList();
        }

        public async Task<float> GetTotalExpenseAmount(Guid userId, DateTime? startDate, DateTime? endDate, string? category = null, string? paidVia = null)
        {
            var expensesQuery = await _expenseRepository.FindByAsync(new RepositoryModel<Expenses>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId && x.IsExcludeTransaction == false
                            && (!startDate.HasValue || x.Date >= startDate.Value)
                            && (!endDate.HasValue || x.Date <= endDate.Value)
                            && (string.IsNullOrEmpty(category) || x.Category == category)
                            && (string.IsNullOrEmpty(paidVia) || x.PaidVia == paidVia)
            });

            return expensesQuery.ToList()?.Select(q => q.Amount)?.Sum() ?? 0;
        }

        public async Task<Expenses> AddExpense(ExpenseIn expenseIn)
        {
            if (expenseIn == null)
                throw new Exception("Invalid Input - Expense");

            var expense = _mapper.Map<Expenses>(expenseIn);
            expense.EncryptedAmount = expenseIn.Amount.ToString();

            await _expenseRepository.AddAsync(expense);
            return expense;
        }


        public async Task AddExpense(List<ExpenseIn> expenseIn, Guid userId)
        {
            if (expenseIn == null || !expenseIn.Any())
                throw new Exception("Invalid Input - Expense");

            var transactionIds = await _expenseRepository.FindByAsync(new RepositoryModel<Expenses, string>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.UserId == userId,
                Select = x => x.TransactionId
            });

            transactionIds = transactionIds.Distinct().Where(t => !string.IsNullOrEmpty(t)).ToList();
            expenseIn = expenseIn.Where(i => !transactionIds.Contains(i.TransactionId)).ToList();

            foreach (var expense in expenseIn)
            {
                var expenseModel = _mapper.Map<Expenses>(expense);
                expenseModel.EncryptedAmount = expense.Amount.ToString();

                await _expenseRepository.AddAsync(expenseModel);
            }
        }

        public async Task Edit(Expenses expenses, Guid userId)
        {
            if (expenses == null)
                throw new Exception("Invalid Input - expenses");

            var expense = await _expenseRepository.GetAsync(new RepositoryModel<Expenses>()
            {
                Where = i => i.Id == expenses.Id && i.UserId == userId
            });

            if (expense == null)
                throw new Exception("Expenses not found for the given Id");

            expense.UpdatedDateTime= DateTime.UtcNow;
            expense.Date = expenses.Date;
            expense.Category = expenses.Category;
            expense.Notes = expenses.Notes;
            expense.EncryptedAmount = expenses.EncryptedAmount;
            expense.Name= expenses.Name;
            expense.PaidVia = expenses.PaidVia;
            expense.BankId = expenses.BankId;
            expense.CurrencyCode = expenses.CurrencyCode;

            await _expenseRepository.UpdateAsync(expense);
        }

        public async Task EditByTrasaction(List<ExpenseIn> expenses, Guid userId)
        {
            if (expenses == null)
                throw new Exception("Invalid Input - expenses");

            var expensesDb = await _expenseRepository.FindByAsync(new RepositoryModel<Expenses>()
            {
                Where = i => expenses.Any(n => n.TransactionId == i.TransactionId)
            });

            if (expensesDb == null)
                throw new Exception("Expenses not found for the given Id");

            foreach (var expenseDb in expensesDb)
            {
                var newExpense = expenses.FirstOrDefault(e => e.TransactionId == expenseDb.TransactionId);

                expenseDb.UpdatedDateTime= DateTime.UtcNow;
                expenseDb.Date = newExpense.Date;
                expenseDb.Category = newExpense.Category;
                expenseDb.EncryptedAmount = newExpense.Amount.ToString();
                expenseDb.Name= newExpense.Name;
                expenseDb.PaidVia = newExpense.PaidVia;
                expenseDb.BankId = newExpense.BankId;

                if (!string.IsNullOrEmpty(newExpense.Notes))
                    expenseDb.Notes = newExpense.Notes;

                await _expenseRepository.UpdateAsync(expenseDb);
            }
        }

        public async Task Delete(Guid id, Guid userId)
        {
            if (id == null || id == Guid.Empty)
                throw new Exception("Invalid Input - Id");

            var expenses = await _expenseRepository.GetAsync(new RepositoryModel<Expenses>()
            {
                Where = i => i.Id == id && i.UserId == userId
            });

            if (expenses == null)
                throw new Exception("Expense not found for the given Id");

            await _expenseRepository.DeleteAsync(expenses);
        }

        public async Task DeleteByTransaction(Guid userId, List<string> transactionIds)
        {
            if (transactionIds == null)
                throw new Exception("Invalid Input - Id");

            var expenses = await _expenseRepository.FindByAsync(new RepositoryModel<Expenses>()
            {
                Where = i => i.UserId == userId && transactionIds.Contains(i.TransactionId)
            });

            foreach (var expense in expenses)
            {
                await _expenseRepository.DeleteAsync(expense);
            }
        }
    }
}