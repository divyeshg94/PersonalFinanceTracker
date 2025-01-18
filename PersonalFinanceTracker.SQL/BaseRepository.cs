using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Model.Helpers;

namespace PersonalFinanceTracker.SQL
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private PFTDbContext _dbContext;
        private DbSet<T> _dbSet;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly IServiceScopeFactory _scopeFactory;

        public Repository(IServiceScopeFactory scopeFactory, EncryptionHelper encryptionHelper, PFTDbContext moneyPalDbContext)
        {
            _encryptionHelper = encryptionHelper;
            _dbSet = moneyPalDbContext.Set<T>();
            _dbContext=moneyPalDbContext;
            _scopeFactory = scopeFactory;
        }

        private PFTDbContext CreateNewContext()
        {
            var scope = _scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<PFTDbContext>();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            DecryptEntity(entity);
            return entity;
        }

        public async Task<List<T>> FindByAsync(RepositoryModel<T> repositoryModel)
        {
            var dbSet = NoTrackingOptOut(repositoryModel.AsNoTrackingOptOut).Where(repositoryModel.Where);
            if (repositoryModel.Include != null) dbSet = repositoryModel.Include(dbSet);

            var query = repositoryModel.OrderBys.Any()
                ? dbSet.OrderByWithExpressionTransform(repositoryModel.OrderBys)
                : dbSet;

            var projectedResult = query.ToList();
            // Decrypt each entity in the result if encryption is needed
            foreach (var result in projectedResult)
            {
                DecryptEntity(result);
            }

            return projectedResult;
        }

        public async Task<List<TResult>> FindByAsync<TResult>(RepositoryModel<T, TResult> repositoryModel, bool? isEncrypted = false)
        {
            var dbSet = NoTrackingOptOut(repositoryModel.AsNoTrackingOptOut).Where(repositoryModel.Where);

            if (repositoryModel.Include != null)
                dbSet = repositoryModel.Include(dbSet);


            var query = repositoryModel.OrderBys.Any()
                ? dbSet.OrderByWithExpressionTransform(repositoryModel.OrderBys)
                : dbSet;

            // Apply the select projection
            var projectedResult = await query.Select(repositoryModel.Select).ToListAsync();

            // Decrypt if the result is marked as encrypted
            if (isEncrypted.Value)
            {
                for (int i = 0; i < projectedResult.Count; i++)
                {
                    var encryptedValue = projectedResult[i]?.ToString();
                    if (!string.IsNullOrEmpty(encryptedValue))
                    {
                        // Decrypt the encrypted value and update the result
                        projectedResult[i] = (TResult)(object)_encryptionHelper.Decrypt(encryptedValue);
                    }
                }
            }

            return projectedResult;
        }

        public async Task<PagedEntities<T>> FindByPageAsync(RepositoryModel<T> repositoryModel)
        {
            if (!repositoryModel.OrderBys.Any())
            {
                repositoryModel.OrderBys.Add(new OrderByModel<T> { Ascending = true });
            }
            var dbSet = NoTrackingOptOut(repositoryModel.AsNoTrackingOptOut).Where(repositoryModel.Where);
            if (repositoryModel.Include != null) dbSet = repositoryModel.Include(dbSet);
            var dbSetOrdered = dbSet.OrderByWithExpressionTransform(repositoryModel.OrderBys);
            var count = await dbSetOrdered.CountAsync();
            var query = dbSetOrdered.Skip(repositoryModel.Skip).Take(repositoryModel.Take);
            // Decrypt each entity in the result if encryption is needed

            var items = new List<T>();
            items = await query.ToListAsync();
            foreach (var entity in items)
            {
                DecryptEntity(entity);
            }

            return new PagedEntities<T>
            {
                Count = count,
                Entities = items
            };
        }

        public async Task<T> GetAsync(RepositoryModel<T> repositoryModel)
        {
            if (repositoryModel.Id != null && repositoryModel.Include == null)
            {
                var data = await _dbSet.FindAsync(repositoryModel.Id);
                DecryptEntity(data);
                return data;
            }

            //if (repositoryModel.Id != null)
            //{
            //    repositoryModel.Where = x => x.Id == repositoryModel.Id;
            //}

            var dbSet = NoTrackingOptOut(repositoryModel.AsNoTrackingOptOut).Where(repositoryModel.Where);
            if (repositoryModel.Include != null) dbSet = repositoryModel.Include(dbSet);
            if (!repositoryModel.OrderBys.Any())
            {
                var entity = dbSet.FirstOrDefault();
                if(entity != null)
                    DecryptEntity(entity);
                
                return entity;
            }
            var result = await dbSet.OrderByWithExpressionTransform(repositoryModel.OrderBys).FirstOrDefaultAsync();
            DecryptEntity(result);
            return result;
        }


        public async Task AddAsync(T entity)
        {
            EncryptEntity(entity);
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            EncryptEntity(entity);
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        private void EncryptEntity(T entity)
        {
            var encryptedProperties = GetEncryptedProperties(entity);
            foreach (var property in encryptedProperties)
            {
                var value = property.GetValue(entity)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    if (property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
                    {
                        // Convert the float to string for encryption
                        var floatValue = (float?)property.GetValue(entity);
                        if (floatValue.HasValue)
                        {
                            var encryptedValue = _encryptionHelper.Encrypt(floatValue.Value.ToString("G"));  // Encrypt float value
                            property.SetValue(entity, encryptedValue);
                        }
                    }
                    else
                    {
                        // Encrypt string or other types directly
                        property.SetValue(entity, _encryptionHelper.Encrypt(value));
                    }
                }
            }
        }

        private void DecryptEntity(T entity)
        {
            var encryptedProperties = GetEncryptedProperties(entity);
            foreach (var property in encryptedProperties)
            {
                var value = property.GetValue(entity)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var decryptedValue = _encryptionHelper.Decrypt(value);

                    // Check if the property type is float (Single)
                    if (property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
                    {
                        // Try to parse the decrypted value to a float
                        if (float.TryParse(decryptedValue, out float floatValue))
                        {
                            property.SetValue(entity, floatValue);
                        }
                        else
                        {
                            // Handle the case where the decrypted value could not be parsed to float
                            throw new ArgumentException($"Unable to convert decrypted value '{decryptedValue}' to float.");
                        }
                    }
                    else
                    {
                        // If not float, set the decrypted string value
                        property.SetValue(entity, decryptedValue);
                    }
                }
            }
        }

        internal IQueryable<T> NoTrackingOptOut(bool optOut)
        {
            _dbContext = CreateNewContext();
            _dbSet = _dbContext.Set<T>();
            return optOut ? _dbContext.Set<T>() : _dbContext.Set<T>().AsNoTracking();
        }

        private static PropertyInfo[] GetEncryptedProperties(T entity)
        {
            return entity.GetType()
                         .GetProperties()
                         .Where(prop => prop.IsDefined(typeof(EncryptedAttribute), false))
                         .ToArray();
        }
    }
}
