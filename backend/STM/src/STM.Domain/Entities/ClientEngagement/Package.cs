using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static STM.Domain.Exceptions.DomainErrors;

namespace STM.Domain.Entities.ClientEngagement
{
    public sealed class Package
    {
        private const int NameMaxLength = 200;

        private const int DescriptionMaxLength = 5000;

        private const decimal MaxPrice = 99999999.99m;

        private Package() { /* EF Core constructor */ }

        public Package(Guid tenantId, string name, string? description, decimal price, int sessionsIncluded)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateTenantId(tenantId));
            errors.AddRange(ValidateName(name));
            errors.AddRange(ValidateDescription(description));
            errors.AddRange(ValidatePrice(price));
            errors.AddRange(ValidateSessionsIncluded(sessionsIncluded));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            SetName(name);
            SetDescription(description);
            Price = price;
            SessionsIncluded = sessionsIncluded;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        public void UpdateDetails(string name, string? description, decimal price, int sessionsIncluded)
        {
            if(IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Package.CannotUpdateDeletedPackage)]);

            List<DomainError> errors = [];

            errors.AddRange(ValidateName(name));
            errors.AddRange(ValidateDescription(description));
            errors.AddRange(ValidatePrice(price));
            errors.AddRange(ValidateSessionsIncluded(sessionsIncluded));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetName(name);
            SetDescription(description);
            Price = price;
            SessionsIncluded = sessionsIncluded;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete(Guid deletedBy)
        {
            if(IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Package.IsAlreadyDeleted)]);

            if(deletedBy == Guid.Empty)
                throw new BusinessRuleException([new DomainError(DomainErrors.Package.DeletedByIsRequired)]);

            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedAt = DateTimeOffset.UtcNow;

        }

        private static List<DomainError> ValidateTenantId(Guid tenantId)
        {
            List<DomainError> errors = [];

            if (tenantId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.Package.TenantIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateName(string name)
        {
            List<DomainError> errors = [];

            if(string.IsNullOrWhiteSpace(name))
                errors.Add(new DomainError(DomainErrors.Package.NameIsRequired));
            else if(name.Trim().Length > NameMaxLength)
                errors.Add(new DomainError(DomainErrors.Package.NameIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateDescription(string? description)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > DescriptionMaxLength)
                errors.Add(new DomainError(DomainErrors.Package.DescriptionIsTooLong));
           
            return errors;
        }

        private static List<DomainError> ValidatePrice(decimal price)
        {
            List<DomainError> errors = [];

            if(price <= 0 || price > MaxPrice)
                errors.Add(new DomainError(DomainErrors.Package.PriceIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidateSessionsIncluded(int sessionsIncluded)
        {
            List<DomainError> errors = [];

            if (sessionsIncluded <= 0)
                errors.Add(new DomainError(DomainErrors.Package.SessionsIncludedIsInvalid));

            return errors;
        }

        private void SetName(string name)
        {
            Name = name.Trim();
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        public Guid Id { get; private set; }

        public Guid TenantId { get; private set; }

        public string Name { get; private set; } = default!;

        public string? Description { get; private set; }

        public decimal Price { get; private set; }

        public int SessionsIncluded { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTimeOffset? DeletedAt { get; private set; }

        public Guid? DeletedBy { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;


        /* Navigtion properties */

        private readonly List<Proposal> _proposals = [];

        public Tenant Tenant { get; private set; } = default!;

        public Users.User? DeletedByUser { get; private set; }

        public IReadOnlyCollection<Proposal> Proposals => _proposals;
    }
}
