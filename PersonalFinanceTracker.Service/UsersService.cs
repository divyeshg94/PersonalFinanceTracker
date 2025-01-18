using Newtonsoft.Json;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Service
{
    public class UsersService
    {
        private readonly IRepository<Users> _userRepository;

        public UsersService(IRepository<Users> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Users> GetUser(string userUpn)
        {
            var user = await _userRepository.GetAsync(new RepositoryModel<Users>()
            {
                Where = u => u.UserUpn == userUpn
            });

            if (user == null)
                throw new Exception("User not found");

            return user;
        }

        public async Task<Users> GetUser(Guid id)
        {
            var user = await _userRepository.GetAsync(new RepositoryModel<Users>() { Where = u => u.Id == id });
            if (user == null)
                throw new Exception("User not found");

            return user;
        }

        public async Task<List<string>> GetUserCurrencyCodes(Guid userId)
        {
            var defaultCurrencies = new List<string>() { "USD" };
            var user = await _userRepository.GetAsync(new RepositoryModel<Users>() { Where = u => u.Id == userId });

            if (user == null)
                throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.Currencies))
                return defaultCurrencies;

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(user.Currencies) ?? defaultCurrencies;
            }
            catch (Exception ex)
            {
                return defaultCurrencies;
            }
        }

        public async Task<Users> Add(Users user)
        {
            if (user == null)
                throw new Exception("Invalid Input - User");

            var existingUser = await _userRepository.GetAsync(new RepositoryModel<Users>() { Where = u => u.UserUpn == user.UserUpn });
            if (existingUser != null)
            {
                existingUser.LastLoginTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(existingUser);
                return existingUser;
            }

            await _userRepository.AddAsync(user);
            return user;
        }
    }
}
