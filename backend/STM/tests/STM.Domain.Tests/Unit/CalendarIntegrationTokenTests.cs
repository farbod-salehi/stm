using STM.Domain.Exceptions;
using STM.Domain.ValueObjects.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class CalendarIntegrationTokenTests
    {
        [Fact]
        public void CalendarIntegrationToken_must_be_created_and_returned_with_valid_inputs()
        {
            // Arrange
            var provider = CalendarProvider.Google;
            var accessToken = "access_token";
            var refreshToken = "refresh_token";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            var calendarIntegrationToken = CalendarIntegrationToken.Create(
                provider,
                accessToken,
                refreshToken,
                expiresAt);

            // Assert
            Assert.Equal(provider, calendarIntegrationToken.Provider);
            Assert.Equal(accessToken, calendarIntegrationToken.AccessToken);
            Assert.Equal(refreshToken, calendarIntegrationToken.RefreshToken);
            Assert.Equal(expiresAt, calendarIntegrationToken.ExpiresAt);
        }

        [Fact]
        public void CalendarProvider_must_be_valid_enum_value()
        {
            // Arrange
            var accessToken = "access_token";
            var refreshToken = "refresh_token";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            CalendarProvider provider = (CalendarProvider)999;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CalendarIntegrationToken.Create(
                provider,
                accessToken,
                refreshToken,
                expiresAt));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.InvalidCalendarProvider, domainError.Code);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [Theory]
        public void AccessToken_is_required(string? accessToken)
        {
            // Arrange
            var provider = CalendarProvider.Google;
            var refreshToken = "refresh_token";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CalendarIntegrationToken.Create(
               provider,
               accessToken!,
               refreshToken,
               expiresAt));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.AccessTokenRequired, domainError.Code);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [Theory]
        public void RefreshToken_is_required(string? refreshToken)
        {
            // Arrange
            var provider = CalendarProvider.Google;
            var accessToken = "access_token";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CalendarIntegrationToken.Create(
               provider,
               accessToken,
               refreshToken!,
               expiresAt));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.UserSettings.RefreshTokenRequired, domainError.Code);
        }

        [Fact]
        public void CalendarIntegrationToken_with_same_values_must_be_equal()
        {
            // Arrange
            var provider = CalendarProvider.Outlook;
            var accessToken = "access_token";
            var refreshToken = "refresh_token";
            var expires = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            var token1 = CalendarIntegrationToken.Create(provider, accessToken, refreshToken, expires);
            var token2 = CalendarIntegrationToken.Create(provider, accessToken, refreshToken, expires);

            // Assert
            Assert.Equal(token1, token2);

        }

        [Fact]
        public void CalendarIntegrationToken_with_different_values_must_not_be_equal()
        {
            // Arrange
            var provider1 = CalendarProvider.Google;
            var accessToken1 = "access_token_1";
            var refreshToken1 = "refresh_token_1";
            var expires1 = DateTimeOffset.UtcNow.AddHours(1);

            var provider2 = CalendarProvider.Outlook;
            var accessToken2 = "access_token_2";
            var refreshToken2 = "refresh_token_2";
            var expires2 = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            var token1 = CalendarIntegrationToken.Create(provider1, accessToken1, refreshToken1, expires1);
            var token2 = CalendarIntegrationToken.Create(provider2, accessToken2, refreshToken2, expires2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [InlineData(-1, true)]
        [InlineData(-30, true)]
        [InlineData(1, false)]
        [InlineData(30, false)]     
        [Theory]
        public void IsExpired_must_return_correct_value(int expirationOffsetMinutes, bool expected)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationOffsetMinutes);

            // Act
            var token = CalendarIntegrationToken.Create(
                CalendarProvider.Google,
                "access_token",
                "refresh_token",
                expiresAt);

            // Assert
            Assert.Equal(expected, token.IsExpired);
        }

        [Fact]
        public void Multiple_validation_errors_must_be_returned()
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                CalendarIntegrationToken.Create(
                    (CalendarProvider)999,
                    "",
                    "",
                    DateTimeOffset.UtcNow));

            Assert.Equal(3, exception.Errors.Count);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserSettings.InvalidCalendarProvider);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserSettings.AccessTokenRequired);

            Assert.Contains(exception.Errors, e => e.Code == DomainErrors.UserSettings.RefreshTokenRequired);
        }
    }
}
