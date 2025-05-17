using UserAPI.Models;

namespace UserAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByLoginAsync(string login);
        Task<User?> GetUserByLoginAndPasswordAsync(string login, string password);
        Task<List<User>> GetAllActiveUsersAsync();
        Task<List<User>> GetUsersOlderThanAsync(int age);
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<bool> DeleteAsync(string login, string deletedBy, bool hardDelete = false);
        Task<bool> RestoreUserAsync(string login);
    }

}
