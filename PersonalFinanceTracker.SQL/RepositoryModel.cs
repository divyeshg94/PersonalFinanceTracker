using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace PersonalFinanceTracker.SQL
{
    public class RepositoryModel<T>
    {
        /// <summary>
        /// Some methods like <seealso cref="IRepository{T}.FindByAsync"/> ignore this property. Use <seealso cref="IRepository{T}.GetAsync"/> instead.
        /// </summary>
        public int? Id { get; set; }

        public OrderByModel<T> OrderBy
        {
            set => OrderBys.Add(value);
        }

        public List<OrderByModel<T>> OrderBys { get; set; } = new List<OrderByModel<T>>();

        public Expression<Func<T, bool>> Where { get; set; } = x => true;

        public int Skip { get; set; } = 1;

        public int Take { get; set; } = 10;
        public Func<IQueryable<T>, IIncludableQueryable<T, object>> Include { get; set; }

        public bool AsNoTrackingOptOut { get; set; } = false;
    }

    public class RepositoryModel<T, TResult> : RepositoryModel<T>
    {
        public Expression<Func<T, TResult>> Select { get; set; }
    }

    public class OrderByModel<T>
    {
        public Expression<Func<T, object>> Expression { get; set; }

        public bool Ascending { get; set; } = true;
    }

    public class PagedEntities<T>
    {
        public int Count { get; set; }
        public IEnumerable<T> Entities { get; set; }
    }
}
