using STM.Domain.Entities.Users;
using STM.Domain.Exceptions;

namespace STM.Domain.Tests.Unit
{
    public class UserLoginAttemptTests
    {
        [MemberData(nameof(ValidInputs))]
        [Theory]
        public void User_login_attempt_must_be_created_and_returned_with_valid_inputs(
            bool isSuccessful, Guid? userId, string? ipAddress, string? userAgent, string? deviceId)
        {
            // Arrange
            string email = "test@gmail.com";

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, userLoginAttempt.Id);
            Assert.Equal(email, userLoginAttempt.Email);
            Assert.Equal(userId, userLoginAttempt.UserId);
            Assert.Equal(ipAddress, userLoginAttempt.IpAddress);
            Assert.Equal(userAgent, userLoginAttempt.UserAgent);
            Assert.Equal(deviceId, userLoginAttempt.DeviceId);
            Assert.Equal(isSuccessful, userLoginAttempt.IsSuccessful);
            Assert.InRange(userLoginAttempt.CreatedAt, beforeCreationTime, afterCreationTime);
        }

        
        [Fact]
        public void User_id_must_be_valid()
        {
            // Arrange
            var userId = Guid.Empty;
            var ipAddress = "";
            var userAgent = "";
            var deviceId = "";
            string email = "test@gmail.com";
            bool isSuccessful = true;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.UserIdIsInvalid, domainError.Code);

        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [Theory]
        public void Email_is_required(string? email)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ipAddress = " ";
            var userAgent = " ";
            var deviceId = " ";
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email!, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.EmailIsRequired, domainError.Code);
        }

        [Fact]
        public void Email_length_must_not_exceed_256_characters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = new string('A', 257);
            string? ipAddress = null;
            string? userAgent = null;
            string? deviceId = null;
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.EmailIsTooLong, domainError.Code);
        }

        [InlineData("test.gmail.com")]
        [InlineData("test@")]
        [InlineData("@gmail.com")]
        [Theory]
        public void Email_must_be_valid(string email)
        {
            // Arrange
            var userId = Guid.NewGuid();
            string? ipAddress = null;
            string? userAgent = null;
            string? deviceId = null;
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.EmailInvalidFormat, domainError.Code);
        }

        [Fact]
        public void Ip_address_length_must_not_exceed_64_characters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var ipAddress = new string('A', 65);
            string? userAgent = null;
            string? deviceId = null;
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.IpAddressIsTooLong, domainError.Code);
        }

        [Fact]
        public void Device_id_length_must_not_exceed_64_characters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var deviceId = new string('A', 65);
            string? userAgent = null;
            string? ipAddress = null;
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.DeviceIdIsTooLong, domainError.Code);
        }

        [Fact]
        public void User_agent_length_must_not_exceed_512_characters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var userAgent = new string('A', 513);
            string? deviceId = null;
            string? ipAddress = null;
            bool isSuccessful = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserLoginAttempt.UserAgentIsTooLong, domainError.Code);
        }

        [Fact]
        public void Email_must_be_trimmed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = " test@gmail.com ";
            var ipAddress = " ";
            var userAgent = " ";
            var deviceId = " ";
            bool isSuccessful = false;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);
            
            
            // Assert
            Assert.Equal("test@gmail.com", userLoginAttempt.Email);
        }

        [Fact]
        public void Ip_address_must_be_trimmed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var ipAddress = " valid-ip ";
            var userAgent = " ";
            var deviceId = " ";
            bool isSuccessful = false;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);


            // Assert
            Assert.Equal("valid-ip", userLoginAttempt.IpAddress);
        }

        [Fact]
        public void User_agent_must_be_trimmed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var ipAddress = "valid-ip";
            var userAgent = " valid-user-agent ";
            var deviceId = " ";
            bool isSuccessful = false;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);


            // Assert
            Assert.Equal("valid-user-agent", userLoginAttempt.UserAgent);
        }

        [Fact]
        public void Device_id_must_be_trimmed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@gmail.com";
            var ipAddress = "valid-ip";
            var userAgent = "valid-user-agent";
            var deviceId = " valid-device-id ";
            bool isSuccessful = false;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);


            // Assert
            Assert.Equal("valid-device-id", userLoginAttempt.DeviceId);
        }

        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        [Theory]
        public void Empty_inputs_must_be_saved_null(string deviceId, string userAgent, string ipAddress)
        {
            // Arrange
            var userId = Guid.NewGuid();
            string email = "test@gmail.com";
            bool isSuccessful = true;

            // Act
            UserLoginAttempt userLoginAttempt = new(userId, email, isSuccessful, ipAddress, userAgent, deviceId);

            // Assert
            Assert.Null(userLoginAttempt.IpAddress);
            Assert.Null(userLoginAttempt.UserAgent);
            Assert.Null(userLoginAttempt.DeviceId);
        }

        [Fact]
        public void Multiple_validation_errors_must_be_returned()
        {
            // Arrange
            var userId = Guid.Empty;
            var email = "test@";
            var ipAddress = new string('A', 65);
            var userAgent = new string('A', 513); ;
            var deviceId = new string('A', 65); ;
            bool isSuccessful = true;

          

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserLoginAttempt(userId, email, isSuccessful, ipAddress, userAgent, deviceId));

            Assert.Equal(5, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserLoginAttempt.UserIdIsInvalid);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserLoginAttempt.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserLoginAttempt.IpAddressIsTooLong);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserLoginAttempt.UserAgentIsTooLong);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserLoginAttempt.DeviceIdIsTooLong);

        }

        public static TheoryData<bool, Guid?, string?, string?, string?> ValidInputs => new()
        {
            { true, null, null, null, null },
            { true, Guid.NewGuid(),  "valid-ip", "valid-user-agent", "valid-device-id" },
            { false, Guid.NewGuid(), null, "valid-user-agent", null },
            { false, Guid.NewGuid(), "valid-ip", null, "valid-device-id" },
            { true, Guid.NewGuid(), null, null, "valid-device-id" }
        };
    }
}
