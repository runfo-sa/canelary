# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Canelary is a WPF desktop IDE for creating, editing, comparing, and validating ZPL labels (`net10.0-windows`, Prism modular architecture). See `README.md` for a module overview and `REQUIREMENTS.md` for the authoritative, per-module requirements spec (architecture, all modules, and services) — read it before making non-trivial changes so behavior stays spec-compliant.

## Build

```
dotnet build Canelary.sln
```

Run/debug via the `Main` project (the only module compiled into the executable); it dynamically loads the other modules at runtime through Prism.

There is no test project in the solution currently.

## Architecture

- **Modules vs. Services**: Screens (`Editor`, `Comparator`, `Cohere` (Verificador), `Publish`) are Prism `IModule` implementations loaded on demand (`OnDemand = true`). Services (`Backend.Twins`, `Preview.Labelary`, `Version.Git`, `Version.Database`) implement one of three swappable interfaces defined in `Core/Services`: `IBackend`, `IVersion`, `IPreview`. `Version.Git`/`Version.Database` load at startup since the app needs a version service immediately.
- **Service resolution**: Each interface has a static `*ServiceProvider` class in `Core/Services` (e.g. `BackendServiceProvider`) that holds the active singleton implementation, selected by name-string match against `Settings.yaml`. Requesting a service before it's configured/registered throws `NoServiceException`.
- **Settings**: `Settings.yaml` (YAML via `YamlDotNet`) is loaded as a singleton (`Core/Services/SettingsService`) and drives which modules/services load, DB connection, theme, culture, file extensions, virtual directory grouping, etc. There is no auto-generated default — a missing/invalid file fails at startup.
- **Data**: `IdeDbContext` (SQLite/SQL Server) stores IDE-owned data (rules, rule attributes, rule-label assignments) used by the Cohere/Verificador module. `ServiceDbContext` in `Backend.Twins` queries the external Twins ERP database — these are separate EF Core contexts for separate databases.
- **Cross-module communication**: Prism's event aggregator / regions are used for module interop — e.g. `Publish` fires a `PublishEvent` that the active `IVersion` module (e.g. `Version.Git`) consumes to populate its own view into the `Publicar#Region`.
- **Errors**: Surfaced through the shared `Core/Logger/Logger` and an `ExceptionPopUp` dialog rather than ad hoc try/catch UI.

When adding a new Backend/Preview/Version implementation, implement the corresponding interface in `Core/Services` and register it so it can be selected by name from `Settings.yaml` — follow the pattern of the existing `Backend.Twins`, `Preview.Labelary`, or `Version.Git`/`Version.Database` projects.
