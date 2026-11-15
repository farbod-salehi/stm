using STM.Domain.Entities.Clients;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class ClientTimelineTests
    {
        [MemberData(nameof(ValidInputs))]
        [Theory]
        public void Client_timeline_must_be_created_and_returned_when_inputs_are_valid(Guid? consultantId, string? metadataJson)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var eventType = TimelineEventType.ClientStatusChanged;
            var occurredAt = DateTimeOffset.UtcNow.AddSeconds(30);

            var beforeCreation = DateTimeOffset.UtcNow;

            // Act
            ClientTimeline clientTimeline = new(clientId, consultantId, eventType, occurredAt, metadataJson);

            var afterCreation = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, clientTimeline.Id);
            Assert.Equal(clientId, clientTimeline.ClientId);
            Assert.Equal(eventType, clientTimeline.EventType);
            Assert.Equal(occurredAt, clientTimeline.OccurredAt);
            Assert.InRange(clientTimeline.CreatedAt, beforeCreation, afterCreation);
            Assert.Equal(consultantId, clientTimeline.ConsultantId);
            Assert.Equal(metadataJson, clientTimeline.MetadataJson);
        }

        [Fact]
        public void ClientId_is_required()
        {
            // Arrange
            var clientId = Guid.Empty;
            var eventType = TimelineEventType.ClientStatusChanged;
            var occurredAt = DateTimeOffset.UtcNow.AddSeconds(30);
            Guid? consultantId = null;
            string? metadataJson = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                new ClientTimeline(clientId, consultantId, eventType, occurredAt, metadataJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientTimeline.ClientIdRequired, domainError.Code);
        }

        [Fact]
        public void Event_type_must_be_valid()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var eventType = (TimelineEventType)999;
            var occurredAt = DateTimeOffset.UtcNow.AddSeconds(30);
            Guid? consultantId = null;
            string? metadataJson = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                new ClientTimeline(clientId, consultantId, eventType, occurredAt, metadataJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientTimeline.EventTypeIsInvalid, domainError.Code);
        }

        [Fact]
        public void Occurred_at_must_be_valid()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var eventType = TimelineEventType.ConsultantDocumentDeleted;
            DateTimeOffset occurredAt = default;
            Guid? consultantId = null;
            string? metadataJson = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                new ClientTimeline(clientId, consultantId, eventType, occurredAt, metadataJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientTimeline.OccurredAtIsInvalid, domainError.Code);
        }

        [Fact]
        public void Consultant_id_must_be_valid()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var eventType = TimelineEventType.ConsultantDocumentDeleted;
            var occurredAt = DateTimeOffset.UtcNow.AddSeconds(30);
            Guid? consultantId = Guid.Empty;
            string? metadataJson = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                new ClientTimeline(clientId, consultantId, eventType, occurredAt, metadataJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientTimeline.ConsultantIdIsInvalid, domainError.Code);
        }

        public static TheoryData<Guid?, string?> ValidInputs => new()
        {
            { null, null },
            { Guid.NewGuid(), null },
            { null, string.Empty },
            { Guid.NewGuid(), string.Empty },
            { null, "{}" },
            { Guid.NewGuid(), "{}" },
            { null, """{"source":"Website"}""" },
        };
    }
}
