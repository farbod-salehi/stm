using STM.Domain.Entities.ClientEngagement;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace STM.Domain.Tests.Unit
{
    public class PackageTests
    {
        [InlineData("package description")]
        [InlineData(null)]
        [Theory]
        public void Package_must_be_created_and_returned_with_valid_inputs(string? description)
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "package name";
            decimal price = 99.99m;
            int sessionsIncluded = 5;

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            Package package = new(tenantId, name, description, price, sessionsIncluded);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, package.Id);
            Assert.Equal(tenantId, package.TenantId);
            Assert.Equal(name, package.Name);
            Assert.Equal(description, package.Description);
            Assert.Equal(price, package.Price);
            Assert.Equal(sessionsIncluded, package.SessionsIncluded);
            Assert.InRange(package.CreatedAt, beforeCreationTime, afterCreationTime);
            Assert.Null(package.UpdatedAt);
            Assert.Null(package.DeletedAt);
            Assert.Null(package.DeletedBy);
            Assert.False(package.IsDeleted);

        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_description_must_be_saved_as_null_for_creation(string? description)
        {
            
            // Act
            var package = CreatePackage(description: description);

            // Assert
            Assert.Null(package.Description);

        }

        [InlineData("new package description")]
        [InlineData(null)]
        [Theory]
        public void Package_must_be_updated_with_valid_inputs(string? newDescription)
        {
            // Arrange
            var package = CreatePackage();

            var id = package.Id;
            var createdAt = package.CreatedAt;
            var tenantId = package.TenantId;

            var newName = "new package name";
            decimal newPrice = 109.99m;
            int newSessionsIncluded = 8;

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            package.UpdateDetails(newName, newDescription, newPrice, newSessionsIncluded);

            var afterUpdateTime = DateTimeOffset.UtcNow;


            // Assert
            Assert.Equal(id, package.Id);
            Assert.Equal(tenantId, package.TenantId);
            Assert.Equal(newName, package.Name);
            Assert.Equal(newDescription, package.Description);
            Assert.Equal(newPrice, package.Price);
            Assert.Equal(newSessionsIncluded, package.SessionsIncluded);
            Assert.Equal(createdAt, package.CreatedAt);
            Assert.NotNull(package.UpdatedAt);
            Assert.InRange(package.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
            Assert.Null(package.DeletedAt);
            Assert.Null(package.DeletedBy);
            Assert.False(package.IsDeleted);

        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_description_must_be_saved_as_null_for_update(string? newDescription)
        {
            // Arrange
            var package = CreatePackage();

            var newName = "new package name";
            decimal newPrice = 109.99m;
            int newSessionsIncluded = 8;

            // Act
            package.UpdateDetails(newName, newDescription, newPrice, newSessionsIncluded);


            // Assert
            Assert.Null(package.Description);
        }

        [Fact]
        public void Package_must_be_deleted_with_valid_inputs()
        {
            // Arrange
            var package = CreatePackage();

            var id = package.Id;
            var tenantId = package.TenantId;
            var name = package.Name;
            var price = package.Price;
            var sessionsIncluded = package.SessionsIncluded;

            var deletedBy = Guid.NewGuid();

            var beforeDeleteTime = DateTimeOffset.UtcNow;

            // Act
            package.Delete(deletedBy);

            var afterDeleteTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(package.IsDeleted);
            Assert.NotNull(package.DeletedAt);
            Assert.InRange(package.DeletedAt ?? default, beforeDeleteTime, afterDeleteTime);
            Assert.Equal(deletedBy, package.DeletedBy);
            Assert.Equal(id, package.Id);
            Assert.Equal(tenantId, package.TenantId);
            Assert.Equal(name, package.Name);
            Assert.Equal(price, package.Price);
            Assert.Equal(sessionsIncluded, package.SessionsIncluded);
        }

        [Fact]
        public void Deleted_package_cannot_be_updated()
        {
            // Arrange
            var package = CreatePackage();

            var price = package.Price;
            var updatedAt = package.UpdatedAt;

            package.Delete(Guid.NewGuid());

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(package.Name, package.Description, 109.99m, package.SessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.CannotUpdateDeletedPackage, domainError.Code);
            Assert.Equal(price, package.Price);
            Assert.Equal(updatedAt, package.UpdatedAt);
        }

        [Fact]
        public void Deleted_package_cannot_be_deleted()
        {
            // Arrange
            var package = CreatePackage();

            var deletedBy = Guid.NewGuid();

            package.Delete(deletedBy);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.IsAlreadyDeleted, domainError.Code);
            Assert.Equal(deletedBy, package.DeletedBy);
        }

        [Fact]
        public void Tenant_id_is_required_for_creation()
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(tenantId: Guid.Empty));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.TenantIdIsRequired, domainError.Code);
            
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Name_is_required_for_creation(string? name)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(name: name!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.NameIsRequired, domainError.Code);

        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Name_is_required_for_update(string? newName)
        {
            //Arrange
            var package = CreatePackage();

            var name = package.Name;
            var updatedAt = package.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(newName!, package.Description, package.Price, package.SessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.NameIsRequired, domainError.Code);
            Assert.Equal(name, package.Name);
            Assert.Equal(updatedAt, package.UpdatedAt);

        }

        [Fact]
        public void Name_length_should_not_exceed_200_characters_for_creation()
        {
            //Arrange
            var name = new string('A', 201);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(name: name));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.NameIsTooLong, domainError.Code);

        }

        [Fact]
        public void Name_length_should_not_exceed_200_characters_for_update()
        {
            //Arrange
            var package = CreatePackage();

            var name = package.Name;
            var updatedAt = package.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(new string('A', 201), package.Description, package.Price, package.SessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.NameIsTooLong, domainError.Code);
            Assert.Equal(name, package.Name);
            Assert.Equal(updatedAt, package.UpdatedAt);
        }

        [Fact]
        public void Description_length_should_not_exceed_5000_characters_for_creation()
        {
            //Arrange
            var description = new string('A', 5001);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(description: description));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.DescriptionIsTooLong, domainError.Code);

        }

        [Fact]
        public void Description_length_should_not_exceed_5000_characters_for_update()
        {
            //Arrange
            var package = CreatePackage();

            var description = package.Description;
            var updatedAt = package.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(package.Name, new string('A', 5001), package.Price, package.SessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.DescriptionIsTooLong, domainError.Code);
            Assert.Equal(description, package.Description);
            Assert.Equal(updatedAt, package.UpdatedAt);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(999999999)]
        [Theory]
        public void Price_must_be_between_1_and_99999999_99_for_creation(decimal price)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(price: price));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.PriceIsInvalid, domainError.Code);

        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(999999999)]
        [Theory]
        public void Price_must_be_between_1_and_99999999_99_for_update(decimal newPrice)
        {
            //Arrange
            var package = CreatePackage();

            var price = package.Price;
            var updatedAt = package.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(package.Name,package.Description, newPrice, package.SessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.PriceIsInvalid, domainError.Code);
            Assert.Equal(price, package.Price);
            Assert.Equal(updatedAt, package.UpdatedAt);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [Theory]
        public void Sessions_included_must_be_positive_for_creation(int sessionsIncluded)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreatePackage(sessionsIncluded: sessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.SessionsIncludedIsInvalid, domainError.Code);

        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [Theory]
        public void Sessions_included_must_be_positive_for_update(int newSessionsIncluded)
        {
            //Arrange
            var package = CreatePackage();

            var sessionsIncluded = package.SessionsIncluded;
            var updatedAt = package.UpdatedAt;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => package.UpdateDetails(package.Name, package.Description, package.Price, newSessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Package.SessionsIncludedIsInvalid, domainError.Code);
            Assert.Equal(sessionsIncluded, package.SessionsIncluded);
            Assert.Equal(updatedAt, package.UpdatedAt);
        }

        [Fact]
        public void Name_must_be_trimmed_for_creation()
        {
            //Arrange
            var name = " package name ";

            // Act
            var package = CreatePackage(name: name);

            // Assert
            Assert.Equal("package name", package.Name);
        }

        [Fact]
        public void Name_must_be_trimmed_for_update()
        {
            //Arrange
            var package = CreatePackage();

            var newName = " new package name ";

            // Act
            package.UpdateDetails(newName, package.Description, package.Price, package.SessionsIncluded);

            // Assert
            Assert.Equal("new package name", package.Name);
        }

        [Fact]
        public void Description_must_be_trimmed_for_creation()
        {
            //Arrange
            var description = " package description ";

            // Act
            var package = CreatePackage(description: description);

            // Assert
            Assert.Equal("package description", package.Description);
        }

        [Fact]
        public void Description_must_be_trimmed_for_update()
        {
            //Arrange
            var package = CreatePackage();

            var newDescription = " new package description ";

            // Act
            package.UpdateDetails(package.Name, newDescription, package.Price, package.SessionsIncluded);

            // Assert
            Assert.Equal("new package description", package.Description);
        }


        private static Package CreatePackage(Guid? tenantId = null, string name = "package name", string? description = "Package description", decimal price = 99.99m, int sessionsIncluded = 5)
        {
            return new(tenantId ?? Guid.NewGuid(), name, description, price, sessionsIncluded);
        }
    }
}
