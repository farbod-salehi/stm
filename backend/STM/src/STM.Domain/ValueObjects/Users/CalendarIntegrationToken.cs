using STM.Domain.Common;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.ValueObjects.Users
{
    public sealed class CalendarIntegrationToken : ValueObject
    {
        private CalendarIntegrationToken() { /* EF Core constructor */ }

        private CalendarIntegrationToken(CalendarProvider provider, string accessToken, string refreshToken, DateTimeOffset expiresAt)
        {
            Provider = provider;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }

        public static CalendarIntegrationToken Create(CalendarProvider provider, string accessToken, string refreshToken, DateTimeOffset expiresAt)
        {
            List<DomainError> errors = [];

            if (!Enum.IsDefined(provider))
                errors.Add(new DomainError(DomainErrors.UserSettings.InvalidCalendarProvider));

            if (string.IsNullOrWhiteSpace(accessToken))
                errors.Add(new DomainError(DomainErrors.UserSettings.AccessTokenRequired));

            if (string.IsNullOrWhiteSpace(refreshToken))
                errors.Add(new DomainError(DomainErrors.UserSettings.RefreshTokenRequired));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            return new CalendarIntegrationToken(
                provider,
                accessToken,
                refreshToken,
                expiresAt);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Provider;
            yield return AccessToken;
            yield return RefreshToken;
            yield return ExpiresAt;
        }

        public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

        public CalendarProvider Provider { get; }

        public string AccessToken { get; } = default!;

        public string RefreshToken { get; } = default!;

        public DateTimeOffset ExpiresAt { get; }
    }
}
