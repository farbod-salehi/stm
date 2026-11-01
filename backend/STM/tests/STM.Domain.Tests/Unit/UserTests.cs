using STM.Domain.Entities.Users;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class UserTests
    {
        [Fact]
        public void Admin_must_be_created_and_returned_when_input_is_valid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "admin@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";

            var beforeCreation = DateTimeOffset.UtcNow;

            // Act
            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            var afterCreation = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, admin.Id);
            Assert.Equal(tenantId, admin.TenantId);
            Assert.Equal(email, admin.Email);
            Assert.Equal(passwordHash, admin.PasswordHash);
            Assert.Equal(fullName, admin.FullName);
            Assert.Equal(UserRole.Admin, admin.Role);
            Assert.True(admin.IsActive);
            Assert.True(admin.IsVerified);
            Assert.InRange(admin.CreatedAt, beforeCreation, afterCreation);
            Assert.Null(admin.UpdatedAt);
            Assert.Null(admin.LastLoginAt);
        }

        [InlineData(UserRole.Assistant)]
        [InlineData(UserRole.Consultant)]
        [Theory]
        public void Staff_must_be_created_and_returned_when_input_is_valid(UserRole role)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "staff@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";

            var beforeCreation = DateTimeOffset.UtcNow;

            // Act
            User staff = User.CreateStaff(tenantId, email, passwordHash, fullName, role);

            var afterCreation = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, staff.Id);
            Assert.Equal(tenantId, staff.TenantId);
            Assert.Equal(email, staff.Email);
            Assert.Equal(passwordHash, staff.PasswordHash);
            Assert.Equal(fullName, staff.FullName);
            Assert.Equal(role, staff.Role);
            Assert.True(staff.IsActive);
            Assert.True(staff.IsVerified);
            Assert.InRange(staff.CreatedAt, beforeCreation, afterCreation);
            Assert.Null(staff.UpdatedAt);
            Assert.Null(staff.LastLoginAt);
        }

        
        [Fact]
        public void TenantId_is_required()
        {
            //Arrange
            Guid tenantId = Guid.Empty;
            var email = "admin@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateAdmin(tenantId, email, passwordHash, fullName));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.TenantIdRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(null)]
        [Theory]
        public void Email_is_required(string? email)
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email!, passwordHash, fullName, UserRole.Assistant));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.EmailRequired, domainError.Code);
        }

        [InlineData("test.gmail.com")]
        [InlineData("test@")]
        [InlineData("@gmail.com")]
        [Theory]
        public void Email_must_have_valid_format(string? email)
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email!, passwordHash, fullName, UserRole.Consultant));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.EmailInvalidFormat, domainError.Code);
        }

        [InlineData("")]
        [InlineData(null)]
        [Theory]
        public void Password_hash_is_required(string? passwordHash)
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var fullName = "Test Admin";

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateAdmin(tenantId, email, passwordHash!, fullName));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.PasswordHashRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(null)]
        [Theory]
        public void Full_name_is_required(string? fullName)
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email, passwordHash, fullName!, UserRole.Assistant));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.FullNameRequired, domainError.Code);
        }

        [Fact]
        public void Role_must_be_valid()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            var email = "admin@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";
            var role = (UserRole)999;

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email, passwordHash, fullName, role));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.RoleIsInvalid, domainError.Code);
        }

       
        [Fact]
        public void Staff_role_must_be_valid()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";
            var role = UserRole.Admin;

            //Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email, passwordHash, fullName, role));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.User.RoleCannotBeAdmin, domainError.Code);
        }

        [Fact]
        public void All_validation_errors_must_be_returned()
        {
            //Arrange
            Guid tenantId = Guid.Empty;
            var email = "test";
            string? passwordHash = null;
            string? fullName = null;
            var role = UserRole.Consultant;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => User.CreateStaff(tenantId, email, passwordHash!, fullName!, role));

            Assert.Equal(4, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.TenantIdRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.PasswordHashRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.FullNameRequired);
        }

        [Fact]
        public void Full_name_must_be_trimmed()
        {
            //Arrnge
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "  Test Admin  ";

            // Act
            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            // Assert
            Assert.Equal("Test Admin", admin.FullName);
        }

        [Fact]
        public void Email_must_be_trimmed_and_lowercase()
        {
            //Arrnge
            var tenantId = Guid.NewGuid();
            var email = " TEST@gmail.com ";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";
            var role = UserRole.Consultant;

            // Act
            User staff = User.CreateStaff(tenantId, email, passwordHash, fullName, role);

            // Assert
            Assert.Equal("test@gmail.com", staff.Email);
            
        }

        [Fact]
        public void Updated_email_and_fullname_must_be_normalized()
        {
            //Arrnge
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";
            
            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            // Act
            admin.UpdateAdminDetails(" NEW@gmail.com ", " New Admin ");

            // Assert
            Assert.Equal("new@gmail.com", admin.Email);
            Assert.Equal("New Admin", admin.FullName);
        }

        [Fact]
        public void Last_login_should_be_set_truly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";

            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            var beforeLogin = DateTimeOffset.UtcNow;

            // Act
            admin.RecordLogin();

            var afterLogin = DateTimeOffset.UtcNow;

            // Assert
            Assert.InRange(admin.LastLoginAt ?? default, beforeLogin, afterLogin);
        }

        [Fact]
        public void Admin_details_must_be_updated()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";

            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            // Act
            email = "new@gmail.com";
            fullName = "New Name";

            var beforeUpdate = DateTimeOffset.UtcNow;

            admin.UpdateAdminDetails(email, fullName);

            var afterUpdate = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(fullName, admin.FullName);
            Assert.Equal(email, admin.Email);
            Assert.InRange(admin.UpdatedAt ?? default, beforeUpdate, afterUpdate);
        }

        [Fact]
        public void Staff_details_must_be_updated()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";
            var role = UserRole.Consultant;

            User staff = User.CreateStaff(tenantId, email, passwordHash, fullName, role);

            // Act
            email = "new@gmail.com";
            fullName = "New Name";
            var isActive = false;
            role = UserRole.Assistant;

            var beforeUpdate = DateTimeOffset.UtcNow;

            staff.UpdateStaffDetails(email, fullName, isActive, role);

            var afterUpdate = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(fullName, staff.FullName);
            Assert.Equal(email, staff.Email);
            Assert.Equal(UserRole.Assistant, staff.Role);
            Assert.False(staff.IsActive);
            Assert.InRange(staff.UpdatedAt ?? default, beforeUpdate, afterUpdate);
        }

        [InlineData(UserRole.Admin)]
        [InlineData((UserRole)999)]
        [Theory]
        public void Staff_details_must_be_validated_when_updating(UserRole newRole)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Staff";
            var role = UserRole.Assistant;

            User staff = User.CreateStaff(tenantId, email, passwordHash, fullName, role);

            // Act & Assert            
            email = "test@@gmail.com";
            fullName = "";

            var exception = Assert.Throws<BusinessRuleException>(() => staff.UpdateStaffDetails(email, fullName, false, newRole));

            Assert.Equal(3, exception.Errors.Count);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.FullNameRequired);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.RoleCannotBeAdmin || e.Code == DomainErrors.User.RoleIsInvalid);
        }

        [Fact]
        public void Admin_details_must_be_validated_when_updating()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "test@gmail.com";
            var passwordHash = "hashed_password";
            var fullName = "Test Admin";

            User admin = User.CreateAdmin(tenantId, email, passwordHash, fullName);

            // Act & Assert            
            email = "test@@gmail.com";
            fullName = "";

            var exception = Assert.Throws<BusinessRuleException>(() => admin.UpdateAdminDetails(email, fullName));

            Assert.Equal(2, exception.Errors.Count);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.EmailInvalidFormat);
            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.User.FullNameRequired);
        }
    }
}
