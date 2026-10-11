using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Exceptions
{
    public sealed class BusinessRuleException(IEnumerable<DomainError> errors) : Exception("Business rule violation.")
    {
        public IReadOnlyList<DomainError> Errors { get; } = [.. errors]; // Simplified syntax for ToList() in C# 14.0
    }
}
