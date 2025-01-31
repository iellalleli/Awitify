using KaraokeApp.Models;
using KaraokeApp.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace KaraokeApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterUserAsync(string username, string password, string email)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null) return false;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateAsync(user);
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return null;

            var hashedPassword = HashPassword(password);
            return user.PasswordHash == hashedPassword ? user : null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}