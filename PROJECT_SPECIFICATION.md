# Date Matcher – Design Overview

## Overview

Date Matcher is an ASP.NET Core (.NET 10) web application that finds month/year combinations where a specified day of the month falls on a given weekday within a year range.

The application also logs each search request and its result to a SQLite database for future review.

---

## Architecture

The solution follows a simple layered architecture:

* **Domain** – Entities
* **Application** – Business logic, validation, DTOs
* **Infrastructure** – EF Core, SQLite, repositories
* **Web** – Razor Pages, API controllers, middleware

This keeps business logic independent from the UI and data access.

---

## Main Components

### DateMatchingService

Contains the core matching algorithm.

Responsibilities:

* Validate calendar dates
* Find matching month/year combinations
* Return formatted results

---

### Logging Middleware

Intercepts search API requests.

Responsibilities:

* Measure execution time
* Capture request and response
* Store search information in the database

Keeping logging outside the business logic keeps the matching service focused on one responsibility.

---

### Repository

The application uses a dedicated `SearchLogRepository` for persisting and reading search logs through EF Core.

---

## Validation

Input validation is implemented using FluentValidation.

Rules include:

* Valid year range
* Start year must not exceed end year
* Day between 1 and 31
* Valid weekday

---

## Technologies

* .NET 10
* ASP.NET Core
* Razor Pages
* REST API
* Entity Framework Core
* SQLite
* FluentValidation
* IMemoryCache

---

## Design Decisions

Some intentional decisions made during the implementation:

* Layered architecture instead of introducing unnecessary complexity.
* SQLite for simple local setup and easy evaluation.
* Middleware for centralized request logging.
* Thin controllers with business logic contained in services.
* Repository pattern for data access.

---



