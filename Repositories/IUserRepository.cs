using KaraokeApp.Models;

namespace KaraokeApp.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }
}