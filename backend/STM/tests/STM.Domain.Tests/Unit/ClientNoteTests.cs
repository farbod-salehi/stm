using STM.Domain.Entities.Clients;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class ClientNoteTests
    {
        [Fact]
        public void Client_note_must_be_created_and_returned_with_valid_inputs()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var consultantId = Guid.NewGuid();
            var content = "Client note content";

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            ClientNote clientNote = new(clientId, consultantId, content);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, clientNote.Id);
            Assert.Equal(clientId, clientNote.ClientId);
            Assert.Equal(consultantId, clientNote.ConsultantId);
            Assert.Equal(content, clientNote.Content);
            Assert.InRange(clientNote.CreatedAt, beforeCreationTime, afterCreationTime);
            Assert.Null(clientNote.UpdatedAt);
        }

        [Fact]
        public void Content_of_client_note_must_be_updated_with_valid_value()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var consultantId = Guid.NewGuid();
            var content = "Client note content";
            var newContent = "Client note new content";

            ClientNote clientNote = new(clientId, consultantId, content);

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            clientNote.UpdateContent(newContent);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newContent, clientNote.Content);
            Assert.NotNull(clientNote.UpdatedAt);
            Assert.InRange(clientNote.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
        }

        [Fact]
        public void Client_id_is_required()
        {
            // Arrange
            var clientId = Guid.Empty;
            var consultantId = Guid.NewGuid();
            var content = "Client note content";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientNote(clientId, consultantId, content));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientNote.ClientIdIsRequired, domainError.Code);
            
        }

        [Fact]
        public void Consultant_id_is_required()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var consultantId = Guid.Empty;
            var content = "Client note content";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientNote(clientId, consultantId, content));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientNote.ConsultantIdIsRequired, domainError.Code);

        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [Theory]
        public void Content_is_required(string? content)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var consultantId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientNote(clientId, consultantId, content!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientNote.ContentIsRequired, domainError.Code);

        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [Theory]
        public void Content_is_required_for_update(string? newContent)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var consultantId = Guid.NewGuid();
            var content = "Client note content";

            ClientNote clientNote = new(clientId, consultantId, content);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => clientNote.UpdateContent(newContent!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientNote.ContentIsRequired, domainError.Code);
        }

        [Fact]
        public void Multiple_validation_errors_must_be_returned()
        {
            // Arrange
            var clientId = Guid.Empty;
            var consultantId = Guid.Empty;
            var content = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientNote(clientId, consultantId, content));

            Assert.Equal(3, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientNote.ClientIdIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientNote.ConsultantIdIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientNote.ContentIsRequired);

        }

    }
}
