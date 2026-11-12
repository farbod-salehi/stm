using STM.Domain.Entities.Users;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace STM.Domain.Entities.Clients
{
    public sealed class Client
    {

        private const int MaxTimezoneLength = 100;
        private const int MaxPhoneLength = 50;
        private const int MaxFullNameLength = 200;

        private Client() { /* EF Core constructor*/ }

        public Client(Guid tenantId, string fullName, string? email, string? phone, ClientStatus status, string timezone, ClientSource? source)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateTenantId(tenantId));
            errors.AddRange(ValidateFullName(fullName));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidateStatus(status));
            errors.AddRange(ValidateTimezone(timezone));
            errors.AddRange(ValidateSource(source));
            errors.AddRange(ValidatePhone(phone));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            SetFullName(fullName);
            SetEmail(email);
            SetSource(source);
            SetStatus(status);
            SetTimezone(timezone);
            SetPhone(phone);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        public void Update(string fullName, string? email, string? phone, ClientStatus status, string timezone, ClientSource? source)
        {
            if (IsDeleted)
            {
                throw new BusinessRuleException([new DomainError(DomainErrors.Client.CannotUpdateDeletedClient)]);
            }

            List<DomainError> errors = [];

            errors.AddRange(ValidateFullName(fullName));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidateStatus(status));
            errors.AddRange(ValidateTimezone(timezone));
            errors.AddRange(ValidateSource(source));
            errors.AddRange(ValidatePhone(phone));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetFullName(fullName);
            SetEmail(email);
            SetPhone(phone);
            SetStatus(status);
            SetTimezone(timezone);
            SetSource(source);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateTenantId(Guid tenantId)
        {
            List<DomainError> errors = [];

            if (tenantId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.Client.TenantIdRequired));

            return errors;
        }

        private static List<DomainError> ValidateFullName(string fullName)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(fullName))
                errors.Add(new DomainError(DomainErrors.Client.FullNameRequired));
            else if (fullName.Trim().Length > MaxFullNameLength)
                errors.Add(new DomainError(DomainErrors.Client.FullNameTooLong));

            return errors;
        }

        private static List<DomainError> ValidateEmail(string? email)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    _ = new MailAddress(email.Trim());
                }
                catch (FormatException)
                {
                    errors.Add(new DomainError(DomainErrors.Client.EmailInvalidFormat));
                }
            }

            return errors;
        }

        private static List<DomainError> ValidateTimezone(string timezone)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(timezone))
            {
                errors.Add(new DomainError(DomainErrors.Client.TimezoneRequired));

                return errors;
            }
                

            timezone = timezone.Trim();

            if (timezone.Length > MaxTimezoneLength)
            {
                errors.Add(new DomainError(DomainErrors.Client.TimezoneTooLong));

                return errors;
            }

            return errors;
        }

        private static List<DomainError> ValidateStatus(ClientStatus status)
        {
            List<DomainError> errors = [];

            if (!Enum.IsDefined(status))
                errors.Add(new DomainError(DomainErrors.Client.StatusIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidateSource(ClientSource? source)
        {
            List<DomainError> errors = [];

            if (source.HasValue && !Enum.IsDefined(source.Value))
                errors.Add(new DomainError(DomainErrors.Client.SourceIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidatePhone(string? phone)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim();

                if (phone.Length > MaxPhoneLength)
                    errors.Add(new DomainError(DomainErrors.Client.PhoneTooLong));
            }

            return errors;
        }

        private void SetEmail(string? email)
        {
            Email = email?.Trim();
        }

        private void SetFullName(string fullName)
        {
            FullName = fullName.Trim();
        }

        private void SetTimezone(string timezone)
        {
            Timezone = timezone.Trim();
        }

        private void SetStatus(ClientStatus status)
        {
            Status = status;
        }

        private void SetSource(ClientSource? source)
        {
            Source = source;
        }

        private void SetPhone(string? phone)
        {
            Phone = phone?.Trim();
        }

        public void SetDeleted(Guid deletedBy)
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Client.IsAlreadyDeleted)]);
            
            if (deletedBy == Guid.Empty)
                throw new BusinessRuleException([new DomainError(DomainErrors.Client.DeletedByRequired)]);

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
        }

        public Guid Id { get; private set; }

        public Guid TenantId { get; private set; }

        public string FullName { get; private set; } = string.Empty;

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        public ClientStatus Status { get; private set; }

        public string Timezone { get; private set; } = "UTC";

        public ClientSource? Source { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public bool IsDeleted { get; private set; } = false;

        public DateTimeOffset? DeletedAt { get; private set; }

        public Guid? DeletedBy { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;

        /* Navigation properties */

        public Tenant Tenant { get; private set; } = default!;

        public User? DeletedByUser { get; private set; }

    }
}
