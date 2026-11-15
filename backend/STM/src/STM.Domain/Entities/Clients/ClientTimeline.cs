using STM.Domain.Entities.Users;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using static STM.Domain.Exceptions.DomainErrors;

namespace STM.Domain.Entities.Clients
{
    public class ClientTimeline
    {
        private ClientTimeline(){ /* EF Core constructor */ }

        public ClientTimeline(Guid clientId, Guid? consultantId, TimelineEventType eventType, 
            DateTimeOffset occurredAt, string? metadataJson)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateClientId(clientId));
            errors.AddRange(ValidateEventType(eventType));
            errors.AddRange(ValidateOccurredAt(occurredAt));
            errors.AddRange(ValidateConsultantId(consultantId));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            ClientId = clientId;
            ConsultantId = consultantId;
            EventType = eventType;
            OccurredAt = occurredAt;
            MetadataJson = metadataJson;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateClientId(Guid clientId)
        {
            List<DomainError> errors = [];

            if (clientId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientTimeline.ClientIdRequired));

            return errors;
        }

        private static List<DomainError> ValidateEventType(TimelineEventType eventType)
        {
            List<DomainError> errors = [];

            if (!Enum.IsDefined(eventType))
                errors.Add(new DomainError(DomainErrors.ClientTimeline.EventTypeIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidateOccurredAt(DateTimeOffset occurredAt)
        {
            List<DomainError> errors = [];

            if (occurredAt == default)
                errors.Add(new DomainError(DomainErrors.ClientTimeline.OccurredAtIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidateConsultantId(Guid? consultantId)
        {
            List<DomainError> errors = [];

            if (consultantId.HasValue && consultantId.Value == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.ClientTimeline.ConsultantIdIsInvalid));

            return errors;
        }

        public Guid Id { get; private set; }

        public Guid ClientId { get; private set; }

        public Guid? ConsultantId { get; private set; }

        public TimelineEventType EventType { get; private set; }

        public DateTimeOffset OccurredAt { get; private set; }

        public string? MetadataJson { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;


        /* Navigation properties */

        public Client Client { get; private set; } = default!;

        public Users.User? Consultant { get; private set; }
    }
}
