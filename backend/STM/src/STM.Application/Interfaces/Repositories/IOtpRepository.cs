using STM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Interfaces.Repositories
{
    public interface IOtpRepository
    {
        Task AddAsync(OtpCode otp);
        Task<OtpCode> GetLatestAsync(Guid userId);
    }


}
