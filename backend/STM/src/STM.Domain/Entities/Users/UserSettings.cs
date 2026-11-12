using STM.Domain.Exceptions;
using STM.Domain.ValueObjects.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Entities.Users
{
    public sealed class UserSettings
    {
        private const int MaxTimezoneLength = 100;

        private UserSettings() { /* EF Core constructor */ }

        public UserSettings(Guid userId, string timezone, NotificationPreferences notificationPreferences, CalendarIntegrationToken? calendarIntegrationToken)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateUserId(userId));
            errors.AddRange(ValidateTimezone(timezone));
            errors.AddRange(ValidateNotificationPreferences(notificationPreferences));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            UserId = userId;            
            SetTimezone(timezone);
            SetNotificationPreferences(notificationPreferences);
            SetCalendarIntegrationToken(calendarIntegrationToken);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
        }

        public void Update(string timezone, NotificationPreferences notificationPreferences, CalendarIntegrationToken? calendarIntegrationToken)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateTimezone(timezone));
            errors.AddRange(ValidateNotificationPreferences(notificationPreferences));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetTimezone(timezone);
            SetNotificationPreferences(notificationPreferences);
            SetCalendarIntegrationToken(calendarIntegrationToken);

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateUserId(Guid userId)
        {
            List<DomainError> errors = [];

            if (userId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.UserSettings.UserIdRequired));

            return errors;
        }

        private static List<DomainError> ValidateTimezone(string timezone)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(timezone))
            {
                errors.Add(new DomainError(DomainErrors.UserSettings.TimezoneRequired));

                return errors;
            }

            timezone = timezone.Trim();

            if (timezone.Length > MaxTimezoneLength)
            {
                errors.Add(new DomainError(DomainErrors.UserSettings.TimezoneTooLong));

                return errors;
            }

            
            return errors;
        }

        private static List<DomainError> ValidateNotificationPreferences(NotificationPreferences? notificationPreferences)
        {
            List<DomainError> errors = [];

            if (notificationPreferences == null)
                errors.Add(new DomainError(DomainErrors.UserSettings.NotificationPreferencesRequired));

            return errors;
        }

        private void SetTimezone(string timezone)
        {
            Timezone = timezone.Trim();
        }

        private void SetNotificationPreferences(NotificationPreferences notificationPreferences)
        {
            NotificationPreferences = notificationPreferences;
        }

        private void SetCalendarIntegrationToken(CalendarIntegrationToken? calendarIntegrationToken)
        {
            CalendarIntegrationToken = calendarIntegrationToken;
        }

        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string Timezone { get; private set; } = default!;

        public NotificationPreferences NotificationPreferences { get; private set; } = default!;

        public CalendarIntegrationToken? CalendarIntegrationToken { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        /* Navigation properties */

        public User User { get; private set; } = default!;
    }
}
