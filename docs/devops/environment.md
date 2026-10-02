## Document things like:

- ASPNETCORE_ENVIRONMENT
- ConnectionStrings
- JWT settings
- Email settings
- Google Calendar settings
- Azure settings
- Frontend environment variables

### Example:

This document describes the environment variables required to run STM locally and in production.

#### Backend

| Variable                               | Required | Description                                                |
| -------------------------------------- | -------- | ---------------------------------------------------------- |
| ASPNETCORE_ENVIRONMENT                 | Yes      | Application environment (Development, Staging, Production) |
| ConnectionStrings\_\_DefaultConnection | Yes      | SQL Server connection string                               |
| Jwt\_\_Key                             | Yes      | JWT signing key                                            |
| Jwt\_\_Issuer                          | Yes      | JWT token issuer                                           |
| Jwt\_\_Audience                        | Yes      | JWT token audience                                         |
| Google\_\_ClientId                     | No       | Google OAuth Client ID                                     |
| Google\_\_ClientSecret                 | No       | Google OAuth Client Secret                                 |
| GoogleCalendar\_\_Enabled              | No       | Enable Google Calendar integration                         |

#### Frontend

| Variable         | Required | Description            |
| ---------------- | -------- | ---------------------- |
| API_URL          | Yes      | Backend API URL        |
| GOOGLE_CLIENT_ID | No       | Google OAuth Client ID |

#### Local Development Example

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=.;Database=STM;Trusted_Connection=True
Jwt__Key=your-secret-key
Jwt__Issuer=STM
Jwt__Audience=STM
API_URL=https://localhost:5001
```
