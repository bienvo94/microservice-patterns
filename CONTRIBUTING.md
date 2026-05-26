# Contributing to Microservice Patterns

Thank you for your interest in contributing! This educational repository demonstrates production-inspired microservice patterns on .NET 10 with .NET Aspire 13.

## How to Add a New Pattern

### 1. Create the folder structure

```
patterns/[PatternName]/
├── [PatternName].[ServiceName]/
│   ├── Program.cs               # DI registration and Aspire integration
│   ├── Apis/ or Endpoints/      # Minimal API route handlers
│   ├── Services/ or Handlers/   # Business logic
│   ├── Models/ or Dtos/         # Domain models and DTOs
│   ├── Infrastructure/          # DbContext, event bus adapters
│   └── Migrations/              # EF Core migrations (if applicable)
└── README.md                    # Pattern documentation (required)
```

### 2. Wire it into Aspire AppHost

Add your service(s) to `MicroservicePatterns.AppHost/Extensions/` with an appropriate extension method. Follow the existing pattern of grouping related services.

### 3. Write a README

Every pattern must include a `README.md` with:
- **Problem statement** — What problem does this pattern solve?
- **Solution overview** — How does it solve the problem?
- **Service descriptions** — What each service does
- **API endpoints** — Curl examples for testing
- **Flow diagram** — Step-by-step message/data flow
- **Trade-offs** — When to use / when not to use
- **Common pitfalls** — What can go wrong

### 4. Add tests

- **Integration tests** go in `tests/IntegrationTests/`
- **Unit tests** go in `tests/[PatternName].UnitTests/`
- Use xUnit SDK v3 with `IAsyncLifetime` for async setup/teardown
- Use Moq for mocking

## Coding Standards

- **C# 13** features preferred (file-scoped namespaces, primary constructors, collection expressions)
- **Nullable reference types** enabled; use `is null` / `is not null`
- **Pattern matching** and switch expressions over `if`/`else` chains
- **`nameof(...)`** instead of string literals for member names
- **XML doc comments** on all public APIs
- Follow the `.editorconfig` in the repository root

## Building and Testing

```bash
# Restore and build
dotnet restore
dotnet build

# Run all services
dotnet run --project MicroservicePatterns.AppHost

# Run tests
dotnet test tests/IntegrationTests/IntegrationTests.csproj
dotnet test tests/Saga.UnitTests/Saga.UnitTests.csproj
```

## Pull Request Checklist

- [ ] Code follows the C# coding standards above
- [ ] Pattern has a comprehensive README.md
- [ ] New services are wired into the Aspire AppHost
- [ ] Tests pass locally
- [ ] No new compiler warnings
- [ ] XML docs added for public APIs
