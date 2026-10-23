# TaskTrack

ASP.NET Core MVC project and task management for the Group System Deployment assignment.

## Features

Admin/Manager/Member roles, owner-controlled projects, project membership, assigned tasks, dashboard, comments, and project activity. The project includes all EF migrations. The repository contains no database files or test passwords.

## Build from a clean clone

Requirements: .NET 10 SDK (10.0.401 or a compatible later feature band); SQL Server LocalDB on Windows or a configured SQL Server instance.

```text
dotnet tool restore
dotnet restore TaskTrack.slnx
dotnet build TaskTrack.slnx --configuration Release
```

Build does not require a running database. To run locally, copy `TaskTrack/appsettings.Development.example.json` to `TaskTrack/appsettings.Development.json` and adjust the connection string for your SQL instance. This local file is ignored by Git. Alternatively set ConnectionStrings:DefaultConnection with .NET User Secrets.

```text
dotnet ef database update --project TaskTrack --startup-project TaskTrack -- --environment Development
dotnet run --project TaskTrack --launch-profile https
```

Open the localhost URL printed by the app. Roles are initialized automatically after the schema exists. Registration always creates a Member; it never offers a public role selector. New passwords require 12 characters, uppercase, lowercase, a number, and a symbol.

## Administration

The initial administrator must be assigned through a controlled, one-time database procedure after registering an account. Never commit passwords, connection credentials, bootstrap code, or data-protection keys. A local database and its administrator account are not automatically copied to staging.

## Staging configuration

Set ASPNETCORE_ENVIRONMENT=Staging and supply ConnectionStrings:DefaultConnection in the hosting environment. LocalDB is rejected outside Development. Review `TaskTrack/docs/Staging-deployment.md` before deploying. The staging registration policy allows synthetic test accounts without email confirmation; email delivery/password-recovery service configuration is not included.

The database must be migrated before the first application start. `/healthz` reports application/database availability without returning connection details.

## Testing and assessment

- [Security test plan](TaskTrack/docs/Security-test-plan.md)
- [Azure readiness](TaskTrack/docs/Azure-readiness.md)
- [Staging deployment](TaskTrack/docs/Staging-deployment.md)

Only the agreed staging hostname, synthetic test accounts, and authorized testing window are in scope. Do not test hosting infrastructure, classmates' unrelated accounts, or production services. Do not perform denial-of-service testing without separate agreement.

Repository access for reviewers should be read-only. Staging URL and reviewer instructions will be added after deployment verification.
