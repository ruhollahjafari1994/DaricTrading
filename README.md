# Daric Trading API

A .NET 8 reference implementation for a gold micro-trading platform. It demonstrates a small, auditable vertical slice: buy and sell orders, gold wallet balances, settled-order metadata, cancellation of pending orders, filtering, and RFC 7807 error responses.

## MVP scope

The default runtime uses thread-safe in-memory repositories so the API starts without a database. PostgreSQL is also implemented as an optional persistence adapter and is selected by Docker Compose or by setting `Persistence:Provider=PostgreSql`. The API applies EF Core migrations on startup in PostgreSQL mode. Keeping in-memory as the local default keeps the core use cases easy to run; the database adapter and Testcontainers suite exercise durable persistence without making a database mandatory for domain development.

## Architecture

- **Domain** contains `GoldWeight`, `Money`, `Tag`, wallet/order aggregates, domain exceptions/events, and repository ports. It has no ASP.NET Core or persistence dependency.
- **Application** contains separate buy, sell, get, search, update-tags, cancel, and wallet-balance use-case handlers plus FluentValidation validators and API-independent DTOs.
- **Infrastructure** contains concurrent in-memory repositories, PostgreSQL/EF Core repositories and migrations, the clock, and domain-event dispatch adapters.
- **API** contains versioned HTTP contracts/controllers, Swagger examples, health checks, and the global RFC 7807 exception handler. API contracts do not expose Domain types.
- **Tests** contains domain invariant tests, in-memory API integration tests, and PostgreSQL Testcontainers end-to-end tests.

## HTTP endpoints

| Method | Path | Purpose |
|---|---|---|
| `POST` | `/api/v1/orders/buy` | Create and settle a buy |
| `POST` | `/api/v1/orders/sell` | Create and settle a sell |
| `GET` | `/api/v1/orders/{id}` | Read an order |
| `GET` | `/api/v1/orders` | Filter settled orders by `userId`, `tag`, `from`, `to`, and `orderType` |
| `PATCH` | `/api/v1/orders/{id}/tags` | Update tags and user note on a settled order; returns 204 |
| `POST` | `/api/v1/orders/{id}/cancel` | Cancel a pending order; returns 204 |
| `GET` | `/api/v1/wallets/{userId}/balance` | Read a gold balance |
| `GET` | `/health` | Container health probe |

Invalid request fields return `400 ValidationProblemDetails`, missing orders return `404 ProblemDetails`, business-rule violations return `422 ProblemDetails`, optimistic concurrency collisions return `409`, and unexpected failures return a generic `500` response. Swagger is available at `/swagger` and includes sample order requests.

### Request example

```json
{
  "userId": "10000000-0000-0000-0000-000000000001",
  "weightGrams": 1.250,
  "unitPrice": 3500000,
  "fee": 5000,
  "tags": ["پس‌انداز_بلندمدت"]
}
```

## ADR 01: Financial precision

All financial calculations use `decimal`; binary floating-point is not used. Gold weights are grams and are accepted only when positive and exact to at most three decimal places (one milligram / سوت). `Money` is non-negative, requires an uppercase three-letter currency code, and allows at most two decimal places. The gross value is calculated as `weight × price-per-gram`, rounded to the currency precision using midpoint-to-even; buy fees are added and sell fees are subtracted. A sell whose fee exceeds gross proceeds is rejected. Settled order financial fields have private setters and no mutation methods; only tags and the user note have an update operation.

## ADR 02: Concurrency and persistence

The in-memory adapter uses `ConcurrentDictionary` for repository access and a lock inside each wallet aggregate for balance mutations. Concurrent sell operations serialize at the wallet; insufficient funds cannot drive a balance below zero. This storage is process-local and is lost on restart, so it is suitable for the single-process MVP only.

The PostgreSQL adapter stores order and wallet snapshots through EF Core. Wallet/order version columns are optimistic concurrency tokens. The wallet change and order insert share a scoped `DbContext` and are committed by one `SaveChanges` transaction, preventing a failed sell from persisting a partial order or negative balance. PostgreSQL migrations are applied at API startup. Set `Persistence:Provider=PostgreSql` and `ConnectionStrings:Trading` to use it outside Docker Compose.

Domain events are collected by aggregates and dispatched after a successful repository write. The current adapters log events for demonstration; a durable outbox is a production requirement if event delivery must survive process failure.

## Run with Docker

Docker Engine must be running. From the repository root:

```powershell
docker compose up --build
```

The API is exposed on `http://localhost:5000`, Swagger on `/swagger`, and the health endpoint on `/health`. Compose starts PostgreSQL and waits for its health check before starting the API. A named volume preserves local PostgreSQL data.

## Run tests

```powershell
dotnet test
```

Domain and in-memory API tests run without Docker. The PostgreSQL integration fixture uses Testcontainers to start a disposable PostgreSQL container, applies the API's EF migrations, exercises persistence, search, metadata updates, cancellation, insufficient balance, and concurrent sales, then disposes the container. Docker Engine must be available for those tests.

## Production roadmap

1. Add command idempotency keys and an explicit application unit-of-work contract for providers that need retries or multiple persistence writes.
2. Replace demonstration event logging with a transactional outbox and a reliable event bus such as RabbitMQ.
3. Deploy PostgreSQL with managed backups and database-level monitoring; evaluate TimescaleDB only if time-series market data is added.
4. Use distributed coordination only where multi-instance workflows require it; database transactions and optimistic concurrency remain the source of truth. Redis locks can supplement, but must not replace, database invariants.
5. Add authentication/authorization, pricing source controls, audit retention, rate limits, and operational dashboards before real funds are supported.

