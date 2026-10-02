using STM.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterUserRequest request);
        Task VerifyOtpAsync(VerifyOtpRequest request);
        Task<string> LoginAsync(string email, string password);
    }
}
