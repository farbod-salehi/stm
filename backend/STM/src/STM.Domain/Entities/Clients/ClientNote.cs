using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static STM.Domain.Exceptions.DomainErrors;

namespace STM.Domain.Entities.Clients
{
    public sealed class ClientNote
    {
        private ClientNote() { /* EF Core constructor */ }

        public ClientNote(Guid clientId, Guid consultantId, string content)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateClientId(clientId));
            errors.AddRange(ValidateConsultantId(consultantId));
            errors.AddRange(ValidateContent(content));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            ClientId = clientId;
            ConsultantId = consultantId;
            Content = content;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateContent(string content)
        {
            List<DomainError> errors = ValidateContent(content);

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Content = content;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateClientId(Guid clientId)
        {
            List<DomainError> errors = [];

            if (clientId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientNote.ClientIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateConsultantId(Guid consultantId)
        {
            List<DomainError> errors = [];

            if (consultantId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientNote.ConsultantIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateContent(string content)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(content))
                errors.Add(new DomainError(DomainErrors.ClientNote.ContentIsRequired));

            return errors;
        }

        public Guid Id { get; private set; }

        public Guid ClientId { get; private set; }

        public Guid ConsultantId { get; private set; }

        public string Content { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;

        /* Navigation properties */

        public Client Client { get; private set; } = default!;

        public Users.User Consultant { get; private set; } = default!;
    }
}
