# Azure deployment runbook

The verified assessment deployment uses myASP.NET. See [Deployment handover](Deployment-handover.md). This document records the alternative Azure deployment procedure.

## Environment

Target: Azure App Service (.NET 10) and Azure SQL, in the existing Azure for Students subscription. Use only free resources or the agreed student-credit allowance. Do not convert the subscription, enable paid overage, or create a paid tier without approval.

## Deployment order

1. Choose a dedicated resource group and confirm region, subscription offer, pricing tier, quota, and credit/spending limits in Azure.
2. Create the staging SQL database. For the free SQL offer, keep free-limit exhaustion set to pause rather than additional charges. An auto-paused database may briefly delay first access; exhaustion can make it unavailable until the quota resets. Verify eligibility for this subscription.
3. Create an App Service plan and web app with .NET 10. Confirm the free/student offer actually shown by Azure. Do not silently substitute a paid tier if the free tier is unavailable. Free plans have usage limits and may not provide continuous availability.
4. Set ASPNETCORE_ENVIRONMENT to Staging, AllowedHosts to the actual app hostname, and ConnectionStrings:DefaultConnection to the hosted SQL connection. Store credentials only in the hosting configuration or approved secret store. Choose a database authentication/network approach and review its access scope.
5. Apply the reviewed idempotent migration SQL to the new staging database before starting the app. Do not upload the developer's LocalDB files or accounts.
6. Deploy the contents of the Release publish folder to the web app. The ZIP must contain TaskTrack.dll at its root. Development configuration is excluded by the project file.
7. Check HTTPS home page, registration, login, /healthz, and all role workflows. Register the trusted staging administrator, then use deploy/InitializeAdmin.sql once with their exact email. Do not put an Admin password into code or CI.
8. Provision synthetic reviewer accounts and a small dataset. Share credentials privately with authorized reviewers. Never commit passwords or authentication cookies.
9. Add verified app/repository URLs and the testing window to the submission document. Run docs/Security-test-plan.md against the hosted URL.

## Registration and account limitations

Staging explicitly permits registration without confirmed email for authorized classroom testing. New accounts are Member. No functioning email sender/password-recovery service has been configured; use synthetic addresses and record this limitation for reviewers. Production defaults to requiring confirmation and needs a real email service before use.

## Source access

The owner authorized public source publication. Source: https://github.com/Thotherguy/TaskTrack . Visitors can read and clone without collaborator write access. Credentials and actual local configuration are excluded.

## Commands

From the repository root:

```text
dotnet tool restore
dotnet restore TaskTrack.slnx
dotnet build TaskTrack.slnx --configuration Release
dotnet ef migrations script --idempotent --project TaskTrack --startup-project TaskTrack --output artifacts/staging-schema.sql -- --environment Development
dotnet publish TaskTrack/TaskTrack.csproj --configuration Release --output artifacts/site
```

The migration command needs the documented local configuration so EF can resolve the context. Publishing does not require a running database. Exclude all appsettings.Development files, database files, secrets, and publish profiles from the package.

## Release evidence

Record hostname, deployment time, deployed commit, build result, migration version, health result, and role-test results. Keep the environment available for the agreed assessment window. Document resource limits and expected cold-start behavior. Budget alerts alone do not enforce a hard spending cap.
