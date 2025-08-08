AGENT.md

# .NET Authentication Server Generator Agent

This Codex agent helps to create the skeleton of a .NET Core project that acts as an OAuth2 authentication server with JWT.

## Objective

Generate the base code for an authentication service in ASP.NET Core, including:

- Endpoints for registration, login, and refresh token
- Issuance of configured JWTs
- Organized structure for controllers, use cases, and persistence
- EF Core configuration for the database
- Azure DevOps pipeline and XUnit tests

## Project Structure

```
/azuredevops
  └─ pipeline.yml

/src
  /NetAuth
    /Controllers
    /UseCases
      /Commands
      /Queries
    /Data
      /Models
      /Configuration
        AuthDbContext.cs
    Program.cs
  /NetAuth.Tests
    └─ Tests for various components in XUnit
```

## Database Schema

The application database should support the following concepts:

### Core Tables

- **Users**: Stores user credentials and profile information.
- **UserGroups**: Defines user roles or groups (e.g., admin, user, guest).
- **UserGroupMemberships**: Many-to-many relation between users and groups.
- **JWT Signing Keys**: Allows key rotation by storing keys with metadata (e.g., creation date, active status).

### OAuth2 Support

- **Clients**: Represents registered applications (e.g., Single Page Applications) with client ID, secrets, and redirect URIs.
- **ClientSecrets**: Stores versionable client secrets.
- **RefreshTokens**: Stores issued refresh tokens to enable session continuation for clients.

### Additional Features

- **LoginAttempts**: Tracks login attempts for auditing and security.
- **UserSessions**: Tracks active sessions and logout events.
- **PasswordResetRequests**: Supports password reset flows via token validation.
- **UserClaims**: Stores additional user-specific claims (e.g., tenant ID, custom limits).

These tables enable core user flows such as registration, password change, and login, along with OAuth2-based authentication and session tracking.

## Conventions

- **Framework**: ASP.NET Core 8.0
- **Use Cases**: implemented with MediatR
- **Database**: Entity Framework Core with `AuthDbContext`
- **Token**: JWT, with keys stored in the database to support annual rotation
- **Test**: XUnit with Moq for mocking
- **Testing**: XUnit with FluentAssertions and NSubstitute for mocking

## Supported Prompts

- `Generate a UseCase Command for user registration with data validation and persistence in DbContext.`
- `Add an AuthController with POST /login endpoint that returns a JWT.`
- `Configure JWT generation in Program.cs reading the secret key from appsettings.json.`
- `Create the UserModel class in Data/Models with fields Id, Username, PasswordHash.`
- `Write an XUnit test using FluentAssertions and NSubstitute to verify the login behavior with valid and invalid credentials.`

## How to Use

1. Open a new Codex request using this agent.
2. Provide the desired prompt among the supported or a custom one.
3. The system will generate files and classes according to the structure and conventions.
4. Integrate the generated components and adapt the business logic as needed.

## Configuration

- Multiple environments are supported; each environment reads from its own `appsettings.{Environment}.json`.
- JWT signing keys are read from the database at runtime to support annual key rotation.

## Monitoring

- Use OpenTelemetry for distributed tracing and metrics.
- Instrument controllers, MediatR handlers, and database operations.

## Migrations

- Database schema changes are managed with Entity Framework Core Migrations.
- Configure automatic migrations on startup or via CLI (`dotnet ef database update`).

## API Documentation

- Generate Swagger/OpenAPI documentation automatically.
- Include XML comments for controllers and models to enhance the Swagger UI.

## CI/CD Pipeline

- The Azure DevOps pipeline (`/azuredevops/pipeline.yml`) performs:
  1. `dotnet restore`
  2. `dotnet build`
  3. `dotnet test`
  4. `docker build` using a generated `Dockerfile` at the solution root
- Use pipeline variables or Key Vault integration for secrets management.

## Use Cases

The system initially supports OAuth2 flows for Single Page Applications (SPAs), including implicit and authorization code with PKCE.

Use cases are organized following the CQRS pattern with MediatR and may include:

- User registration
- User login with JWT issuance
- Token refresh
- Password change
- Retrieving user profile or group information
