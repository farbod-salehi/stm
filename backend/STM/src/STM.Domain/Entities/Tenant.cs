using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace STM.Domain.Entities
{
    public sealed class Tenant
    {
        private static readonly HashSet<string> ReservedDomains = new (StringComparer.OrdinalIgnoreCase)
        {
            "admin",
            "root",
            "system",
            "api",
            "www",
            "mail",
            "ftp",
            "smtp",
            "pop3",
            "imap",
            "webmail",
            "support",
            "helpdesk",
            "stmapp",
            "login"
        };

        private const int DomainMaxLength = 50;
        private const int DomainMinLength = 3;


        private Tenant() { /* EF Core constructor*/ }

        public Tenant(string name, string domain)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateName(name));
            errors.AddRange(ValidateDomain(domain));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            SetName(name);
            SetDomain(domain);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
        }

        public void UpdateDetails(string name, string domain)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateName(name));
            errors.AddRange(ValidateDomain(domain));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetName(name);
            SetDomain(domain);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateName(string name)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new DomainError(DomainErrors.Tenant.NameRequired));
            }

            return errors;
        }

        private static List<DomainError> ValidateDomain(string domain)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(domain))
            {
                errors.Add(new DomainError(DomainErrors.Tenant.DomainRequired));

                return errors;
            }

            domain = domain.Trim().ToLowerInvariant();

            if (domain.Length < DomainMinLength || domain.Length > DomainMaxLength)
                errors.Add(new DomainError(DomainErrors.Tenant.DomainLength, new { MinLength = DomainMinLength, MaxLength = DomainMaxLength }));

            var invalidCharacters = domain.Where(c => !char.IsLetterOrDigit(c) && c != '-').Distinct().ToArray();

            if (invalidCharacters.Length > 0)
                errors.Add(new DomainError(DomainErrors.Tenant.DomainContent, new { InvalidCharacters = String.Join(", ", invalidCharacters) }));

            if (domain.StartsWith('-') || domain.EndsWith('-') || domain.Contains("--"))
                errors.Add(new DomainError(DomainErrors.Tenant.DomainHyphenPlacement));

            if (ReservedDomains.Contains(domain))
                errors.Add(new DomainError(DomainErrors.Tenant.DomainReservedWords, new { ReservedWord = domain }));

            return errors;
        }

        private void SetName(string name)
        {
            Name = name.Trim();
        }

        private void SetDomain(string domain)
        {
            Domain = domain.Trim().ToLowerInvariant(); ;
        }
       

        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Domain { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;
    }
}
