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

            public const string TimezoneTooLong = "UserSettings.Timezone.TooLong";

            public const string InvalidSessionReminderMinutes = "UserSettings.NotificationPreferences.InvalidSessionReminderMinutes";

            public const string InvalidCalendarProvider = "UserSettings.CalendarIntegrationToken.InvalidCalendarProvider";

            public const string AccessTokenRequired = "UserSettings.CalendarIntegrationToken.AccessTokenRequired";

            public const string RefreshTokenRequired = "UserSettings.CalendarIntegrationToken.RefreshTokenRequired";

        }

        public static class Client
        {
            public const string TenantIdRequired = "Client.TenantId.Required";
            public const string FullNameRequired = "Client.FullName.Required";
            public const string FullNameTooLong = "Client.FullName.TooLong";
            public const string EmailInvalidFormat = "Client.Email.InvalidFormat";
            public const string PhoneTooLong = "Client.Phone.TooLong";
            public const string StatusIsInvalid = "Client.Status.IsInvalid";
            public const string TimezoneRequired = "Client.Timezone.Required";
            public const string TimezoneTooLong = "Client.Timezone.TooLong";
            public const string SourceIsInvalid = "Client.Source.IsInvalid";
            public const string IsAlreadyDeleted = "Client.Delete.IsAlreadyDeleted";
            public const string DeletedByRequired = "Client.Delete.DeletedByRequired";
            public const string CannotUpdateDeletedClient = "Client.Update.CannotUpdateDeletedClient";
        }
    }
}
