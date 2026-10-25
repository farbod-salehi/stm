using STM.Domain.Entities;
using STM.Domain.Exceptions;

namespace STM.Domain.Tests.Unit;

public class TenantTests
{
    [MemberData(nameof(DomainsWithValidLength))]
    [Theory]
    public void Tenant_must_be_created_and_returned_when_input_is_valid(string tenantDomain)
    {
        // Arrange
        var tenantName = "Test Tenant";
      

        // Act
        Tenant tenant = new(tenantName, tenantDomain);

        // Assert
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantDomain, tenant.Domain);

    }


    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public void Name_is_required(string? name)
    {
        //Arrange
        var tenantDomain = "valid-domain";

        //Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(name!, tenantDomain));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.NameRequired, domainError.Code);
    }

    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public void Domain_is_required(string? domain)
    {
        //Arrange
        var tenantName = "Test Tenant";

        //Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain!));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.DomainRequired, domainError.Code);
    }


    [MemberData(nameof(DomainsWithInvalidLength))]
    [Theory]
    public void Domain_must_be_between_3_and_50_characters(string domain)
    {
        // Arrange
        var tenantName = "Test Tenant";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain));

        DomainError domainError = Assert.Single(exception.Errors);

        Assert.Equal(DomainErrors.Tenant.DomainLength, domainError.Code);

        var parameters = domainError.Parameters!;
        var minLength = (int)parameters.GetType().GetProperty("MinLength")!.GetValue(parameters)!;
        var maxLength = (int)parameters.GetType().GetProperty("MaxLength")!.GetValue(parameters)!;

        Assert.Equal(3, minLength);
        Assert.Equal(50, maxLength);
    }

    [MemberData(nameof(DomainsWithInvalidCharacters))]
    [Theory]
    public void Domain_must_content_letters_numbers_hyphens(string domain)
    {
        // Arrange
        var tenantName = "Test Tenant";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.DomainContent, domainError.Code);
    }

    [InlineData("-test")]
    [InlineData("test-")]
    [InlineData("-test-")]
    [Theory]
    public void Domain_must_start_and_end_with_letter_or_number(string domain)
    {
        // Arrange
        var tenantName = "Test Tenant";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.DomainHyphenPlacement, domainError.Code);
    }

    [Fact]
    public void Domain_must_not_contain_consecutive_hyphens()
    {
        // Arrange
        var tenantName = "Test Tenant";
        var domain = "test--tenant";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.DomainHyphenPlacement, domainError.Code);
    }

    [InlineData("admin")]
    [InlineData("root")]
    [InlineData("stmapp")]
    [Theory]
    public void Domain_must_not_be_reserved(string domain)
    {
        // Arrange
        var tenantName = "Test Tenant";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant(tenantName, domain));

        DomainError domainError = Assert.Single(exception.Errors);
        Assert.Equal(DomainErrors.Tenant.DomainReservedWords, domainError.Code);
    }

    [Fact]
    public void All_validation_errors_must_be_returned()
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => new Tenant("", ""));

        Assert.Equal(2, exception.Errors.Count);

        Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Tenant.NameRequired);

        Assert.Contains(exception.Errors,e => e.Code == DomainErrors.Tenant.DomainRequired);
    }

    [Fact]
    public void Name_must_be_trimmed()
    {
        //Arrange & Act
        Tenant tenant = new("  Test Tenant  ", "tenant");

        // Assert
        Assert.Equal("Test Tenant", tenant.Name);
    }

    [Fact]
    public void Domain_must_be_trimmed_and_lowercase()
    {
        // Arrange & Act
        Tenant tenant = new("Test Tenant","  My-Domain  ");

        // Assert
        Assert.Equal("my-domain", tenant.Domain);
    }

    [Fact]
    public void Name_and_domain_must_be_updated()
    {
        // Arrange
        Tenant tenant = new("Old Name", "old-domain");

        // Act
        tenant.UpdateDetails("New Name", "new-domain");

        // Assert
        Assert.Equal("New Name", tenant.Name);
        Assert.Equal("new-domain", tenant.Domain);
    }

    [Fact]
    public void Updated_at_must_be_set_when_details_are_updated()
    {
        // Arrange
        Tenant tenant = new("Name", "domain");

        // Act
        tenant.UpdateDetails("New Name", "new-domain");

        // Assert
        Assert.NotNull(tenant.UpdatedAt);
    }

    [Fact]
    public void Name_and_domain_must_be_validated_when_updating_details()
    {
        // Arrange
        Tenant tenant = new("Name","domain");

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => tenant.UpdateDetails("", "-domain"));

        Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Tenant.NameRequired);
        Assert.Contains(exception.Errors, e => e.Code == DomainErrors.Tenant.DomainHyphenPlacement);
    }

    public static TheoryData<string> DomainsWithInvalidLength =>
        [
            new string('a', 1),
            new string('a', 2),
            new string('a', 51),
            new string('a', 52)
        ];

    public static TheoryData<string> DomainsWithValidLength =>
        [
            new string('a', 3),
            new string('a', 4),
            new string('a', 49),
            new string('a', 50)
        ];

    public static TheoryData<string> DomainsWithInvalidCharacters =>
        [
            "test@tenant",
            "test tenant",
            "test.tenant",
            "test_tenant"
        ];

   
}
