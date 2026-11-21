using STM.Domain.Entities.Leads;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class LeadFormSubmissionTests
    {
        [InlineData("test@gmail.com", "valid-phone")]
        [InlineData(null, null)]
        [Theory]
        public void Lead_form_submission_must_be_created_and_returned_with_valid_inputs(string? email, string? phone)
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";

            var beforeSubmissionTime = DateTimeOffset.UtcNow;

            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            var afterSubmissionTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, leadFormSubmission.Id);
            Assert.Equal(leadFormId, leadFormSubmission.LeadFormId);
            Assert.Equal(fullName, leadFormSubmission.FullName);
            Assert.Equal(answersJson, leadFormSubmission.AnswersJson);
            Assert.Equal(email, leadFormSubmission.Email);
            Assert.Equal(phone, leadFormSubmission.Phone);
            Assert.InRange(leadFormSubmission.SubmittedAt, beforeSubmissionTime, afterSubmissionTime);

        }

        [InlineData("", "")]
        [InlineData(" ", " ")]
        [Theory]
        public void Empty_values_must_be_saved_as_null(string? email, string? phone)
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";

            
            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            
            // Assert
            Assert.Null(leadFormSubmission.Email);
            Assert.Null(leadFormSubmission.Phone);
            
        }

        [Fact]
        public void Lead_form_id_is_required()
        {
            // Arrange
            var leadFormId = Guid.Empty;
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            string? email = null;
            string? phone = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.LeadFormIdIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void FullName_is_required(string? fullName)
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var answersJson = "valid-answers-json";
            string? email = null;
            string? phone = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName!, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.FullNameIsRequired, domainError.Code);
        }

        [Fact]
        public void FullName_length_must_not_exceed_200_characters()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = new string('A', 201);
            var answersJson = "valid-answers-json";
            string? email = null;
            string? phone = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.FullNameIsTooLong, domainError.Code);
        }

        [Fact]
        public void Email_length_must_not_exceed_320_characters()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            var email = new string('A', 321);
            string? phone = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.EmailIsTooLong, domainError.Code);
        }

        [InlineData("test.gmail.com")]
        [InlineData("test@")]
        [InlineData("@gmail.com")]
        [Theory]
        public void Email_format_must_be_valid(string email)
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            var phone = "";

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.EmailInvalidFormat, domainError.Code);
        }

        [Fact]
        public void Phone_length_must_not_exceed_50_characters()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            string? email = null;
            var phone = new string('A', 51);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.PhoneIsTooLong, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Answers_json_is_required(string? answersJson)
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            string? email = null;
            string? phone = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new LeadFormSubmission(leadFormId, fullName, email, phone, answersJson!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.LeadFormSubmission.AnswersJsonIsRequired, domainError.Code);
        }

        [Fact]
        public void FullName_must_be_trimmed()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = " lead fullName ";
            var answersJson = "valid-answers-json";
            string? email = null;
            string? phone = null;

            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            // Assert
            Assert.Equal("lead fullName", leadFormSubmission.FullName);
        }

        [Fact]
        public void Email_must_be_trimmed()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            var email = " test@gmail.com ";
            string? phone = null;

            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            // Assert
            Assert.Equal("test@gmail.com", leadFormSubmission.Email);
        }

        [Fact]
        public void Phone_must_be_trimmed()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = "valid-answers-json";
            string? email = null;
            var phone = " valid-phone ";

            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            // Assert
            Assert.Equal("valid-phone", leadFormSubmission.Phone);
        }

        [Fact]
        public void Answers_json_must_be_trimmed()
        {
            // Arrange
            var leadFormId = Guid.NewGuid();
            var fullName = "lead fullName";
            var answersJson = " valid-answers-json ";
            string? email = null;
            string? phone = null;

            // Act
            LeadFormSubmission leadFormSubmission = new(leadFormId, fullName, email, phone, answersJson);

            // Assert
            Assert.Equal("valid-answers-json", leadFormSubmission.AnswersJson);
        }

    }
}
