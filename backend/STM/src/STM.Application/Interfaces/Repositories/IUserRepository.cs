using STM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }

}
