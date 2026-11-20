using STM.Domain.Entities.Leads;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class LeadFormTests
    {
        [InlineData(null)]
        [InlineData("lead form description")]
        [Theory]
        public void Lead_form_must_be_created_and_returned_with_valid_inputs(string? description)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var fieldsJson = "valid-fields-json";

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, leadForm.Id);
            Assert.Equal(tenantId, leadForm.TenantId);
            Assert.Equal(title, leadForm.Title);
            Assert.Equal(description, leadForm.Description);
            Assert.Equal(fieldsJson, leadForm.FieldsJson);
            Assert.InRange(leadForm.CreatedAt, beforeCreationTime, afterCreationTime);
            Assert.True(leadForm.IsActive);
            Assert.Null(leadForm.UpdatedAt);
            Assert.Null(leadForm.DeletedAt);
            Assert.False(leadForm.IsDeleted);
            Assert.Null(leadForm.DeletedBy);
        }

        [InlineData(null, true)]
        [InlineData("lead form new description", false)]
        [Theory]
        public void Lead_form_must_be_updated_with_valid_inputs(string? newDescription, bool isActive)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newTitle = "lead form new title";
            var newFieldsJson = "new-valid-fields-json";

            var createdAtBeforeUpdate = leadForm.CreatedAt;
            var idBeforeUpdate = leadForm.Id;
            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            leadForm.Update(newTitle, newDescription!, newFieldsJson, isActive);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(idBeforeUpdate, leadForm.Id);
            Assert.Equal(tenantId, leadForm.TenantId);
            Assert.Equal(newTitle, leadForm.Title);
            Assert.Equal(newDescription, leadForm.Description);
            Assert.Equal(newFieldsJson, leadForm.FieldsJson);
            Assert.Equal(createdAtBeforeUpdate, leadForm.CreatedAt);
            Assert.Equal(isActive, leadForm.IsActive);
            Assert.NotNull(leadForm.UpdatedAt);
            Assert.InRange(leadForm.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
            Assert.Null(leadForm.DeletedAt);
            Assert.False(leadForm.IsDeleted);
            Assert.Null(leadForm.DeletedBy);

        }

        [Fact]
        public void Lead_form_must_be_deleted_with_valid_values()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var createdAtBeforeDelete = leadForm.CreatedAt;
            var beforeDeleteTime = DateTimeOffset.UtcNow;

            var deletedBy = Guid.NewGuid();

            // Act
            leadForm.Delete(deletedBy);

            var afterDeleteTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(tenantId, leadForm.TenantId);
            Assert.Equal(title, leadForm.Title);
            Assert.Equal(description, leadForm.Description);
            Assert.Equal(fieldsJson, leadForm.FieldsJson);
            Assert.Equal(createdAtBeforeDelete, leadForm.CreatedAt);
            Assert.True(leadForm.IsDeleted);
            Assert.NotNull(leadForm.DeletedAt);
            Assert.InRange(leadForm.DeletedAt ?? default, beforeDeleteTime, afterDeleteTime);
            Assert.Equal(deletedBy, leadForm.DeletedBy);
        }

        [Fact]
        public void Tenant_id_is_required()
        {
            // Arrange
            var tenantId = Guid.Empty;
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title, description, fieldsJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.TenantIdIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Title_is_required(string? title)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title!, description, fieldsJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.TitleIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Title_is_required_for_update(string? newTitle)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Update(newTitle!, description, fieldsJson, false));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.TitleIsRequired, domainError.Code);
        }

        [Fact]
        public void Title_length_must_not_exceed_200_characters()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = new string('A', 201);
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title, description, fieldsJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.TitleIsTooLong, domainError.Code);
        }

        [Fact]
        public void Title_length_must_not_exceed_200_characters_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newTitle = new string('A', 201);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Update(newTitle, description, fieldsJson, true));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.TitleIsTooLong, domainError.Code);
        }

        [Fact]
        public void Description_length_must_not_exceed_500_characters()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = new string('A', 501);
            var fieldsJson = "valid-fields-json";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title, description, fieldsJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.DescriptionIsTooLong, domainError.Code);
        }

        [Fact]
        public void Description_length_must_not_exceed_500_characters_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newDescription = new string('A', 501);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Update(title, newDescription, fieldsJson, true));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.DescriptionIsTooLong, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Fields_json_is_required(string? fieldsJson)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";


            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title, description, fieldsJson!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.FieldsJsonIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Fields_json_is_required_for_update(string? newFieldsJson)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Update(title, description, newFieldsJson!, false));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.FieldsJsonIsRequired, domainError.Code);
        }


        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void Empty_description_must_be_saved_as_null(string? description)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var fieldsJson = "valid-fields-json";

            // Act
            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Assert
            Assert.Null(leadForm.Description);
        }

        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void Empty_description_must_be_saved_as_null_for_update(string? newDescription)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Act
            leadForm.Update(title, newDescription!, fieldsJson, true);

            // Assert
            Assert.Null(leadForm.Description);
        }

        [Fact]
        public void Title_must_be_trimmed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = " lead form title ";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            // Act
            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Assert
            Assert.Equal("lead form title", leadForm.Title);
        }

        [Fact]
        public void Title_must_be_trimmed_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newTitle = " lead form new title ";

            // Act
            leadForm.Update(newTitle, description, fieldsJson, false);

            // Assert
            Assert.Equal("lead form new title", leadForm.Title);
        }

        [Fact]
        public void Description_must_be_trimmed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = " lead form description ";
            var fieldsJson = "valid-fields-json";

            // Act
            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Assert
            Assert.Equal("lead form description", leadForm.Description);
        }

        [Fact]
        public void Description_must_be_trimmed_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newDescription = " lead form new description ";

            // Act
            leadForm.Update(title, newDescription, fieldsJson, false);

            // Assert
            Assert.Equal("lead form new description", leadForm.Description);
        }

        [Fact]
        public void Fields_json_must_be_trimmed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = " valid-fields-json ";

            // Act
            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            // Assert
            Assert.Equal("valid-fields-json", leadForm.FieldsJson);
        }

        [Fact]
        public void Fields_json_must_be_trimmed_for_update()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = "valid-fields-json";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var newFieldsJson = " new-valid-fields-json ";

            // Act
            leadForm.Update(title, description, newFieldsJson, false);

            // Assert
            Assert.Equal("new-valid-fields-json", leadForm.FieldsJson);
        }


        [Fact]
        public void Cannot_update_deleted_lead_form()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = " valid-fields-json ";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            leadForm.Delete(Guid.NewGuid());

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Update(title, description, fieldsJson, false));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.CannotUpdateDeletedLeadForm, domainError.Code);

        }

        [Fact]
        public void Cannot_delete_already_deleted_lead_form()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = " valid-fields-json ";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            leadForm.Delete(Guid.NewGuid());

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.IsAlreadyDeleted, domainError.Code);
        }

        [Fact]
        public void Deleted_by_is_required_for_delete()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var title = "lead form title";
            var description = "lead form description";
            var fieldsJson = " valid-fields-json ";

            LeadForm leadForm = new(tenantId, title, description, fieldsJson);

            var deletedBy = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => leadForm.Delete(deletedBy));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadForm.DeletedByIsRequired, domainError.Code);
        }


        [Fact]
        public void Multiple_validation_errors_must_be_returned()
        {
            // Arrange
            var tenantId = Guid.Empty;
            var title = " ";
            var description = new string('A', 501);
            string? fieldsJson = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadForm(tenantId, title, description, fieldsJson!));

            Assert.Equal(4, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.LeadForm.TenantIdIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.LeadForm.TitleIsRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.LeadForm.DescriptionIsTooLong);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.LeadForm.FieldsJsonIsRequired);

        }
    }
}
