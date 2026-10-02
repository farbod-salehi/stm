using STM.Application.DTOs.Auth;
using STM.Application.Interfaces.Repositories;
using STM.Application.Interfaces.Services;
using STM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IOtpRepository _otps;
        private readonly IEmailSender _email;
        private readonly IClock _clock;

        public AuthService(IUserRepository users, IOtpRepository otps, IEmailSender email, IClock clock)
        {
            _users = users;
            _otps = otps;
            _email = email;
            _clock = clock;
        }

        public async Task RegisterAsync(RegisterUserRequest request)
        {
            var existing = await _users.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsVerified = false
            };

            await _users.AddAsync(user);

            var otp = new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Code = new Random().Next(100000, 999999).ToString(),
                ExpiresAt = _clock.UtcNow.AddMinutes(5)
            };

            await _otps.AddAsync(otp);

            await _email.SendAsync(user.Email, "Your OTP Code", $"Your code is {otp.Code}");
        }

        public async Task VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _users.GetByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found");

            var otp = await _otps.GetLatestAsync(user.Id);
            if (otp == null || otp.IsExpired(_clock.UtcNow))
                throw new Exception("OTP expired");

            if (otp.Code != request.Code)
                throw new Exception("Invalid OTP");

            user.IsVerified = true;
            await _users.UpdateAsync(user);
        }
    }
}
