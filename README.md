# SafeVault

A security-focused ASP.NET Core API built as part of a Coursera course, demonstrating secure authentication, role-based authorization, and input sanitization.

## Tech Stack

- ASP.NET Core (.NET 9) minimal API
- ASP.NET Core Identity with role-based authorization (`Admin`, `User`)
- JWT bearer authentication
- Entity Framework Core (in-memory provider)
- NUnit + FluentAssertions for tests

## What It Demonstrates

- Secure user registration with role assignment
- Login that issues a JWT token
- Authorization policies: `/admin-only` requires the `Admin` role, `/user-only` requires the `User` role
- `Helpers/InputSanitizer.cs` — strips script/HTML tags and common SQL/XSS injection characters
- `Repositories/UserRepository.cs` — parameterized SQL queries (protects against SQL injection)
- `Tests/` — unit tests for the sanitizer and repository logic

## Getting Started

### Prerequisites

- .NET 9 SDK

### Run the API

```bash
dotnet run
```

The API listens on `http://localhost:5279` (or `https://localhost:7257`). An admin user is seeded at startup.

### Endpoints

| Method | Route         | Description                                      |
| ------ | ------------- | ------------------------------------------------ |
| POST   | `/register`   | Register a user with a role                      |
| POST   | `/login`      | Log in and receive a JWT token                   |
| GET    | `/admin-only` | Protected — requires the `Admin` role            |
| GET    | `/user-only`  | Protected — requires the `User` role             |

Send the JWT as a `Bearer` token in the `Authorization` header for protected routes.

### Run the tests

```bash
dotnet test
```

## Notes

- The in-memory database resets on each restart. The `UserRepository` class shows how the same patterns are applied against SQL Server.
- The JWT signing key is hardcoded in `Program.cs` for demonstration purposes. For a production deployment, move it to configuration or user secrets.

## Author

Ali Haydoura