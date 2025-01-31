using KaraokeApp.Data;
using KaraokeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KaraokeApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> CreateAsync(User user)
        {
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;
            
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}