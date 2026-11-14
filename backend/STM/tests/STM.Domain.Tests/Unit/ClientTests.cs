using STM.Domain.Entities.Clients;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class ClientTests
    {
        [InlineData(null, null, null)]
        [InlineData("test@example.com", "1234567890", null)]
        [InlineData("test@example.com", "1234567890", ClientSource.Website)]
        [Theory]
        public void Client_must_be_created_and_returned_with_valid_inputs(string? email, string? phone, ClientSource? source)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            var client = new Client(tenantId, fullName, email, phone, status, timeZone, source);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(tenantId, client.TenantId);
            Assert.Equal(fullName, client.FullName);
            Assert.Equal(email, client.Email);
            Assert.Equal(phone, client.Phone);
            Assert.Equal(status, client.Status);
            Assert.Equal(timeZone, client.Timezone);
            Assert.Equal(source, client.Source);
            Assert.InRange(client.CreatedAt, beforeCreationTime, afterCreationTime);
            Assert.Null(client.UpdatedAt);
            Assert.False(client.IsDeleted);
            Assert.Null(client.DeletedAt);
            Assert.Null(client.DeletedBy);

        }

        [InlineData(null, null, null)]
        [InlineData("test@example.com", "1234567890", null)]
        [InlineData("test@example.com", "1234567890", ClientSource.Manual)]
        [Theory]
        public void Client_must_be_updated_and_returned_with_valid_inputs(string? newEmail, string? newPhone, ClientSource? newSource)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var email = "test@example.com";
            var phone = "1234567890";
            var source = ClientSource.Website;

            var client = new Client(tenantId, fullName, email, phone, status, timeZone, source);

            var createdAtBeforeUpdate = client.CreatedAt;
            var tenantIdBeforeUpdate = client.TenantId;


            var beforeUpdateTime = DateTimeOffset.UtcNow;

            var newFullName = "Updated Client";
            var newStatus = ClientStatus.Active;
            var newTimezone = "Europe/London";

            // Act
            client.Update(newFullName, newEmail, newPhone, newStatus, newTimezone, newSource);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(tenantIdBeforeUpdate, client.TenantId);
            Assert.Equal(newFullName, client.FullName);
            Assert.Equal(newEmail, client.Email);
            Assert.Equal(newPhone, client.Phone);
            Assert.Equal(newStatus, client.Status);
            Assert.Equal(newTimezone, client.Timezone);
            Assert.Equal(newSource, client.Source);
            Assert.NotNull(client.UpdatedAt);
            Assert.InRange(client.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
            Assert.Equal(createdAtBeforeUpdate, client.CreatedAt);
            Assert.False(client.IsDeleted);
            Assert.Null(client.DeletedAt);
            Assert.Null(client.DeletedBy);

        }

        [Fact]
        public void Client_must_be_deleted_with_valid_inputs()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var email = "test@example.com";
            var phone = "1234567890";
            var source = ClientSource.Website;

            var client = new Client(tenantId, fullName, email, phone, status, timeZone, source);

            var createdAtBeforeDelete = client.CreatedAt;
            var deletedBy = Guid.NewGuid();
            var beforeDeleteTime = DateTimeOffset.UtcNow;

            // Act
            client.Delete(deletedBy);

            var afterDeleteTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(tenantId, client.TenantId);
            Assert.Equal(fullName, client.FullName);
            Assert.Equal(email, client.Email);
            Assert.Equal(phone, client.Phone);
            Assert.Equal(status, client.Status);
            Assert.Equal(timeZone, client.Timezone);
            Assert.Equal(source, client.Source);
            Assert.Equal(createdAtBeforeDelete, client.CreatedAt);
            Assert.True(client.IsDeleted);
            Assert.InRange(client.DeletedAt ?? default, beforeDeleteTime, afterDeleteTime);
            Assert.Equal(deletedBy, client.DeletedBy);
        }

        [Fact]
        public void TenantId_is_required_for_client_creation()
        {
            // Arrange
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(Guid.Empty, fullName, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.TenantIdRequired, domainError.Code);

        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Theory]
        public void FullName_is_required_for_client_creation(string? fullName)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName!, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.FullNameRequired, domainError.Code);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Theory]
        public void FullName_is_required_for_client_update(string? newFullName)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;
            var fullName = "Test Client";

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(newFullName!, email, phone, status, timeZone, source));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.FullNameRequired, domainError.Code);
        }


        [Fact]
        public void FullName_length_must_be_less_than_or_equal_to_200_characters_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = new string('A', 201); // 201 characters
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.FullNameTooLong, domainError.Code);
        }


        [Fact]
        public void FullName_length_must_be_less_than_or_equal_to_200_characters_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newFullName = new string('A', 201); // 201 characters

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(newFullName, email, phone, status, timeZone, source));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.FullNameTooLong, domainError.Code);
        }

        [InlineData("test.gmail.com")]
        [InlineData("test@")]
        [InlineData("@gmail.com")]
        [Theory]
        public void Email_must_be_valid_for_client_creation(string email)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.EmailInvalidFormat, domainError.Code);
        }

        [InlineData("test.gmail.com")]
        [InlineData("test@")]
        [InlineData("@gmail.com")]
        [Theory]
        public void Email_must_be_valid_for_client_update(string newEmail)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, newEmail, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.EmailInvalidFormat, domainError.Code);
        }


        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Theory]
        public void Timezone_is_required_for_client_creation(string? timeZone)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone!, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.TimezoneRequired, domainError.Code);
        }


        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Theory]
        public void Timezone_is_required_for_client_update(string? newTimeZone)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, phone, status, newTimeZone!, source));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.TimezoneRequired, domainError.Code);
        }

        [Fact]
        public void Timezone_length_must_be_less_than_or_equal_to_100_characters_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = new string('A', 101); // 101 characters
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.TimezoneTooLong, domainError.Code);
        }

        [Fact]
        public void Timezone_length_must_be_less_than_or_equal_to_100_characters_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newTimeZone = new string('A', 101); // 101 characters

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, phone, status, newTimeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.TimezoneTooLong, domainError.Code);
        }

        [Fact]
        public void Status_must_be_valid_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;
            var status = (ClientStatus)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.StatusIsInvalid, domainError.Code);
        }

        [Fact]
        public void Status_must_be_valid_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;
            var status = ClientStatus.Lead;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newStatus = (ClientStatus)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, phone, newStatus, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.StatusIsInvalid, domainError.Code);
        }

        [Fact]
        public void Source_must_be_valid_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = (ClientSource)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.SourceIsInvalid, domainError.Code);
        }

        [Fact]
        public void Source_must_be_valid_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newSource = (ClientSource)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, phone, status, timeZone, newSource));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.SourceIsInvalid, domainError.Code);
        }

        [Fact]
        public void Phone_length_must_be_less_than_or_equal_to_50_characters_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = new string('1', 51); // 51 characters
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.PhoneTooLong, domainError.Code);
        }

        [Fact]
        public void Phone_length_must_be_less_than_or_equal_to_50_characters_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "";
            var phone = "1234567890";
            var status = ClientStatus.Lead;
            var timeZone = "Asia/Tehran";
            var source = ClientSource.Website;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newPhone = new string('1', 51); // 51 characters

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, newPhone, status, timeZone, source));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.PhoneTooLong, domainError.Code);
        }

        [Fact]
        public void Email_must_be_trimmed_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "  test@example.com  ";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            // Act
            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Assert
            Assert.Equal("test@example.com", client.Email);
        }

        [Fact]
        public void Email_must_be_trimmed_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newEmail = "  new@example.com  ";

            // Act
            client.Update(fullName, newEmail, phone, status, timeZone, source);

            // Assert
            Assert.Equal("new@example.com", client.Email);
        }

        [Fact]
        public void FullName_must_be_trimmed_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "  Test Client  ";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            // Act
            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Assert
            Assert.Equal("Test Client", client.FullName);
        }

        [Fact]
        public void FullName_must_be_trimmed_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newFullName = "  New Client  ";

            // Act
            client.Update(newFullName, email, phone, status, timeZone, source);

            // Assert
            Assert.Equal("New Client", client.FullName);
        }
 
        [Fact]
        public void Timezone_must_be_trimmed_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "  Asia/Tehran  ";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            // Act
            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Assert
            Assert.Equal("Asia/Tehran", client.Timezone);
        }

        [Fact]
        public void Timezone_must_be_trimmed_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newTimeZone = "  Europe/London  ";

            // Act
            client.Update(fullName, email, phone, status, newTimeZone, source);

            // Assert
            Assert.Equal("Europe/London", client.Timezone);
        }

        [Fact]
        public void Phone_must_be_trimmed_for_client_creation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "  1234567890  ";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            // Act
            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            // Assert
            Assert.Equal("1234567890", client.Phone);
        }

        [Fact]
        public void Phone_must_be_trimmed_for_client_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var newPhone = "  1234567890  ";

            // Act
            client.Update(fullName, email, newPhone, status, timeZone, source);

            // Assert
            Assert.Equal("1234567890", client.Phone);
        }

        [Fact]
        public void Deleted_client_cannot_be_updated()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var deletedBy = Guid.NewGuid();

            client.Delete(deletedBy);

            var newStatus = ClientStatus.Active;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(fullName, email, phone, newStatus, timeZone, source));
           
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.CannotUpdateDeletedClient, domainError.Code);
        }

        [Fact]
        public void Deleted_client_cannot_be_deleted()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var deletedBy = Guid.NewGuid();

            client.Delete(deletedBy);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Delete(deletedBy));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.IsAlreadyDeleted, domainError.Code);
        }

        [Fact]
        public void DeletedBy_is_required_for_client_delete()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);

            var deletedBy = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Delete(deletedBy));

            var error = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Client.DeletedByRequired, error.Code);
        }

        [Fact]
        public void Multiple_validation_errors_should_be_returned_for_creation()
        {
            // Arrange
            var tenantId = Guid.Empty;
            var fullName = "";
            var email = "invalid";
            var phone = new string('1', 51);
            var timeZone = "";
            var status = (ClientStatus)999;
            var source = (ClientSource)999;

            // act & assert
            var exception = Assert.Throws<BusinessRuleException>(() => new Client(tenantId, fullName, email, phone, status, timeZone, source));

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.TenantIdRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.FullNameRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.PhoneTooLong);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.TimezoneRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.StatusIsInvalid);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.SourceIsInvalid);
        }

        [Fact]
        public void Multiple_validation_errors_should_be_returned_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var fullName = "Test Client";
            var email = "test@example.com";
            var phone = "1234567890";
            var timeZone = "Asia/Tehran";
            var status = ClientStatus.Lead;
            var source = ClientSource.Referral;

            Client client = new(tenantId, fullName, email, phone, status, timeZone, source);


            var newFullName = "";
            var newEmail = "invalid";
            var newPhone = new string('1', 51);
            var newTimeZone = "";
            var newStatus = (ClientStatus)999;
            var newSource = (ClientSource)999;

            // act & assert
            var exception = Assert.Throws<BusinessRuleException>(() => client.Update(newFullName, newEmail, newPhone, newStatus, newTimeZone, newSource));

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.FullNameRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.PhoneTooLong);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.TimezoneRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.StatusIsInvalid);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Client.SourceIsInvalid);
        }
    }
}
