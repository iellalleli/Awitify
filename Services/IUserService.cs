using KaraokeApp.Models;

namespace KaraokeApp.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(string username, string password, string email);
        Task<User> AuthenticateAsync(string username, string password);
    }
}