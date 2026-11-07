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

        public static class UserSettings
        {
            public const string TimezoneRequired = "UserSettings.Timezone.Required";

            public const string UserIdRequired = "UserSettings.UserId.Required";

            public const string NotificationPreferencesRequired = "UserSettings.NotificationPreferences.Required";

            public const string InvalidTimezone = "UserSettings.Timezone.Invalid";

            public const string InvalidSessionReminderMinutes = "UserSettings.NotificationPreferences.InvalidSessionReminderMinutes";

            public const string InvalidCalendarProvider = "UserSettings.CalendarIntegrationToken.InvalidCalendarProvider";

            public const string AccessTokenRequired = "UserSettings.CalendarIntegrationToken.AccessTokenRequired";

            public const string RefreshTokenRequired = "UserSettings.CalendarIntegrationToken.RefreshTokenRequired";

        }

    }
}
