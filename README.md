# Date Matcher

A .NET web application that finds month–year combinations where a given day of the month falls on a specific weekday within a year range. Every search is logged to SQLite for later review.

## Quick start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

```bash
cd DateMatcherSolution
dotnet restore
dotnet run --project src/DateMatcher.Web
```

Open the URL shown in the console (typically `https://localhost:7295`).

On first run, EF Core migrations are applied automatically and `DateMatcher.db` is created in the Web project folder.

### Docker

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose)

**Run with Compose (recommended):**

```bash
cd DateMatcherSolution
docker compose up --build
```

Open **http://localhost:8080**

The SQLite database is stored in a Docker volume (`datematcher-data`) so data persists across restarts.

**Stop:**

```bash
docker compose down
```

**Run without Compose:**

```bash
cd DateMatcherSolution
docker build -t datematcher .
docker run --rm -p 8080:8080 -v datematcher-data:/app/data datematcher
```

**Useful commands:**

```bash
# Run in the background
docker compose up --build -d

# View logs
docker compose logs -f web

# Rebuild after code changes
docker compose up --build
```

### Example search

| Field | Value |
|-------|-------|
| Start year | 2025 |
| End year | 2027 |
| Day of month | 15 |
| Day of week | Saturday |

**Expected results:** Feb-2025, Mar-2025, Nov-2025, Aug-2026, May-2027

### API

```http
POST /api/datematcher
Content-Type: application/json

{
  "startYear": 2025,
  "endYear": 2027,
  "dayOfMonth": 15,
  "dayOfWeek": "Saturday"
}
```

**Success (200):**

```json
{
  "matches": ["Feb-2025", "Mar-2025", "Nov-2025", "Aug-2026", "May-2027"]
}
```

**Validation error (400):** RFC 7807 Problem Details (`application/problem+json`)

**Health check:** `GET /api/health`

**Log detail:** `GET /api/searchlogs/{id}` — returns full response JSON for a logged search

### Pages

| Route | Description |
|-------|-------------|
| `/` | Search form and results (calls the API via JavaScript) |
| `/Logs` | Paginated search log table with sortable columns |

The search UI is a Razor Page shell; submissions go through `POST /api/datematcher` so validation, matching, caching, and logging share one code path.

## Solution structure

```
DateMatcherSolution/
├── src/
│   ├── DateMatcher.Domain/          # Entities
│   ├── DateMatcher.Application/     # Business logic, DTOs, validation, caching
│   ├── DateMatcher.Infrastructure/  # EF Core, SQLite, repositories
│   └── DateMatcher.Web/             # Razor UI, API, middleware
├── README.md
└── PROJECT_SPECIFICATION.md         # Architecture, patterns, and design decisions
```

## Configuration

`src/DateMatcher.Web/appsettings.json`:

```json
{
  "DatabaseOptions": {
    "DefaultConnection": "Data Source=DateMatcher.db"
  }
}
```

## Documentation

See **[PROJECT_SPECIFICATION.md](PROJECT_SPECIFICATION.md)** for architecture, request flows, patterns, API reference, and scalability notes.
