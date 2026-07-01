# Date Matcher — Project Specification

This document describes the purpose, architecture, conventions, and design decisions for the Date Matcher solution.

---

## 1. Overview

**Date Matcher** is a web application that, given a year range, a day of the month, and a day of the week, returns all matching month–year pairs formatted as `MMM-yyyy` (e.g. `Feb-2025`).

The application also **logs every search request and response** to a SQLite database so entries can be queried and reviewed later.

### Functional requirements mapping

| Requirement | Implementation |
|-------------|----------------|
| Find month–year matches for day + weekday in a year range | `DateMatchingService.FindMatches` |
| Log every request and response to queryable storage | `SearchLogs` table + `SearchRequestLoggingMiddleware` on `POST /api/datematcher` |
| Design for scale (millions of requests/day) | Stateless API, layered architecture — see [§8 Scalability](#8-scalability) |
| Functionality over usability over performance | Core algorithm first; polished UI and logs viewer; optional in-memory caching |
| Keep implementation simple | Layered monolith, no CQRS/MediatR/message buses |

---

## 2. Technology stack

| Component | Choice | Version |
|-----------|--------|---------|
| Runtime | .NET | 10.0 |
| Web framework | ASP.NET Core | 10.x |
| UI | Razor Pages + client-side JavaScript + Bootstrap 5 | — |
| ORM | Entity Framework Core | 10.0.9 |
| Database | SQLite | via EF Core |
| Validation | FluentValidation | 11.11.0 |
| Caching | `IMemoryCache` | via `Microsoft.Extensions.Caching.Memory` |
| API errors | RFC 7807 Problem Details | Built-in |

Package versions are declared in each project's `.csproj` file.

---

## 3. Architecture

### 3.1 Layered architecture

This is a **layered monolith**, not full Clean Architecture. Web references Infrastructure directly for composition-root convenience.

```
┌─────────────────────────────────────────────────────────────┐
│  DateMatcher.Web                                            │
│  Razor Pages · REST API · Middleware · Exception handling   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  DateMatcher.Application                                    │
│  Services · DTOs · Validators · Interfaces · Mappings       │
└──────────────────────────┬──────────────────────────────────┘
                           │
         ┌─────────────────┴─────────────────┐
         ▼                                   ▼
┌─────────────────┐               ┌─────────────────────────┐
│ DateMatcher.    │               │ DateMatcher.Infrastructure │
│ Domain          │               │ EF Core · Repositories     │
│ Entities        │               │ Migrations · Options       │
└─────────────────┘               └─────────────────────────┘
```

**Dependency rule:** Domain has no dependencies. Application depends on Domain. Infrastructure implements Application interfaces. Web references Application and Infrastructure.

### 3.2 Request flow — search (UI)

The Index page is a **GET-only Razor shell**. Form submission is handled in the browser via `wwwroot/js/index.js`.

```
User submits form (JavaScript)
    → POST /api/datematcher (JSON)
    → SearchRequestLoggingMiddleware buffers request/response
    → FluentValidation (auto, on DateMatchRequestDto)
    → CachedDateMatchingService → DateMatchingService.FindMatches
    → JSON response (200 or 400 Problem Details)
    → Middleware persists SearchLog to SQLite
    → Browser renders result chips or validation errors
```

### 3.3 Request flow — search (API)

```
POST /api/datematcher
    → SearchRequestLoggingMiddleware (timing, body capture)
    → FluentValidation (auto)
    → CachedDateMatchingService → DateMatchingService.FindMatches
    → JSON response
    → Middleware persists SearchLog (including malformed bodies)
```

### 3.4 Request flow — logs page

```
GET /Logs?PageNumber=&SortBy=&SortDescending=
    → SearchLogRepository.GetPagedAsync (sorted, paginated)
    → Razor renders table

User clicks "View" on a row
    → GET /api/searchlogs/{id} (JavaScript)
    → Modal shows formatted response JSON
```

---

## 4. Project structure

### DateMatcher.Domain

| File | Purpose |
|------|---------|
| `Entities/SearchLog.cs` | Audit log entity |

### DateMatcher.Application

| Area | Purpose |
|------|---------|
| `Services/DateMatchingService.cs` | Core matching algorithm |
| `Services/CachedDateMatchingService.cs` | Decorator; caches `FindMatches` results in `IMemoryCache` |
| `Interfaces/` | `IDateMatchingService`, `ISearchLogRepository` |
| `DTOs/` | Request/response, log list items, sort options |
| `Validators/DateMatchRequestValidator.cs` | Input validation rules |
| `Common/ValidationConstants.cs` | Shared min/max year and day bounds |
| `Common/PagedResult.cs` | Paginated query result |
| `Mappings/` | Entity ↔ DTO conversions |
| `ServiceCollectionExtensions.cs` | Registers validators, services, memory cache |

### DateMatcher.Infrastructure

| Area | Purpose |
|------|---------|
| `Persistence/AppDbContext.cs` | EF Core context |
| `Persistence/Repositories/SearchLogRepository.cs` | Log persistence and queries |
| `Options/DatabaseOptions.cs` | Connection string configuration |
| `Persistence/Migrations/` | Database schema |
| `ServiceCollectionExtensions.cs` | DbContext, repository, migrations helper |

### DateMatcher.Web

| Area | Purpose |
|------|---------|
| `Pages/Index.cshtml` | Search UI (client-side API consumer) |
| `Pages/Logs.cshtml` | Log viewer with sorting and pagination |
| `Controllers/DateMatcherController.cs` | `POST /api/datematcher` |
| `Controllers/SearchLogsController.cs` | `GET /api/searchlogs/{id}` |
| `Controllers/HealthController.cs` | `GET /api/health` |
| `Middleware/SearchRequestLoggingMiddleware.cs` | Request/response logging |
| `Infrastructure/UnhandledExceptionHandler.cs` | Unhandled errors → 500 or `/Error` |
| `Infrastructure/ApiProblemDetailsWriter.cs` | Consistent Problem Details JSON |
| `ServiceCollectionExtensions.cs` | FluentValidation auto-validation |
| `wwwroot/js/index.js` | Search form → API |
| `wwwroot/js/logs.js` | Modal lazy-load of response JSON |

---

## 5. Core concepts and patterns

### 5.1 Dependency Injection

Each layer registers services via extension methods:

- `AddApplication()` — validators, `DateMatchingService`, `CachedDateMatchingService`, memory cache
- `AddInfrastructure(configuration)` — DbContext, repository, options
- `AddWebPresentation()` — FluentValidation auto-validation

Constructor injection is used throughout (primary constructors in C# 12).

### 5.2 Matching service

`IDateMatchingService.FindMatches` returns `DateMatchResponseDto` directly. Validation is handled at the **HTTP boundary** (FluentValidation); the service focuses on matching logic only.

`CachedDateMatchingService` wraps `DateMatchingService` using a decorator pattern:

- Cache key: `matches:{startYear}:{endYear}:{dayOfMonth}:{dayOfWeek}`
- TTL: 1 day; entry size 1; global size limit 500 entries

### 5.3 Repository pattern

`ISearchLogRepository` is a **specific repository** (not generic):

- `AddAsync` — insert a log entry
- `GetByIdAsync` — load one entry with full response JSON
- `GetPagedAsync` — sorted, paginated reads for the Logs page

### 5.4 DTOs (Data Transfer Objects)

| DTO | Role |
|-----|------|
| `DateMatchRequestDto` | Search input |
| `DateMatchResponseDto` | Search output (`Matches` list) |
| `SearchLogListItemDto` | Log row for UI / API detail |
| `SearchLogSortDto` | Sort column and direction for log queries |

Domain entity `SearchLog` is not exposed to the UI directly.

### 5.5 FluentValidation

Rules live in `DateMatchRequestValidator`:

- Years: 1–9999, start ≤ end
- Day of month: 1–31
- Day of week: valid enum value

Auto-validation runs on API controllers. The Razor Index page delegates validation to the API.

Invalid calendar dates (e.g. Feb 30) are skipped by the matching algorithm rather than rejected at validation.

### 5.6 Middleware pipeline

Order in `Program.cs`:

1. `ApplyMigrationsAsync()` — runs EF migrations at startup
2. `UseExceptionHandler` — unhandled exceptions via `UnhandledExceptionHandler`
3. `UseHttpsRedirection` (when HTTPS is configured)
4. `UseRouting`
5. `SearchRequestLoggingMiddleware` — captures `POST /api/datematcher`, persists logs

The logging middleware:

- Intercepts `POST /api/datematcher` only
- Buffers the response body
- Parses the request JSON into search criteria when possible
- Logs unparseable or empty bodies with `Success = false`
- Records execution time, success flag, response JSON, and error message
- Swallows persistence failures so the user response is not affected

### 5.7 API error handling (RFC 7807)

| Scenario | Response |
|----------|----------|
| Validation failure | `400` `ValidationProblemDetails` |
| Unhandled server error (API) | `500` `ProblemDetails` |
| Unhandled server error (page) | Redirect to `/Error` |

Content type: `application/problem+json`

### 5.8 Pagination and sorting

`PagedResult<T>` provides `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage`.

Logs page uses page size **10**. Sortable columns: `#`, When, Criteria, Status, Duration. Sort state is preserved in query string via `LogsSortHelper`.

### 5.9 Database schema — SearchLogs

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| StartYear, EndYear | int | Search criteria (0 when body unparseable) |
| DayOfMonth | int | Search criteria |
| DayOfWeek | int | Search criteria (enum) |
| ResponseJson | text | Serialized API response |
| Success | bool | Whether search succeeded |
| ErrorMessage | text (max 2000) | Validation or error summary |
| ExecutionTimeMs | long | Middleware timing |
| CreatedAt | datetime | UTC timestamp |

---

## 6. Matching algorithm

`DateMatchingService` iterates each year from `StartYear` to `EndYear`, then each month 1–12:

1. Skip months where `DayOfMonth` exceeds `DateTime.DaysInMonth(year, month)` (e.g. Feb 30)
2. Build `DateTime(year, month, dayOfMonth)`
3. If `DayOfWeek` matches, add `date.ToString("MMM-yyyy", InvariantCulture)`

Complexity: O(years × 12) — acceptable for bounded year ranges (max 9999).

---

## 7. Code style and conventions

### 7.1 General C# conventions

- **Target:** .NET 10, nullable reference types enabled, implicit usings
- **Naming:** PascalCase for types/methods/properties; camelCase for locals and parameters
- **Files:** One primary type per file; file name matches type name
- **Constructors:** Primary constructors where appropriate
- **Namespaces:** Match folder structure (`DateMatcher.Application.Services`)

### 7.2 Layer responsibilities

| Layer | May contain | Must not contain |
|-------|-------------|------------------|
| Domain | Entities | EF, HTTP, validation frameworks |
| Application | Business logic, DTOs, interfaces, validators | DbContext, Razor, middleware |
| Infrastructure | EF, repositories, migrations, options | UI concerns |
| Web | Pages, controllers, middleware, DI setup | Direct SQL, business rules |

### 7.3 Validation

- Single source of truth: `DateMatchRequestValidator` + `ValidationConstants`
- API uses FluentValidation auto-validation
- UI relies on the same API validation path

### 7.4 Async

- Repository and page handlers use `async`/`await` with `CancellationToken`
- `DateMatchingService.FindMatches` is synchronous (CPU-bound, fast)

### 7.5 Configuration

- Connection strings in `appsettings.json` under `DatabaseOptions`
- `DatabaseOptions` class lives in **Infrastructure**
- Migrations run automatically at application startup via `ApplyMigrationsAsync()`

### 7.6 UI conventions

- Shared layout: `_Layout.cshtml` with nav (Search, Logs)
- CSS variables in `wwwroot/css/site.css`
- Bootstrap 5 for grid, forms, modal, pagination
- Empty states for no data / no results
- Client-side validation errors displayed in `#searchErrors`

---

## 8. Scalability

Current implementation prioritizes **simplicity and correctness**. The design supports future scale:

| Concern | Current | At high scale |
|---------|---------|---------------|
| Hosting | Single process | Multiple stateless instances behind load balancer |
| Database | SQLite file | PostgreSQL / SQL Server with connection pooling |
| Log writes | Synchronous per request | Background queue or batch inserts |
| Cache | In-memory per instance | Distributed cache (e.g. Redis) |
| Middleware | Buffers full response | Log structured DTOs without stream buffering |
| Migrations | Startup auto-migrate | CI/CD migration step |
| Observability | DB logs + ASP.NET logging | Metrics, distributed tracing |

The **REST API** is the primary integration surface; the Razor UI is a thin client that could be replaced without changing core logic.

---

## 9. API reference

### POST /api/datematcher

**Request body:**

```json
{
  "startYear": 2025,
  "endYear": 2027,
  "dayOfMonth": 15,
  "dayOfWeek": "Saturday"
}
```

`dayOfWeek` accepts enum string values (`Sunday` … `Saturday`).

**Response 200:**

```json
{
  "matches": ["Feb-2025", "Mar-2025"]
}
```

**Response 400:** Problem Details with `errors` object.

### GET /api/searchlogs/{id}

Returns a single `SearchLogListItemDto` including formatted `responseJson`. Returns `404` when not found.

### GET /api/health

```json
{ "status": "healthy" }
```

Liveness only — does not verify database connectivity.

---

## 10. Logs page

| Feature | Description |
|---------|-------------|
| Pagination | 10 entries per page |
| Sorting | By id, created time, criteria, status, duration |
| Response detail | Lazy-loaded via `GET /api/searchlogs/{id}` modal |

---

## 11. Known limitations

- No automated test project (manual verification recommended for the spec example)
- SQLite is not ideal for high-concurrency production writes
- Log `ResponseJson` is unbounded in size (large result sets could grow storage)
- Health endpoint does not verify database connectivity
- In-memory cache is not shared across multiple instances

---

## 12. Running and troubleshooting

```bash
dotnet run --project src/DateMatcher.Web
```

| Issue | Check |
|-------|-------|
| Database not created | Ensure the app can write to the configured SQLite path |
| Port in use | Change URLs in `Properties/launchSettings.json` |
| Empty logs | Submit at least one search via the UI or API |
| Docker data lost | Ensure the `datematcher-data` volume is mounted |

---

## 13. File map (application code)

```
src/
├── DateMatcher.Domain/
│   └── Entities/SearchLog.cs
├── DateMatcher.Application/
│   ├── Common/          PagedResult, ValidationConstants
│   ├── DTOs/            Request, response, sort, log list item
│   ├── Interfaces/      IDateMatchingService, ISearchLogRepository
│   ├── Mappings/        SearchLogMappings, SearchLogListMappings
│   ├── Services/        DateMatchingService, CachedDateMatchingService
│   ├── Validators/      DateMatchRequestValidator
│   └── ServiceCollectionExtensions.cs
├── DateMatcher.Infrastructure/
│   ├── Options/         DatabaseOptions
│   ├── Persistence/     AppDbContext, Migrations, Repositories
│   └── ServiceCollectionExtensions.cs
└── DateMatcher.Web/
    ├── Controllers/     DateMatcherController, SearchLogsController, HealthController
    ├── Middleware/      SearchRequestLoggingMiddleware
    ├── Infrastructure/  Exception handler, Problem Details writer
    ├── Pages/           Index, Logs, Error, LogsSortHelper
    ├── wwwroot/js/      index.js, logs.js
    ├── ServiceCollectionExtensions.cs
    └── Program.cs
```

---

*Last updated to reflect the solution as delivered for the Date Matcher software test.*
