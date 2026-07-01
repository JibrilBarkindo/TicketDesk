# Job Application Tracker API

A production-shaped **REST API for tracking job applications** through their lifecycle, built with **ASP.NET Core 8** and **Entity Framework Core**. Each application moves through a defined set of stages, and the API enforces which stage transitions are valid, so it demonstrates real business logic rather than plain create-read-update-delete.

Built as a portfolio project to practise the patterns real .NET teams use: clean layering, a tested service layer, sensible REST semantics, validation, centralised error handling, containerisation, and continuous integration.

---

## What it does

The API manages job applications. Each application records the role, some notes, an interest level (Low / Medium / High / Dream), an optional contact, and a **status** that follows a small state machine:

```
Applied ──> Interviewing ──> Offer ──> Accepted
   │              │            │
   └──────────────┴────────────┴──> Rejected / Withdrawn
```

New applications start as **Applied**. From an active stage you can advance to the next stage, or exit via **Rejected** or **Withdrawn**. **Accepted, Rejected, and Withdrawn are terminal**, so once an application reaches one of them, no further status changes are allowed. The status endpoint enforces these rules and rejects any invalid transition, which is where the core business logic lives.

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 8 (Web API, controllers) |
| Persistence | EF Core 8 with SQLite (file-based, zero setup) |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Validation | DataAnnotations with automatic 400 responses |
| Errors | Custom middleware producing RFC 7807 ProblemDetails |
| Tests | xUnit with the EF Core in-memory provider and FluentAssertions |
| Container | Multi-stage Dockerfile |
| CI | GitHub Actions (restore, build, test on every push) |

## Architecture

The request flows through clearly separated layers, each with a single responsibility:

```
HTTP ──> Controller ──> Service (business rules) ──> DbContext ──> SQLite
              │              │
              │              └─ maps entities <-> DTOs
              └─ thin: no logic, just shapes the HTTP response
```

- **Controllers** are deliberately thin. They validate input and delegate to the service.
- **The service layer** owns the business rules, including the status state machine, and is the part covered by unit tests.
- **DTOs** keep the public API contract separate from the database entities.
- **Middleware** turns domain exceptions into consistent error responses, so controllers never need try/catch blocks.
- **TimeProvider** is injected rather than calling `DateTime.UtcNow` directly, which keeps time-dependent logic testable.

## Running locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet run --project src/TicketDesk.Api
```

Then open the `/swagger` URL printed in the console (for example `http://localhost:5000/swagger`) to explore the API. A local SQLite database file is created automatically on first run.

## Running the tests

```bash
dotnet test
```

The suite covers the service layer: creation defaults, not-found handling, valid and invalid status transitions, and filtered pagination.

## Running with Docker

```bash
docker build -t application-tracker .
docker run -p 8080:8080 application-tracker
```

The API is then available on `http://localhost:8080`.

## API reference

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/applications` | List applications (filter by `status`, `priority`, `assignedTo`; paged via `page`, `pageSize`) |
| `GET` | `/api/applications/{id}` | Get one application |
| `POST` | `/api/applications` | Create an application (starts as `Applied`) |
| `PUT` | `/api/applications/{id}` | Update an application's editable fields |
| `PATCH` | `/api/applications/{id}/status` | Change status (enforces the state machine) |
| `DELETE` | `/api/applications/{id}` | Delete an application |

### Example: create an application

```bash
curl -X POST http://localhost:5000/api/applications \
  -H "Content-Type: application/json" \
  -d '{ "title": "Junior .NET Developer at Acme", "description": "Applied via LinkedIn", "priority": "High" }'
```

### Example: advance the status

```bash
curl -X PATCH http://localhost:5000/api/applications/{id}/status \
  -H "Content-Type: application/json" \
  -d '{ "status": "Interviewing" }'
```

Attempting an invalid transition (for example moving a `Rejected` application back to `Interviewing`) returns a `400 Bad Request` explaining that the change is not allowed.

## Design decisions worth calling out

- **SQLite with `EnsureCreated()`** keeps the project clone-and-run. In a real deployment this would be replaced with **EF Core migrations** run at deploy time.
- **Enums are stored as strings** in the database, so rows stay human-readable and reordering an enum cannot corrupt existing data.
- **Exceptions are limited** to a couple of domain exception types mapped centrally by middleware; everything else uses ordinary return values.

## Possible next steps

Deliberately left as extensions:

- Add **EF Core migrations** and a seed step
- Add **integration tests** using `WebApplicationFactory`
- Add **authentication** so each user tracks their own applications
- Swap DataAnnotations for **FluentValidation**
- Add **structured logging** (Serilog) and a correlation id

---

## Notes

Built as a solo learning project to practise production-shaped .NET patterns. The structure and decisions are explained above and are mine to walk through.
