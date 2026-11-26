using STM.Domain.Entities.Leads;
using STM.Domain.Exceptions;

namespace STM.Domain.Tests.Unit
{
    public class DiscoveryCallTests
    {
        #region Creation

        [InlineData("call notes","valid-external-event-id")]
        [InlineData(null, null)]
        [Theory]
        public void Discovery_call_must_be_created_and_returned_with_valid_inputs(string? notes, string? externalEventId)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduledAt = DateTimeOffset.UtcNow.AddDays(3);
            var durationMinutes = 30;

            var beforeCreationDateTime = DateTimeOffset.UtcNow;

            // Act
            DiscoveryCall discoveryCall = new(clientId, scheduledAt, durationMinutes, notes, externalEventId);
            
            var afterCreationDateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, discoveryCall.Id);
            Assert.Equal(clientId, discoveryCall.ClientId);
            Assert.Equal(scheduledAt, discoveryCall.ScheduledAt);
            Assert.Equal(durationMinutes, discoveryCall.DurationMinutes);
            Assert.Equal(notes, discoveryCall.Notes);
            Assert.Equal(externalEventId, discoveryCall.ExternalEventId);
            Assert.Equal(DiscoveryCallStatus.Scheduled, discoveryCall.Status);
            Assert.Null(discoveryCall.Outcome);
            Assert.InRange(discoveryCall.CreatedAt, beforeCreationDateTime, afterCreationDateTime);
            Assert.Null(discoveryCall.UpdatedAt);
            Assert.False(discoveryCall.IsDeleted);
            Assert.Null(discoveryCall.DeletedAt);
            Assert.Null(discoveryCall.DeletedBy);
        }

        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void Empty_notes_must_be_saved_as_null(string? notes)
        {
            // Act
            var discoveryCall = CreateDiscoveryCall(notes: notes);

            // Assert
            Assert.Null(discoveryCall.Notes);
        }

        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void Empty_external_event_id_must_be_saved_as_null(string? externalEventId)
        {
            // Act
            var discoveryCall = CreateDiscoveryCall(externalEventId: externalEventId);

            // Assert
            Assert.Null(discoveryCall.ExternalEventId);
        }

        [InlineData(" valid-external-event-id ", "valid-external-event-id")]
        [InlineData("\t", null)]
        [InlineData("\n", null)]
        [Theory]
        public void External_event_id_must_be_trimmed_for_creation(string? externalEventId, string? expectedValue)
        {
            // Act
            var discoveryCall = CreateDiscoveryCall(externalEventId: externalEventId);

            // Assert
            Assert.Equal(expectedValue, discoveryCall.ExternalEventId);
        }

        [InlineData(" call notes ", "call notes")]
        [InlineData("\t", null)]
        [InlineData("\n", null)]
        [Theory]
        public void Notes_must_be_trimmed_for_creation(string? notes, string? expectedValue)
        {
            // Act
            var discoveryCall = CreateDiscoveryCall(notes: notes);

            // Assert
            Assert.Equal(expectedValue, discoveryCall.Notes);
        }

        [Fact]
        public void Client_id_is_required()
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateDiscoveryCall(clientId: Guid.Empty));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.ClientIdIsRequired, domainError.Code);
        }

        [MemberData(nameof(InvalidScheduledAt))]
        [Theory]
        public void Scheduled_at_must_be_in_future(DateTimeOffset scheduledAt)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateDiscoveryCall(scheduledAt: scheduledAt));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.ScheduledAtIsInvalid, domainError.Code);
        }

        [MemberData(nameof(InvalidDurationMinutes))]
        [Theory]
        public void Duration_minutes_must_be_positive(int durationMinutes)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateDiscoveryCall(durationMinutes: durationMinutes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.DurationMinutesIsInvalid, domainError.Code);
        }


        [Fact]
        public void Notes_length_should_not_exceed_5000_characters_for_creation()
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateDiscoveryCall(notes: new string('A', 5001)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.NotesIsTooLong, domainError.Code);
        }

        [Fact]
        public void External_event_id_length_should_not_exceed_200_characters_for_creation()
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateDiscoveryCall(externalEventId: new string('A', 201)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.ExternalEventIdIsTooLong, domainError.Code);
        }

        #endregion

        #region Duration Updates

        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [Theory]
        public void Duration_minutes_must_be_updated_with_valid_input(int newDurationMinutes)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            discoveryCall.UpdateDurationMinutes(newDurationMinutes);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newDurationMinutes, discoveryCall.DurationMinutes);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Duration_minutes_update_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.DurationMinutes;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.UpdateDurationMinutes(50));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.DurationMinutes);
        }

        [Fact]
        public void Duration_minutes_update_must_throw_exception_when_discovery_call_status_is_not_scheduled()
        {
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.NotQualified);

            var originalValue = discoveryCall.DurationMinutes;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.UpdateDurationMinutes(50));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotEditDurationUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.DurationMinutes);
        }

        #endregion

        #region Notes Updates

        [InlineData("call new notes")]
        [InlineData(null)]
        [Theory]
        public void Notes_must_be_updated_with_valid_input(string? newNotes)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            discoveryCall.UpdateNotes(newNotes);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newNotes, discoveryCall.Notes);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Notes_update_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.Notes;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.UpdateNotes(null));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Notes);
        }

        [InlineData(" new call notes ", "new call notes")]
        [InlineData("\t", null)]
        [InlineData("\n", null)]
        [Theory]
        public void Notes_must_be_trimmed_for_update(string? newNotes, string? expectedValue)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            // Act
            discoveryCall.UpdateNotes(newNotes);

            // Assert
            Assert.Equal(expectedValue, discoveryCall.Notes);
        }

        [Fact]
        public void Notes_length_should_not_exceed_5000_characters_for_update()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var originalValue = discoveryCall.Notes;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.UpdateNotes(new string('A', 5001)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.NotesIsTooLong, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Notes);
        }

        #endregion

        #region ExternalEventId Updates

        [InlineData("new-valid-external-event-id")]
        [InlineData(null)]
        [Theory]
        public void External_event_id_must_be_updated_with_valid_input(string? newExternalEvenetId)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            discoveryCall.AttachCalendarEvent(newExternalEvenetId);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newExternalEvenetId, discoveryCall.ExternalEventId);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void External_event_id_update_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.ExternalEventId;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.AttachCalendarEvent(null));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.ExternalEventId);
        }

        [InlineData(" new-valid-external-event-id ", "new-valid-external-event-id")]
        [InlineData("\t", null)]
        [InlineData("\n", null)]
        [Theory]
        public void External_event_id_must_be_trimmed_for_update(string? newExternalEventId, string? expectedValue)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            // Act
            discoveryCall.AttachCalendarEvent(newExternalEventId);

            // Assert
            Assert.Equal(expectedValue, discoveryCall.ExternalEventId);
        }

        [Fact]
        public void External_event_id_length_should_not_exceed_200_characters_for_update()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var originalValue = discoveryCall.ExternalEventId;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.AttachCalendarEvent(new string('A', 201)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.ExternalEventIdIsTooLong, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.ExternalEventId);
        }

        #endregion

        #region Status Updates

        [InlineData(DiscoveryCallOutcome.Qualified)]
        [InlineData(DiscoveryCallOutcome.NotQualified)]
        [Theory]
        public void Status_must_be_marked_completed_with_valid_input(DiscoveryCallOutcome outcome)
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            discoveryCall.MarkAsCompleted(outcome);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(DiscoveryCallStatus.Completed, discoveryCall.Status);
            Assert.Equal(outcome, discoveryCall.Outcome);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Mark_status_as_completed_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.Qualified));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void Completed_discovery_call_cannot_be_marked_as_completed_again()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.Qualified);

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.NotQualified));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void Mark_status_as_completed_with_no_show_outcome_must_throw_exception()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.NoShow));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.OutcomeIsInvalid, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void Status_must_be_marked_no_show_with_valid_input()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            discoveryCall.MarkAsNoShow();

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(DiscoveryCallStatus.No_Show, discoveryCall.Status);
            Assert.Equal(DiscoveryCallOutcome.NoShow, discoveryCall.Outcome);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Mark_status_as_no_show_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsNoShow());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void Completed_discovery_call_cannot_be_marked_as_no_show()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.Qualified);

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsNoShow());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void No_show_discovery_call_cannot_be_marked_as_completed()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsNoShow();

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.Qualified));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        [Fact]
        public void No_show_discovery_call_cannot_be_marked_as_no_show_again()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsNoShow();

            var originalValue = discoveryCall.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsNoShow());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotEditStatusUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.UpdatedAt);
        }

        [Fact]
        public void Outcome_must_have_valid_value()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var originalValue = discoveryCall.Status;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.MarkAsCompleted((DiscoveryCallOutcome)999));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.OutcomeIsInvalid, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.Status);
        }

        #endregion

        #region ScheduledAt Updates

        [Fact]
        public void Scheduled_at_must_be_updated_with_valid_input()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            var newScheduledAt = DateTimeOffset.UtcNow.AddDays(5);

            // Act
            discoveryCall.Reschedule(newScheduledAt);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newScheduledAt, discoveryCall.ScheduledAt);
            Assert.NotNull(discoveryCall.UpdatedAt);
            Assert.InRange(discoveryCall.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Scheduled_at_update_must_throw_exception_when_discovery_call_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.ScheduledAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.Reschedule(DateTimeOffset.UtcNow.AddDays(5)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.ScheduledAt);
        }

        [Fact]
        public void Scheduled_at_update_must_throw_exception_when_discovery_call_status_is_not_scheduled()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.MarkAsCompleted(DiscoveryCallOutcome.Qualified);

            var originalValue = discoveryCall.ScheduledAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.Reschedule(DateTimeOffset.UtcNow.AddDays(5)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotRescheduleUnlessScheduled, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.ScheduledAt);
        }

        #endregion

        #region Delete

        [Fact]
        public void Discovery_call_must_be_deleted_with_valid_inputs()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var beforeDeleteTime = DateTimeOffset.UtcNow;

            var deletedBy = Guid.NewGuid();

            // Act
            discoveryCall.Delete(deletedBy);

            var afterDeleteTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(discoveryCall.IsDeleted);
            Assert.Equal(deletedBy, discoveryCall.DeletedBy);
            Assert.NotNull(discoveryCall.DeletedAt);
            Assert.InRange(discoveryCall.DeletedAt ?? default, beforeDeleteTime, afterDeleteTime);
        }

        [Fact]
        public void Deleted_by_is_required_for_delete()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            var originalValue = discoveryCall.DeletedAt;
            
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.Delete(Guid.Empty));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.DeletedByIsRequired, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.DeletedAt);
        }

        [Fact]
        public void Delete_discovery_call_must_throw_exception_when_it_is_deleted()
        {
            // Arrange
            var discoveryCall = CreateDiscoveryCall();

            discoveryCall.Delete(Guid.NewGuid());

            var originalValue = discoveryCall.DeletedAt;
            
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => discoveryCall.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.DiscoveryCall.CannotUpdateDeletedDiscoveryCall, domainError.Code);
            Assert.Equal(originalValue, discoveryCall.DeletedAt);
        }

        #endregion
    

        private static DiscoveryCall CreateDiscoveryCall(Guid? clientId = null, DateTimeOffset? scheduledAt = null, int durationMinutes = 30, string? notes = "call notes", string? externalEventId = "valid-external-event-id")
        {
            return new DiscoveryCall(clientId ?? Guid.NewGuid(), scheduledAt ?? DateTimeOffset.UtcNow.AddDays(3), durationMinutes, notes, externalEventId);
        }

        public static TheoryData<DateTimeOffset> InvalidScheduledAt => 
            [
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(-3)
            ];

        public static TheoryData<int> InvalidDurationMinutes => [0, -5, -100];
    }
}
