using STM.Domain.Exceptions;
using STM.Domain.ValueObjects.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class NotificationPreferencesTests
    {
        [Fact]
        public void NotificationPreferences_must_be_created_and_returned_with_valid_inputs()
        {
            // Arrange
            bool email = true;
            bool sms = false;
            bool inApp = true;
            bool push = false;
            int sessionReminderMinutes = 15;
            bool dailySummary = true;
            bool clientMessageAlerts = true;
            bool taskAssignedAlerts = false;

            // Act
            var notificationPreferences = NotificationPreferences.Create(
                email,
                sms,
                inApp,
                push,
                sessionReminderMinutes,
                dailySummary,
                clientMessageAlerts,
                taskAssignedAlerts);

            // Assert
            Assert.Equal(email, notificationPreferences.Email);
            Assert.Equal(sms, notificationPreferences.Sms);
            Assert.Equal(inApp, notificationPreferences.InApp);
            Assert.Equal(push, notificationPreferences.Push);
            Assert.Equal(sessionReminderMinutes, notificationPreferences.SessionReminderMinutes);
            Assert.Equal(dailySummary, notificationPreferences.DailySummary);
            Assert.Equal(clientMessageAlerts, notificationPreferences.ClientMessageAlerts);
            Assert.Equal(taskAssignedAlerts, notificationPreferences.TaskAssignedAlerts);
        }

        
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        [Theory]
        public void NotificationPreferences_must_have_valid_session_reminder_minutes(int sessionReminderMinutes)
        {
            // Arrange
            bool email = true;
            bool sms = false;
            bool inApp = true;
            bool push = false;
            bool dailySummary = true;
            bool clientMessageAlerts = true;
            bool taskAssignedAlerts = false;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => NotificationPreferences.Create(
                email,
                sms,
                inApp,
                push,
                sessionReminderMinutes,
                dailySummary,
                clientMessageAlerts,
                taskAssignedAlerts));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.InvalidSessionReminderMinutes, domainError.Code);
        }

        [Fact]
        public void NotificationPreferences_with_same_values_must_be_equal()
        {
            // Act
            var preferences1 = NotificationPreferences.Create(
                true, false, true, false, 15, true, true, false);

            var preferences2 = NotificationPreferences.Create(
                true, false, true, false, 15, true, true, false);
            
            // Assert
            Assert.Equal(preferences1, preferences2);
        }

        [Fact]
        public void NotificationPreferences_with_different_values_must_not_be_equal()
        {
            // Act
            var preferences1 = NotificationPreferences.Create(
                true, false, true, false, 15, true, true, false);

            var preferences2 = NotificationPreferences.Create(
                false, false, true, false, 15, true, true, false);

            // Assert
            Assert.NotEqual(preferences1, preferences2);
        }
    }
}
