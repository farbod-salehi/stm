using STM.Domain.Exceptions;

namespace STM.Domain.Entities.Leads
{
    public sealed class LeadForm
    {
        private const int TitleMaxLength = 200;

        private const int DescriptionMaxLength = 500;

        private LeadForm() { /* EF Core constructor */ }

        public LeadForm(Guid tenantId, string title, string? description, string fieldsJson)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateTenantId(tenantId));
            errors.AddRange(ValidateTitle(title));
            errors.AddRange(ValidateDescription(description));
            errors.AddRange(ValidateFieldJson(fieldsJson));


            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            SetTitle(title);
            SetDescription(description);
            SetFieldsJson(fieldsJson);
            SetActive(true);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            IsDeleted = false;
            DeletedAt = null;
            DeleteByUser = null;
        }

        public void Update(string title, string description, string fieldsJson, bool isActive)
        {
            List<DomainError> errors = [];

            if(IsDeleted)
            {
                errors.Add(new DomainError(DomainErrors.LeadForm.CannotUpdateDeletedLeadForm));
                throw new BusinessRuleException(errors);
            }
                

            errors.AddRange(ValidateTitle(title));
            errors.AddRange(ValidateDescription(description));
            errors.AddRange(ValidateFieldJson(fieldsJson));


            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetTitle(title);
            SetDescription(description);
            SetFieldsJson(fieldsJson);
            SetActive(isActive);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete(Guid deletedBy)
        {
            if (deletedBy == Guid.Empty)
                throw new BusinessRuleException([new DomainError(DomainErrors.LeadForm.DeletedByIsRequired)]);

            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.LeadForm.IsAlreadyDeleted)]);

            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedAt = DateTimeOffset.UtcNow;

        }

        private static List<DomainError> ValidateTenantId(Guid tenantId)
        {
            List<DomainError> errors = [];

            if (Guid.Empty == tenantId)
                errors.Add(new DomainError(DomainErrors.LeadForm.TenantIdIsRequired)); ;

            return errors;
        }

        private static List<DomainError> ValidateTitle(string title)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(title))
                errors.Add(new DomainError(DomainErrors.LeadForm.TitleIsRequired));
            else if (title.Length > TitleMaxLength)
                errors.Add(new DomainError(DomainErrors.LeadForm.TitleIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateDescription(string? description)
        {
            List<DomainError> errors = [];

            if(!string.IsNullOrWhiteSpace(description) && description.Length > DescriptionMaxLength)
                errors.Add(new DomainError(DomainErrors.LeadForm.DescriptionIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateFieldJson(string fieldsJson)
        {
            List<DomainError> errors = [];

            if(string.IsNullOrWhiteSpace(fieldsJson))
                errors.Add(new DomainError(DomainErrors.LeadForm.FieldsJsonIsRequired));

            return errors;
        }

        private void SetTitle(string title)
        {
            Title = title.Trim();
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null: description.Trim();
        }

        private void SetFieldsJson(string fieldsJson)
        {
            FieldsJson = fieldsJson.Trim();
        }

        private void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public Guid Id { get; private set; }

        public Guid TenantId { get; private set; }

        public string Title { get; private set; } = default!;

        public string? Description { get; private set; }

        public string FieldsJson { get; private set; } = default!;

        public bool IsActive { get; private set; } = true;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public bool IsDeleted { get; private set; } = false;

        public DateTimeOffset? DeletedAt { get; private set; }

        public Guid? DeletedBy { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        /* Navigation properties */

        public Tenant Tenant { get; private set; } = default!;

        public Users.User? DeleteByUser { get; private set; }
    }
}
