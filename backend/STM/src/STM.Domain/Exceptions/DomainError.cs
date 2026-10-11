using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Exceptions
{
    public sealed record DomainError(string Code, object? Parameters = null);
    
}
