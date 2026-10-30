using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace STM.Domain.Entities.Users
{
    public sealed class User
    {
        private User() { /* EF Core constructor*/ }

        private User(Guid tenantId, string email, string passwordHash, string fullName, UserRole role) 
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateRole(role));    
            errors.AddRange(ValidateTenantId(tenantId));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidatePasswordHash(passwordHash));
            errors.AddRange(ValidateFullName(fullName));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            SetEmail(email);
            SetFullName(fullName);
            SetRole(role);
            SetActive(true);
            PasswordHash = passwordHash;
            IsVerified = true;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            LastLoginAt = null;

        }

        public static User CreateAdmin(Guid tenantId, string email, string passwordHash, string fullName)
        {
            return new(tenantId, email, passwordHash, fullName, UserRole.Admin);
        }

        public static User CreateStaff(Guid tenantId, string email, string passwordHash, string fullName, UserRole role)
        {
            List<DomainError> errors = ValidateStaffRole(role);

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            return new(tenantId, email, passwordHash, fullName, role);
        }

        public void UpdateAdminDetails(string email, string fullName)
        {
            List<DomainError> errors = [];
         
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidateFullName(fullName));
            
            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetEmail(email);
            SetFullName(fullName);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateStaffDetails(string email, string fullName, bool isActive, UserRole role)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateRole(role));
            errors.AddRange(ValidateStaffRole(role));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidateFullName(fullName));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetEmail(email);
            SetFullName(fullName);
            SetRole(role);
            SetActive(isActive);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateTenantId(Guid tenantId)
        {
            List<DomainError> errors = [];

            if (tenantId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.User.TenantIdRequired));

            return errors;
        }

        private static List<DomainError> ValidateEmail(string email)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new DomainError(DomainErrors.User.EmailRequired));
                
                return errors;
            }

            try
            {
                _ = new MailAddress(email.Trim());
            }
            catch
            {
                errors.Add(new DomainError(DomainErrors.User.EmailInvalidFormat));
            }

            return errors;
        }

        private static List<DomainError> ValidatePasswordHash(string passwordHash)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                errors.Add(new DomainError(DomainErrors.User.PasswordHashRequired));
            }

            return errors;
        }

        private static List<DomainError> ValidateFullName(string fullName)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(fullName))
            {
                errors.Add(new DomainError(DomainErrors.User.FullNameRequired));
            }

            return errors;
        }

        private static List<DomainError> ValidateRole(UserRole role)
        {
            List<DomainError> errors = [];

            if (!Enum.IsDefined(role))
            {
                errors.Add(new DomainError(DomainErrors.User.RoleIsInvalid));
            }

            return errors;
        }

        private static List<DomainError> ValidateStaffRole(UserRole role)
        {
            List<DomainError> errors = [];

            if (role == UserRole.Admin)
            {
                errors.Add(new DomainError(DomainErrors.User.RoleCannotBeAdmin));
            }

            return errors;
        }

        private void SetEmail(string email)
        {
            Email = email.Trim().ToLowerInvariant();
        }

        private void SetFullName(string fullName)
        {
            FullName = fullName.Trim();
        }

        private void SetRole(UserRole role)
        {
            Role = role;
        }

        private void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }

        public Guid TenantId { get; private set; }

        public string Email { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public string FullName { get; private set; } = string.Empty;

        public UserRole Role { get; private set; }

        public bool IsVerified { get; private set; } = false;

        public bool IsActive { get; private set; } = true;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; } = default;

        public byte[] RowVersion { get; private set; } = default!;

    }
}
