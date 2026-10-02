using STM.Application.Interfaces.Repositories;
using STM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly STMDbContext _db;

        public UserRepository(STMDbContext db) => _db = db;

        public Task<User> GetByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(x => x.Email == email);

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
