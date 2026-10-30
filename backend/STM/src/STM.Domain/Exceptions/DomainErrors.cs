using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Exceptions
{
    public static class DomainErrors
    {
        public static class Tenant
        {
            public const string NameRequired = "Tenant.Name.Required";
            public const string DomainRequired = "Tenant.Domain.Required";
            public const string DomainLength = "Tenant.Domain.Length";
            public const string DomainContent = "Tenant.Domain.Content";       
            public const string DomainReservedWords = "Tenant.Domain.ReservedWords";
            public const string DomainHyphenPlacement = "Tenant.Domain.HyphenPlacement";
        }

        public static class User
        {
            public const string EmailRequired = "User.Email.Required";
            public const string EmailInvalidFormat = "User.Email.InvalidFormat";
            public const string PasswordHashRequired = "User.PasswordHash.Required";
            public const string FullNameRequired = "User.FullName.Required";
            public const string RoleIsInvalid = "User.Role.IsInvalid";
            public const string RoleCannotBeAdmin = "User.Role.CannotBeAdmin";
            public const string TenantIdRequired = "User.TenantId.Required";
        }

    }
}
