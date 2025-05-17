using UserAPI.Models;

namespace UserAPI.Repositories
{

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public InMemoryUserRepository()
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Login = "Admin",
                Password = "Admin123",
                Name = "Administrator",
                Gender = 2,
                Birthday = new DateTime(1990, 1, 1),
                Admin = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _users.Add(adminUser);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<User?> GetByLoginAsync(string login)
        {
            var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<User?> GetUserByLoginAndPasswordAsync(string login, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password &&
                u.RevokedOn == null);

            return Task.FromResult(user);
        }

        public Task<List<User>> GetAllActiveUsersAsync()
        {
            var activeUsers = _users
                .Where(u => u.RevokedOn == null)
                .OrderBy(u => u.CreatedOn)
                .ToList();

            return Task.FromResult(activeUsers);
        }

        public Task<List<User>> GetUsersOlderThanAsync(int age)
        {
            var today = DateTime.Today;
            var users = _users
                .Where(u => u.Birthday.HasValue &&
                            (today.Year - u.Birthday.Value.Year -
                            (today.Month < u.Birthday.Value.Month ||
                            (today.Month == u.Birthday.Value.Month && today.Day < u.Birthday.Value.Day) ? 1 : 0)) > age)
                .ToList();

            return Task.FromResult(users);
        }

        public Task<User> CreateAsync(User user)
        {
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<User?> UpdateAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
                return Task.FromResult<User?>(null);

            var index = _users.IndexOf(existingUser);
            _users[index] = user;

            return Task.FromResult<User?>(user);
        }

        public Task<bool> DeleteAsync(string login, string deletedBy, bool hardDelete = false)
        {
            var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
            if (user == null)
                return Task.FromResult(false);

            if (hardDelete)
            {
                _users.Remove(user);
            }
            else
            {
                user.RevokedOn = DateTime.UtcNow;
                user.RevokedBy = deletedBy;
            }

            return Task.FromResult(true);
        }

        public Task<bool> RestoreUserAsync(string login)
        {
            var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
            if (user == null)
                return Task.FromResult(false);

            user.RevokedOn = null;
            user.RevokedBy = null;

            return Task.FromResult(true);
        }
    }
}