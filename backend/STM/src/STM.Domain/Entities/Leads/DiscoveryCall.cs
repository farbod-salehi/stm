using STM.Domain.Exceptions;

namespace STM.Domain.Entities.Leads
{
    public sealed class DiscoveryCall
    {
        private const int ExternalEventIdMaxLength = 200;

        private const int NotesMaxLength = 5000;

        private DiscoveryCall() { /* EF Core construction */ }

        public DiscoveryCall(Guid clientId, DateTimeOffset scheduledAt, int durationMinutes, string? notes, string? externalEventId)
        {
            List<DomainError> erros = [];

            erros.AddRange(ValidateClientId(clientId));
            erros.AddRange(ValidateScheduledAt(scheduledAt));
            erros.AddRange(ValidateDurationMinutes(durationMinutes));
            erros.AddRange(ValidateNotes(notes));
            erros.AddRange(ValidateExternalEventId(externalEventId));
           
            if (erros.Count > 0)
                throw new BusinessRuleException(erros);

            Id = Guid.NewGuid();
            ClientId = clientId;
            ScheduledAt = scheduledAt;
            DurationMinutes = durationMinutes;
            Status = DiscoveryCallStatus.Scheduled;
            Outcome = null;
            SetNotes(notes);
            SetExternalEventId(externalEventId);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        public void UpdateDurationMinutes(int durationMinutes)
        {
            EnsureNotDeleted();

            if (Status != DiscoveryCallStatus.Scheduled)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.CannotEditDurationUnlessScheduled)]);

            List<DomainError> erros = ValidateDurationMinutes(durationMinutes);                 

            if (erros.Count > 0)
                throw new BusinessRuleException(erros);
    
            DurationMinutes = durationMinutes;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateNotes(string? notes)
        {
            EnsureNotDeleted();

            List<DomainError> erros = ValidateNotes(notes);

            if (erros.Count > 0)
                throw new BusinessRuleException(erros);

            SetNotes(notes);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AttachCalendarEvent(string? externalEventId)
        {
            EnsureNotDeleted();

            List<DomainError> erros = ValidateExternalEventId(externalEventId);

            if (erros.Count > 0)
                throw new BusinessRuleException(erros);

            SetExternalEventId(externalEventId);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsCompleted(DiscoveryCallOutcome outcome)
        {
            EnsureNotDeleted();

            if (Status != DiscoveryCallStatus.Scheduled)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled)]);

            var errors = ValidateOutcome(outcome);

            if(errors.Count > 0)
                throw new BusinessRuleException(errors);

            if (outcome == DiscoveryCallOutcome.NoShow)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.OutcomeIsInvalid)]);

            Status = DiscoveryCallStatus.Completed;
            Outcome = outcome;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsNoShow()
        {
            EnsureNotDeleted();

            if (Status != DiscoveryCallStatus.Scheduled)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled)]);

            Status = DiscoveryCallStatus.No_Show;
            Outcome = DiscoveryCallOutcome.NoShow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Reschedule(DateTimeOffset scheduledAt)
        {
            EnsureNotDeleted();

            if (Status != DiscoveryCallStatus.Scheduled)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.CannotRescheduleUnlessScheduled)]);

            var errors = ValidateScheduledAt(scheduledAt);

            if(errors.Count > 0)
                throw new BusinessRuleException(errors);

            ScheduledAt = scheduledAt;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete(Guid deletedBy)
        {
            if (deletedBy == Guid.Empty)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.DeletedByIsRequired)]);

            EnsureNotDeleted();

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
           
        }

        private void SetExternalEventId(string? externalEventId)
        {
            ExternalEventId = string.IsNullOrWhiteSpace(externalEventId) ? null : externalEventId.Trim();
        }

        private void SetNotes(string? notes)
        {
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        private static List<DomainError> ValidateClientId(Guid clientId)
        {
            List<DomainError> erros = [];

            if (Guid.Empty == clientId)
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.ClientIdIsRequired));

            return erros;
        }

        private static List<DomainError> ValidateScheduledAt(DateTimeOffset scheduledAt)
        {
            List<DomainError> erros = [];

            if(scheduledAt <= DateTimeOffset.UtcNow)
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.ScheduledAtIsInvalid));

            return erros;
        }

        private static List<DomainError> ValidateDurationMinutes(int durationMinutes)
        {
            List<DomainError> erros = [];

            if(durationMinutes <= 0)
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.DurationMinutesIsInvalid));

            return erros;
        }

        private static List<DomainError> ValidateOutcome(DiscoveryCallOutcome? outcome)
        {
            List<DomainError> erros = [];

            if (outcome.HasValue && !Enum.IsDefined(outcome.Value))
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.OutcomeIsInvalid));

            return erros;
        }

        private static List<DomainError> ValidateNotes(string? notes)
        {
            List<DomainError> erros = [];

            if(!string.IsNullOrWhiteSpace(notes) && notes.Trim().Length > NotesMaxLength)
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.NotesIsTooLong));

            return erros;
        }

        private static List<DomainError> ValidateExternalEventId(string? externalEventId)
        {
            List<DomainError> erros = [];

            if (!string.IsNullOrWhiteSpace(externalEventId) && externalEventId.Trim().Length > ExternalEventIdMaxLength)
                erros.Add(new DomainError(DomainErrors.DiscoveryCall.ExternalEventIdIsTooLong));

            return erros;
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall)]);
        }

        public Guid Id { get; private set; }

        public Guid ClientId { get; private set; }

        public DateTimeOffset ScheduledAt { get; private set; }

        public int DurationMinutes { get; private set; }

        public DiscoveryCallStatus Status { get; private set; }

        public DiscoveryCallOutcome? Outcome { get; private set; }

        public string? Notes { get; private set; }

        public string? ExternalEventId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTimeOffset? DeletedAt { get; private set; }

        public Guid? DeletedBy { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;


        /* Navigtion properties */

        public Clients.Client Client { get; private set; } = default!;

        public Users.User? DeletedByUser { get; private set; }
    }
}
