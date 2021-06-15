### Important - While Switching from MySQL to MSSQL via EFcore, make sure to delete all the migrations, and re-add migrations via the below CLI Command.
### Navigate to `src\Modules.{Module_Name}.Infrastructure` and execute the following.
dotnet ef migrations add {commitMessage} --startup-project ../../../API -o Persistence/Migrations/ --context {Module}DbContext