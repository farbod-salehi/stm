using STM.Domain.Entities.Clients;
using STM.Domain.Exceptions;


namespace STM.Domain.Tests.Unit
{
    public class ClientDocumentTests
    {
        [Fact]
        public void Client_document_must_be_created_and_returned_with_valid_inputs()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            ClientDocument clientDocument = new(clientId, uploadedByUserId, fileUrl, type);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, clientDocument.Id);
            Assert.Equal(clientId, clientDocument.ClientId);
            Assert.Equal(uploadedByUserId, clientDocument.UploadedByUserId);
            Assert.Equal(fileUrl, clientDocument.FileUrl);
            Assert.Equal(type, clientDocument.Type);
            Assert.InRange(clientDocument.UploadedAt, beforeCreationTime, afterCreationTime);
            Assert.Null(clientDocument.UpdatedAt);
        }

        [Fact]
        public void Client_document_must_be_updated_with_valid_inputs()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;

            var newFileUrl = "/Uploads/type/newName.pdf";
            ClientDocumentType newType = ClientDocumentType.Onboarding;

            ClientDocument clientDocument = new(clientId, uploadedByUserId, fileUrl, type);

            var uploadedAtBeforeUpdate = clientDocument.UploadedAt;

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            clientDocument.Update(newFileUrl, newType);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newFileUrl, clientDocument.FileUrl);
            Assert.Equal(newType, clientDocument.Type);
            Assert.NotNull(clientDocument.UpdatedAt);
            Assert.InRange(clientDocument.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
            Assert.Equal(uploadedAtBeforeUpdate, clientDocument.UploadedAt);
        }

        [Fact]
        public void Client_id_is_required()
        {
            // Arrange
            var clientId = Guid.Empty;
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.ClientIdIsRequired, domainError.Code);

        }

        [Fact]
        public void Uploaded_by_user_id_is_required()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.Empty;
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.UploadedByUserIdIsRequired, domainError.Code);

        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [Theory]
        public void File_url_is_required(string? fileUrl)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            ClientDocumentType type = ClientDocumentType.Invoice;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl!, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.FileUrlIsRequired, domainError.Code);

        }

        [Fact]
        public void File_url_length_must_not_exceed_500_characters()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = new String('A', 501);
            ClientDocumentType type = ClientDocumentType.Invoice;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.FileUrlIsTooLong, domainError.Code);

        }


        [Fact]
        public void Type_must_be_valid()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = (ClientDocumentType)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.TypeIsInvalid, domainError.Code);

        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [Theory]
        public void File_url_is_required_for_update(string? newFileUrl)
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;

            ClientDocument clientDocument = new(clientId, uploadedByUserId, fileUrl, type);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => clientDocument.Update(newFileUrl!, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.FileUrlIsRequired, domainError.Code);

        }

        [Fact]
        public void File_url_length_must_not_exceed_500_characters_for_update()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;
            var newFileUrl = new String('A', 501);

            ClientDocument clientDocument = new(clientId, uploadedByUserId, fileUrl, type);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => clientDocument.Update(newFileUrl, type));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.FileUrlIsTooLong, domainError.Code);

        }


        [Fact]
        public void Type_must_be_valid_for_update()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var uploadedByUserId = Guid.NewGuid();
            var fileUrl = "/Uploads/type/name.pdf";
            ClientDocumentType type = ClientDocumentType.Invoice;
            ClientDocumentType newType = (ClientDocumentType)999;

            ClientDocument clientDocument = new(clientId, uploadedByUserId, fileUrl, type);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => clientDocument.Update(fileUrl, newType));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.ClientDocument.TypeIsInvalid, domainError.Code);

        }

       

        [Fact]
        public void Multiple_validation_errors_must_be_returned()
        {
            // Arrange
            var clientId = Guid.Empty;
            var uploadedByUserId = Guid.Empty;
            var fileUrl = string.Empty;
            ClientDocumentType type = (ClientDocumentType)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new ClientDocument(clientId, uploadedByUserId, fileUrl, type));

            Assert.Equal(4, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientDocument.ClientIdIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientDocument.UploadedByUserIdIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientDocument.FileUrlIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.ClientDocument.TypeIsInvalid);

        }

    }
}
