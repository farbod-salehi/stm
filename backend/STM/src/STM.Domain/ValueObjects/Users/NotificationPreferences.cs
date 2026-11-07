using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.ValueObjects.Users
{
    public sealed class NotificationPreferences
    {
        private NotificationPreferences() { /* EF Core constructor */ }

        private NotificationPreferences(bool email, bool sms, bool inApp, bool push, 
            int sessionReminderMinutes, bool dailySummary, bool clientMessageAlerts,bool taskAssignedAlerts)
        {
            Email = email;
            Sms = sms;
            InApp = inApp;
            Push = push;
            SessionReminderMinutes = sessionReminderMinutes;
            DailySummary = dailySummary;
            ClientMessageAlerts = clientMessageAlerts;
            TaskAssignedAlerts = taskAssignedAlerts;
        }


        public static NotificationPreferences Create(bool email, bool sms, bool inApp, bool push, 
            int sessionReminderMinutes, bool dailySummary, bool clientMessageAlerts, bool taskAssignedAlerts)
        {
            List<DomainError> errors = [];

            if (sessionReminderMinutes < 0)
            {
                errors.Add(new DomainError(DomainErrors.UserSettings.InvalidSessionReminderMinutes));
            }

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            return new NotificationPreferences(
                email,
                sms,
                inApp,
                push,
                sessionReminderMinutes,
                dailySummary,
                clientMessageAlerts,
                taskAssignedAlerts);
        }


        public bool Email { get; }
        public bool Sms { get; }
        public bool InApp { get; }
        public bool Push { get; }

        public int SessionReminderMinutes { get; }

        public bool DailySummary { get; }
        public bool ClientMessageAlerts { get; }
        public bool TaskAssignedAlerts { get; }
    }
}
