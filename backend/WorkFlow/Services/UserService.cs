using WorkFlow.Models;
using WorkFlow.Repositories;

namespace WorkFlow.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public Task<User?> GetByEmailAsync(string email) =>
            _repo.GetByEmailAsync(email);

        public Task<User?> GetByIdAsync(int id) =>
            _repo.GetByIdAsync(id);

        public Task CreateAsync(User user) =>
            _repo.AddAsync(user);
    }
}
