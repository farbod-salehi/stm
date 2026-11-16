using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Entities.Clients
{
    public sealed class ClientDocument
    {
        private const int FileUrlMaxLength = 500;

        private ClientDocument() { /* EF Core constructor */ }

        public ClientDocument(Guid clientId, Guid uploadedByUserId, string fileUrl, ClientDocumentType type)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateClientId(clientId));
            errors.AddRange(ValidateUploadedBy(uploadedByUserId));
            errors.AddRange(ValidateFileUrl(fileUrl));
            errors.AddRange(ValidateType(type));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            ClientId = clientId;
            UploadedByUserId = uploadedByUserId;
            FileUrl = fileUrl;
            Type = type;
            UploadedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
        }

        public void Update(string fileUrl, ClientDocumentType type)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateFileUrl(fileUrl));
            errors.AddRange(ValidateType(type));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            FileUrl = fileUrl;
            Type = type;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateClientId(Guid clientId)
        {
            List<DomainError> errors = [];

            if (clientId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientDocument.ClientIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateUploadedBy(Guid uploadedByUserId)
        {
            List<DomainError> errors = [];

            if (uploadedByUserId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientDocument.UploadedByUserIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateFileUrl(string fileUrl)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(fileUrl))
                errors.Add(new DomainError(DomainErrors.ClientDocument.FileUrlIsRequired));
            else if(fileUrl.Length > FileUrlMaxLength)
                errors.Add(new DomainError(DomainErrors.ClientDocument.FileUrlIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateType(ClientDocumentType type)
        {
            List<DomainError> errors = [];

            if (!Enum.IsDefined(type))
                errors.Add(new DomainError(DomainErrors.ClientDocument.TypeIsInvalid));

            return errors;
        }

        public Guid Id { get; private set; }

        public Guid ClientId { get; private set; }

        public string FileUrl { get; private set; } = default!;

        public ClientDocumentType Type { get; private set; }

        public DateTimeOffset UploadedAt { get; private set; }

        public Guid UploadedByUserId { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;

        /* Navigation properties */

        public Client Client { get; private set; } = default!;

        public Users.User UploadedByUser { get; private set; } = default!;
    }
}
