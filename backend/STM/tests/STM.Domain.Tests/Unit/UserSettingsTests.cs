using STM.Domain.Entities.Users;
using STM.Domain.Exceptions;
using STM.Domain.ValueObjects.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class UserSettingsTests
    {
        [MemberData(nameof(ValidCalendarIntegrationTokens))]
        [Theory]
        public void User_settings_must_be_created_and_returned_when_input_is_valid(CalendarIntegrationToken? calendarIntegrationToken)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            

            var beforeCreation = DateTimeOffset.UtcNow;

            // Act
            UserSettings userSettings = new(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            var afterCreation = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, userSettings.Id);
            Assert.Equal(userId, userSettings.UserId);
            Assert.Equal(timeZone, userSettings.Timezone);
            Assert.Equal(notificationPreferences, userSettings.NotificationPreferences);
            Assert.Equal(calendarIntegrationToken, userSettings.CalendarIntegrationToken);
            Assert.InRange(userSettings.CreatedAt, beforeCreation, afterCreation);
            Assert.Null(userSettings.UpdatedAt);
        }

        [MemberData(nameof(ValidCalendarIntegrationTokens))]
        [Theory]
        public void User_settings_must_be_updated_and_returned_when_input_is_valid(CalendarIntegrationToken? newCalendarIntegrationToken)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            UserSettings userSettings = new(userId, timeZone, notificationPreferences, calendarIntegrationToken);
            var createdAtBeforeUpdate = userSettings.CreatedAt;

            var newTimeZone = "Europe/London";
            var newNotificationPreferences = NotificationPreferences.Create(false, true, false, true, 15, true, false, true);
           

            DateTimeOffset beforeUpdate = DateTimeOffset.UtcNow;

            // Act
            userSettings.Update(newTimeZone, newNotificationPreferences, newCalendarIntegrationToken);

            DateTimeOffset afterUpdate = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(newTimeZone, userSettings.Timezone);
            Assert.Equal(newNotificationPreferences, userSettings.NotificationPreferences);
            Assert.Equal(newCalendarIntegrationToken, userSettings.CalendarIntegrationToken);
            Assert.InRange(userSettings.UpdatedAt ?? default, beforeUpdate, afterUpdate);
            Assert.Equal(createdAtBeforeUpdate, userSettings.CreatedAt);
        }

        [Fact]
        public void User_id_is_required()
        {
            // Arrange
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserSettings(Guid.Empty, timeZone, notificationPreferences, calendarIntegrationToken));
            
            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.UserIdRequired, domainError.Code);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [Theory]
        public void Timezone_is_required_when_updating(string? newTimeZone)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;
            
            var userSettings = new UserSettings(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => userSettings.Update(newTimeZone!, notificationPreferences, calendarIntegrationToken));
            
            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.TimezoneRequired, domainError.Code);
        }


        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [Theory]
        public void Timezone_is_required(string? timeZone)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserSettings(userId, timeZone!, notificationPreferences, calendarIntegrationToken));
            
            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.TimezoneRequired, domainError.Code);
        }

        [MemberData(nameof(InvalidTimezones))]
        [Theory]
        public void Timezone_must_have_valid_format(string timeZone)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserSettings(userId, timeZone, notificationPreferences, calendarIntegrationToken));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.TimezoneTooLong, domainError.Code);
        }

        [MemberData(nameof(InvalidTimezones))]
        [Theory]
        public void Timezone_must_have_valid_format_when_updating(string newTimeZone)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            var userSettings = new UserSettings(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => userSettings.Update(newTimeZone, notificationPreferences, calendarIntegrationToken));

            var domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.TimezoneTooLong, domainError.Code);
        }

        [Fact]
        public void Timezone_must_be_trimmed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "  Asia/Tehran  ";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            // Act
            UserSettings userSettings = new(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            // Assert
            Assert.Equal("Asia/Tehran", userSettings.Timezone);
        }

        [Fact]
        public void Timezone_must_be_trimmed_when_updating()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            UserSettings userSettings = new(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            var newTimeZone = "  Europe/London  ";

            // Act
            userSettings.Update(newTimeZone, notificationPreferences, calendarIntegrationToken);

            // Assert
            Assert.Equal("Europe/London", userSettings.Timezone);
        }

        [Fact]
        public void NotificationPreferences_is_required()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            CalendarIntegrationToken? calendarIntegrationToken = null;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => new UserSettings(userId, timeZone, null!, calendarIntegrationToken));
            
            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.NotificationPreferencesRequired, domainError.Code);
        }

        [Fact]
        public void NotificationPreferences_is_required_when_updating()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var timeZone = "Asia/Tehran";
            var notificationPreferences = NotificationPreferences.Create(true, false, true, false, 30, false, true, false);
            CalendarIntegrationToken? calendarIntegrationToken = null;

            var userSettings = new UserSettings(userId, timeZone, notificationPreferences, calendarIntegrationToken);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => userSettings.Update(timeZone, null!, calendarIntegrationToken));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.NotificationPreferencesRequired, domainError.Code);
        }

        public static TheoryData<CalendarIntegrationToken?> ValidCalendarIntegrationTokens => 
            [
              null,
              CalendarIntegrationToken.Create(
                  CalendarProvider.Google, 
                  "valid-access-token",
                  "valid-refresh-token", 
                  DateTimeOffset.UtcNow.AddMinutes(30))
            ];
        
        public static TheoryData<string> InvalidTimezones =>
            [
                new string('a', 101), // 101 characters
            ];

    }
}
