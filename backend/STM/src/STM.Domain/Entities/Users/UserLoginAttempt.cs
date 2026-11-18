using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace STM.Domain.Entities.Users
{
    public sealed class UserLoginAttempt
    {
        private const int EmailMaxLength = 256;
        private const int IpAddressMaxLength = 64;
        private const int UserAgentMaxLength = 512;
        private const int DeviceIdMaxLength = 64;

        private UserLoginAttempt() { /* EF Core constructor */ }

        public UserLoginAttempt(Guid? userId, string email, bool isSuccessful, 
            string? ipAddress, string? userAgent, string? deviceId)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateUserId(userId));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidateIpAddress(ipAddress));
            errors.AddRange(ValidateUserAgent(userAgent));
            errors.AddRange(ValidateDeviceId(deviceId));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            UserId = userId;
            Email = email.Trim();
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
            DeviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId.Trim();
            IsSuccessful = isSuccessful;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateUserId(Guid? userId)
        {
            List<DomainError> errors = [];

            if (userId.HasValue && userId.Value == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.UserIdIsInvalid));
            
            return errors;
        }

        private static List<DomainError> ValidateEmail(string email)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.EmailIsRequired));

                return errors;
            }

            email = email.Trim();

            if (email.Length > EmailMaxLength)
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.EmailIsTooLong));
            else
            {
                try
                {
                    _ = new MailAddress(email);
                }
                catch
                {
                    errors.Add(new DomainError(DomainErrors.UserLoginAttempt.EmailInvalidFormat));
                }
            } 

            return errors;
        }

        private static List<DomainError> ValidateIpAddress(string? ipAddress)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Trim().Length > IpAddressMaxLength)
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.IpAddressIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateUserAgent(string? userAgent)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(userAgent) && userAgent.Trim().Length > UserAgentMaxLength)
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.UserAgentIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateDeviceId(string? deviceId)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(deviceId) && deviceId.Trim().Length > DeviceIdMaxLength)
                errors.Add(new DomainError(DomainErrors.UserLoginAttempt.DeviceIdIsTooLong));

            return errors;
        }

        public Guid Id { get; private set; }

        public Guid? UserId { get; private set; }

        public string Email { get; private set; } = default!;

        public bool IsSuccessful { get; private set; }

        public string? IpAddress { get; private set; }

        public string? UserAgent { get; private set; }

        public string? DeviceId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        /* Navigation properties */

        public Users.User? User { get; private set; }
    }
}
