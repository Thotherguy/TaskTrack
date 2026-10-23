# TaskTrack: Azure readiness notes

No Azure resources have been created or deployed. These are preparation tasks for a separate deployment step.

## Current local state

- .NET 10 MVC application with SQL Server LocalDB and ASP.NET Core Identity.
- Development registration allows sign-in without email confirmation and uses the default email sender. This is a local testing configuration, not a completed public registration setup.
- Admin/Manager/Member roles, owner-only mutations, membership visibility, default Member registration, comments, and activity are implemented.
- The local database includes AddTaskCommentsAndActivity. Generate and review a deployment migration script/bundle against the actual staging database before publishing.
- No administrator password or automatic privilege bootstrap is stored in application source.

## Deployment decisions and checks

1. Choose the Azure subscription, resource group, region, budget, App Service OS/plan, and staging hostname. Confirm the runtime supports the project's .NET 10 target, or select a self-contained publishing approach.
2. Provision Azure SQL and configure least-privilege access. Replace the LocalDB connection through the hosting environment's ConnectionStrings:DefaultConnection setting. Keep connection credentials out of source and deployment logs. Decide whether managed identity is used.
3. Test the migration against a backup or staging copy. Apply reviewed SQL or a migration bundle as a controlled deployment action. Do not delete/recreate an existing database as a migration shortcut.
4. Implement a real IEmailSender and review RequireConfirmedAccount before public registration. Validate confirmation and password recovery end to end. Existing unconfirmed development accounts need an explicit transition plan.
5. Configure durable Data Protection keys with appropriate access controls. Plan for cookie/session behavior across restarts, multiple instances, and deployment slots.
6. Require HTTPS and verify the actual deployed host, certificate, cookies, and production exception handling. Review AllowedHosts for the selected hostname and verify proxy/forwarded-header behavior for the chosen hosting path.
7. Enable operational logging and alerting without logging credentials, connection strings, anti-forgery tokens, or comment bodies. Define data retention and backup/restore procedures. Project activity is not a substitute for operational/security logs.
8. Re-run Security-test-plan.md against staging with approved test accounts, including stale-session demotion, membership revocation, forged IDs, and missing anti-forgery tokens. Add performance/concurrency and abuse-rate testing before inviting external testers.
9. Review comment moderation/retention, member lookup abuse prevention, administrator MFA, login/registration throttling, and account-deletion ownership behavior before broader public use.
10. Agree on migration rollback, release verification, ownership of the initial Admin, and how deployment is approved. Publish only after the local changes and staging configuration have been reviewed.

## Official references

- [ASP.NET Core and Azure SQL on App Service](https://learn.microsoft.com/en-us/azure/app-service/tutorial-dotnetcore-sqldb-app)
- [Applying EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Identity account confirmation and password recovery](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-10.0)
- [Data Protection key management and lifetime](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings)
- [Configure ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview)
