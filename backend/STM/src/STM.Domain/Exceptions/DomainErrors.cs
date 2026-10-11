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

    }
}
