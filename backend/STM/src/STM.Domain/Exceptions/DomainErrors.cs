using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace STM.Domain.Exceptions
{
    public static class DomainErrors
    {
        public static class Tenant
        {
            public const string NameRequired = "Tenant.Name.IsRequired";
            public const string DomainRequired = "Tenant.Domain.IsRequired";
            public const string DomainLength = "Tenant.Domain.Length";
            public const string DomainContent = "Tenant.Domain.Content";       
            public const string DomainReservedWords = "Tenant.Domain.ReservedWords";
            public const string DomainHyphenPlacement = "Tenant.Domain.HyphenPlacement";
        }

        public static class User
        {
            public const string EmailRequired = "User.Email.IsRequired";
            public const string EmailInvalidFormat = "User.Email.InvalidFormat";
            public const string PasswordHashRequired = "User.PasswordHash.IsRequired";
            public const string FullNameRequired = "User.FullName.IsRequired";
            public const string RoleIsInvalid = "User.Role.IsInvalid";
            public const string RoleCannotBeAdmin = "User.Role.CannotBeAdmin";
            public const string TenantIdRequired = "User.TenantId.IsRequired";
        }

        public static class UserSettings
        {
            public const string TimezoneRequired = "UserSettings.Timezone.IsRequired";

            public const string UserIdRequired = "UserSettings.UserId.IsRequired";

            public const string NotificationPreferencesRequired = "UserSettings.NotificationPreferences.IsRequired";

            public const string TimezoneTooLong = "UserSettings.Timezone.TooLong";

            public const string InvalidSessionReminderMinutes = "UserSettings.NotificationPreferences.InvalidSessionReminderMinutes";

            public const string InvalidCalendarProvider = "UserSettings.CalendarIntegrationToken.InvalidCalendarProvider";

            public const string AccessTokenRequired = "UserSettings.CalendarIntegrationToken.AccessTokenRequired";

            public const string RefreshTokenRequired = "UserSettings.CalendarIntegrationToken.RefreshTokenRequired";

        }

        public static class Client
        {
            public const string TenantIdRequired = "Client.TenantId.IsRequired";
            public const string FullNameRequired = "Client.FullName.IsRequired";
            public const string FullNameTooLong = "Client.FullName.TooLong";
            public const string EmailInvalidFormat = "Client.Email.InvalidFormat";
            public const string PhoneTooLong = "Client.Phone.TooLong";
            public const string StatusIsInvalid = "Client.Status.IsInvalid";
            public const string TimezoneRequired = "Client.Timezone.IsRequired";
            public const string TimezoneTooLong = "Client.Timezone.TooLong";
            public const string SourceIsInvalid = "Client.Source.IsInvalid";
            public const string IsAlreadyDeleted = "Client.Delete.IsAlreadyDeleted";
            public const string DeletedByRequired = "Client.Delete.DeletedByRequired";
            public const string CannotUpdateDeletedClient = "Client.Update.CannotUpdateDeletedClient";
        }

        public static class ClientTimeline
        {
            public const string ClientIdRequired = "ClientTimeline.ClientId.IsRequired";

            public const string EventTypeIsInvalid = "ClientTimeline.EventType.IsInvalid";

            public const string OccurredAtIsInvalid = "ClientTimeline.OccurredAt.IsInvalid";

            public const string ConsultantIdIsInvalid = "ClientTimeline.ConsultantId.IsInvalid";
        }

        public static class ClientNote
        {
            public const string ClientIdIsRequired = "ClientNote.ClientId.IsRequired";

            public const string ConsultantIdIsRequired = "ClientNote.ConsultantId.IsRequired";

            public const string ContentIsRequired = "ClientNote.Content.IsRequired";

         }

        public static class ClientDocument
        {
            public const string ClientIdIsRequired = "ClientDocument.ClientId.IsRequired";

            public const string UploadedByUserIdIsRequired = "ClientDocument.UploadedByUserId.IsRequired";

            public const string FileUrlIsRequired = "ClientDocument.FileUrl.IsRequired";

            public const string FileUrlIsTooLong = "ClientDocument.FileUrl.IsTooLong";

            public const string TypeIsInvalid = "ClientDocument.Type.IsInvalid";
        }

        public static class UserLoginAttempt
        {
            public const string UserIdIsInvalid = "UserLoginAttempt.UserId.IsInvalid";

            public const string EmailIsRequired = "UserLoginAttempt.Email.IsRequired";

            public const string EmailIsTooLong = "UserLoginAttempt.Email.IsTooLong";

            public const string EmailInvalidFormat = "UserLoginAttempt.Email.InvalidFormat";

            public const string IpAddressIsTooLong = "UserLoginAttempt.IpAddress.IsTooLong";

            public const string UserAgentIsTooLong = "UserLoginAttempt.UserAgent.IsTooLong";

            public const string DeviceIdIsTooLong = "UserLoginAttempt.DeviceId.IsTooLong";
        }

        public static class LeadForm
        {
            public const string TenantIdIsRequired = "LeadForm.TenantId.IsRequired";

            public const string TitleIsRequired = "LeadForm.Title.IsRequired";

            public const string TitleIsTooLong = "LeadForm.Title.IsTooLong";

            public const string DescriptionIsTooLong = "LeadForm.Description.IsTooLong";

            public const string FieldsJsonIsRequired = "LeadForm.FieldsJson.IsRequired";

            public const string CannotUpdateDeletedLeadForm = "LeadForm.Update.CannotUpdateDeletedLeadForm";

            public const string IsAlreadyDeleted = "LeadForm.Delete.IsAlreadyDeleted";

            public const string DeletedByIsRequired = "LeadForm.Delete.DeletedByRequired";
        }

        public static class LeadFormSubmission
        {
            public const string LeadFormIdIsRequired = "LeadFormSubmission.LeadFormId.IsRequired";

            public const string FullNameIsRequired = "LeadFormSubmission.FullName.IsRequired";

            public const string FullNameIsTooLong = "LeadFormSubmission.FullName.IsTooLong";

            public const string EmailIsTooLong = "LeadFormSubmission.Email.IsTooLong";
            
            public const string EmailInvalidFormat = "LeadFormSubmission.Email.InvalidFromat";

            public const string PhoneIsTooLong = "LeadFormSubmission.Phone.IsTooLong";

            public const string AnswersJsonIsRequired = "LeadFormSubmission.AnswersJson.IsRequired";
        }

        public static class DiscoveryCall
        {
            public const string ClientIdIsRequired = "DiscoveryCall.ClientId.IsRequired";

            public const string ScheduledAtIsInvalid = "DiscoveryCall.ScheduledAt.IsInvalid";

            public const string DurationMinutesIsInvalid = "DiscoveryCall.DurationMinutes.IsInvalid";

            public const string StatusIsInvalid = "DiscoveryCall.Status.IsInvalid";

            public const string OutcomeIsInvalid = "DiscoveryCall.Outcome.IsInvalid";

            public const string NotesIsTooLong = "DiscoveryCall.Notes.IsTooLong";

            public const string ExternalEventIdIsTooLong = "DiscoveryCall.ExternalEventId.IsTooLong";

            public const string CannotUpdateDeletedDiscoveryCall = "DiscoveryCall.Update.CannotUpdateDeletedDiscoveryCall";

            public const string IsAlreadyDeleted = "DiscoveryCall.Delete.IsAlreadyDeleted";

            public const string DeletedByIsRequired = "DiscoveryCall.Delete.DeletedByIsRequired";

            public const string CannotRescheduleUnlessScheduled = "DiscoveryCall.ScheduledAt.CannotEditUnlessScheduled";

            public const string CannotEditDurationUnlessScheduled = "DiscoveryCall.DurationMinutes.CannotEditUnlessScheduled";

            public const string CannotEditStatusUnlessScheduled = "DiscoveryCall.Status.CannotEditUnlessScheduled";
        }
    }
}
