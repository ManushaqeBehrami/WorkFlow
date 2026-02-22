using Microsoft.EntityFrameworkCore;
using WorkFlow.Data;
using WorkFlow.Models;

namespace WorkFlow.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        public Task<User?> GetByIdAsync(int id) =>
            _context.Users.FindAsync(id).AsTask();

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
