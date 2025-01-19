using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Service
{
    public class PlaidUserItemService
    {
        private readonly IRepository<PlaidUserItems> _plaidUserItemsRepository;

        public PlaidUserItemService(IRepository<PlaidUserItems> plaidUserItemsRepository)
        {
            _plaidUserItemsRepository = plaidUserItemsRepository;
        }

        public async Task<PlaidUserItems> Add(Guid userId, PlaidCredentials credentials)
        {
            var plaidUserItems = new SQL.Models.PlaidUserItems()
            {
                AccessToken = credentials.AccessToken,
                CreatedDateTime = DateTime.UtcNow,
                ItemId = credentials.ItemId,
                UserId = userId
            };
            await _plaidUserItemsRepository.AddAsync(plaidUserItems);
            return plaidUserItems;
        }

        public async Task<PlaidUserItems> GetPlaidUserItem(string itemId)
        {
            var allPlaidUserItems = await _plaidUserItemsRepository.FindByAsync(new RepositoryModel<PlaidUserItems>()
            {
                AsNoTrackingOptOut = true,
                Where = x => x.IsDeleted == false,
            });

            return allPlaidUserItems.FirstOrDefault(a => a.ItemId == itemId);
        }

        public async Task<string> GetCursor(Guid userPlaidItemId)
        {
            var plaidUserItem = await _plaidUserItemsRepository.GetAsync(new RepositoryModel<PlaidUserItems>
            {
                AsNoTrackingOptOut = true,
                Where = x => x.Id== userPlaidItemId && x.IsDeleted == false,
            });

            return plaidUserItem.Cursor;
        }

        public async Task UpdateCursor(Guid userId, Guid userPlaidItemId, string cursor)
        {
            var userPlaidItem = await _plaidUserItemsRepository.GetAsync(new RepositoryModel<PlaidUserItems>
            {
                Where = x => x.UserId == userId && x.IsDeleted == false && x.Id == userPlaidItemId
            });

            if (userPlaidItem != null)
            {
                userPlaidItem.Cursor = cursor;
                await _plaidUserItemsRepository.UpdateAsync(userPlaidItem);
            }
        }
    }
}
