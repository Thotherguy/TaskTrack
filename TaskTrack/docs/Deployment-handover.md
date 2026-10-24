# TaskTrack — staging handover

| Item | Access / evidence |
|---|---|
| Staging HTTPS URL | https://thotherguy-001-site1.gtempurl.com |
| Source | https://github.com/Thotherguy/TaskTrack — public read/clone access |
| Hosting | myASP.NET Windows 60-day free trial, .NET 10 / IIS |
| Database | SQL Server 2022; all four EF migrations applied; 13 tables |
| Build | Clean exported source: Release build, 0 warnings and 0 errors |
| Hosted acceptance | 41 workflow/security checks and 25 additional authorization, cleanup and demo-setup checks passed |

## Tester access

1. Open the HTTPS URL; choose Register, then Login. Passwords require at least 12 characters, uppercase, lowercase, a number and a symbol. New registrations are Members.
2. Obtain the separate reviewer credential file from the group owner. Manager owns **TaskTrack staging demonstration**. The assigned Member can see and update its walkthrough task and add comments. The outsider account is outside this project.
3. Test project/task operations, membership, status updates, dashboard, comments and history. Compare allowed and denied actions using separate sessions. Admin access is reserved for the owner or trusted faculty.
4. Report role, affected URL, steps, expected/actual behavior and sanitized screenshots. Use synthetic data. Agree the assessment window; limit testing to this application and avoid denial-of-service testing without separate permission.

## Role policy

| Role | Permissions |
|---|---|
| Admin | Administration and cross-project reads; write operations still require project ownership |
| Manager | Create projects; manage own projects, memberships and tasks |
| Member | Read joined projects and assigned tasks; update assigned task status and comment |

## Build instructions

Install .NET 10 SDK and SQL Server/LocalDB. From a clean clone:

```text
dotnet tool restore
dotnet restore TaskTrack.slnx
dotnet build TaskTrack.slnx --configuration Release
```

Copy the development configuration example and supply your own database connection, then run:

```text
dotnet ef database update --project TaskTrack --startup-project TaskTrack -- --environment Development
dotnet run --project TaskTrack --launch-profile https
```

See README for setup details and the security test plan for repeatable cases. No hosted passwords, local database files or actual developer configuration are included in source.

## Architecture and validation

Browser → HTTPS / IIS → ASP.NET Core MVC + Identity → EF Core → SQL Server 2022.

| Validation area | Result |
|---|---|
| HTTPS, health, home, login and registration pages | Passed |
| New registration defaults / submitted-role tampering | Passed: new users remain Member |
| Owner CRUD, membership and assignment | Passed |
| Assigned-task visibility and unrelated ID access | Passed |
| Comment encoding, validation, authors and pagination | Passed |
| Anti-forgery enforcement, role access and owner-only writes | Passed |
| Status history, activity, dashboard and membership revocation | Passed |
| Disposable project/task deletion | Passed; test fixtures removed |
| Demonstration dataset | One synthetic project, two tasks, assigned reviewer Member |

## Limits and operations

- Trial hosting expires. Check the panel's expiry date and maintain availability throughout assessment. No paid upgrade has been enabled.
- Staging allows synthetic accounts without confirmed email. Email delivery and password recovery are not configured.
- These acceptance checks are not a full vulnerability assessment. Use the detailed security test plan for peer assessment.
- Database credentials reside only in the hosted configuration. Keep owner Admin credentials separate from reviewer credentials.
- Temporary stdout startup logging was disabled after diagnosis; retained logs are under IIS-protected App_Data.
- The earlier Azure resources remain separate; the myASP.NET URL above is the verified staging target. Source includes a build-workflow template, but no running CI pipeline is claimed.
