# AGENTS.md for HouseholdBook (.NET 9 + ASP.NET Core MVC)

## Build Commands
- `dotnet build` - Build the project
- `dotnet run` - Run the development server (https://localhost:7xxx)
- `dotnet publish -c Release` - Publish for deployment

## Lint/Test Commands
- No specific linting configured (use `dotnet format` for code formatting if needed)
- No unit tests configured yet. Add with `dotnet new xunit` and run `dotnet test`
- For single test: `dotnet test --filter "TestName~TestMethodName"`
- Run all tests: `dotnet test`

## Code Style Guidelines
- **Language**: C# 12 (.NET 9)
- **Framework**: ASP.NET Core MVC with Entity Framework Core (SQLite)
- **Naming Conventions**: PascalCase for classes/methods/properties, camelCase for locals/parameters
- **Imports**: Use explicit using statements; group System, then third-party, then local
- **Formatting**: Use built-in .editorconfig (4-space indentation, auto-formatting)
- **Types**: Use nullable reference types (enabled). Prefer decimal for money, DateTime for dates
- **Error Handling**: Use try-catch for DbUpdateConcurrencyException in controllers; return NotFound() for missing entities
- **Models**: Use DataAnnotations for validation ([Required], [StringLength])
- **Views**: Razor syntax, Bootstrap 5 for styling, Chart.js for graphs
- **Database**: SQLite via EF Core migrations (`dotnet ef migrations add`, `dotnet ef database update`)
- **Security**: Use [ValidateAntiForgeryToken] on POST actions; HTTPS redirection enabled
- **Conventions**: Follow MVC patterns; inject DbContext via DI; use async/await for DB operations

## Additional Notes
- Database file: householdbook.db (in app root)
- Charts: Chart.js CDN in MonthlySummary view
- Navigation: Updated _Layout.cshtml with links to features
- No authentication implemented (add Identity if needed)