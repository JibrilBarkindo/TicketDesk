# TicketDesk API

A small but production-shaped **support-ticket tracking API** built with **ASP.NET Core 8** and **Entity Framework Core**. It exists to demonstrate the things real .NET teams care about: clean layering, a tested service layer, sensible REST semantics, validation, centralised error handling, API docs, containerisation, and CI.

> Domain note: tickets are an easy-to-swap example. The same structure works for an inventory service, a booking system, or any CRUD-plus-business-rules API.

---

## What it does

A REST API to manage support tickets. Each ticket has a title, description, **priority** (Low / Medium / High / Critical) and a **status** that follows a small state machine (Open → InProgress → Resolved → Closed). The status endpoint enforces which transitions are legal — e.g. a `Closed` ticket cannot be reopened — so there is real business logic, not just CRUD.

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 8 (Web API, controllers) |
| Persistence | EF Core 8 + SQLite (file-based, zero setup) |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Validation | DataAnnotations + `[ApiController]` automatic 400s |
| Errors | Custom middleware → RFC 7807 `ProblemDetails` |
| Tests | xUnit + EF Core InMemory + FluentAssertions |
| Container | Multi-stage Dockerfile |
| CI | GitHub Actions (restore → build → test) |

## Architecture

The request flows through clearly separated layers, each with one job:

```
HTTP ──> Controller ──> Service (business rules) ──> DbContext ──> SQLite
              │              │
              │              └─ maps entities <-> DTOs
              └─ thin: no logic, just shapes the HTTP response
```

- **Controllers** are deliberately thin — they validate input (via attributes) and delegate.
- **Services** (`ITicketService` / `TicketService`) own the business rules and are the unit-tested core.
- **DTOs** keep the public API contract separate from the database entities.
- **Middleware** turns domain exceptions into consistent error responses, so controllers never need try/catch.
- **`TimeProvider`** is injected instead of calling `DateTime.UtcNow` directly, which keeps time-dependent logic testable.

## Running locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet run --project src/TicketDesk.Api
```

Then open **`https://localhost:5001/swagger`** (or the `http://localhost:5000` URL printed in the console) to explore the API. A `ticketdesk.db` SQLite file is created automatically on first run.

There is also a `src/TicketDesk.Api/TicketDesk.Api.http` file with ready-made sample requests if you use VS Code or Rider's HTTP client.

## Running the tests

```bash
dotnet test
```

The suite covers the service layer: creation defaults, not-found handling, status-transition rules, and filtered pagination.

## Running with Docker

```bash
docker build -t ticketdesk .
docker run -p 8080:8080 ticketdesk
```

The API will be available on `http://localhost:8080`.

## API reference

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tickets` | List tickets (filter by `status`, `priority`, `assignedTo`; paged via `page`, `pageSize`) |
| `GET` | `/api/tickets/{id}` | Get one ticket |
| `POST` | `/api/tickets` | Create a ticket (starts as `Open`) |
| `PUT` | `/api/tickets/{id}` | Update a ticket's editable fields |
| `PATCH` | `/api/tickets/{id}/status` | Transition status (enforces the state machine) |
| `DELETE` | `/api/tickets/{id}` | Delete a ticket |

### Example: create a ticket

```bash
curl -X POST http://localhost:5000/api/tickets \
  -H "Content-Type: application/json" \
  -d '{ "title": "Login returns 500", "description": "SSO sign-in fails", "priority": "High" }'
```

## Design decisions worth calling out

- **SQLite + `EnsureCreated()`** keeps the project clone-and-run. In a real deployment I'd switch to **EF Core migrations** (`dotnet ef migrations add Initial`) and run them at deploy time instead of creating the schema at startup.
- **Enums stored as strings** in the database, so rows stay human-readable and reordering the enum can't corrupt existing data.
- **Exceptions for control flow are limited** to a couple of domain exception types mapped centrally; everything else is normal return values.

## Possible next steps

These are deliberately left as extensions (good things to discuss in an interview or build next):

- Swap DataAnnotations for **FluentValidation**
- Add **EF Core migrations** and a seed step
- Add **integration tests** with `WebApplicationFactory<Program>`
- Add **authentication/authorisation** (JWT) and per-user ticket ownership
- Add **structured logging** (Serilog) and a correlation id

---

## Notes

Built as a portfolio reference project. The structure and decisions are mine to explain — if you're reviewing this, ask me about any layer and I'll walk you through it.
