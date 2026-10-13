# DaricTrading

A .NET 8 Clean Architecture starter for a gold micro-trading API.

## Projects

- `DaricTrading.Domain`: gold weights, IRR money, tags, wallet/order invariants and domain events.
- `DaricTrading.Application`: use case boundary (kept intentionally thin for this MVP).
- `DaricTrading.Infrastructure`: concurrent in-memory repositories.
- `DaricTrading.API`: buy/sell, order search/details and wallet balance endpoints.
- `tests`: domain and API test project placeholders.

## Run

```powershell
dotnet restore
dotnet run --project src/DaricTrading.API
```

Swagger is available at `/swagger`.

## API sample

`POST /api/orders/buy`

```json
{
  "userId": "10000000-0000-0000-0000-000000000001",
  "weightGrams": 1.25,
  "unitPrice": 3500000,
  "fee": 10000,
  "tags": ["پس‌انداز_بلندمدت"]
}
```

Use `POST /api/orders/sell` with the same payload shape. `GET /api/wallets/{userId}` returns the gold balance. `GET /api/orders?userId=...&tag=...&from=...&to=...&type=Buy` filters settled orders.

## Design notes

`GoldWeight` stores grams with at most three decimal places; order quantity cannot be zero. Wallet zero is represented internally as a zero weight. `Money` accepts whole IRR amounts to avoid introducing sub-rial amounts, and order value is rounded upward to a whole rial before adding the fee. Revisit this rounding rule with product/accounting before production.

The repositories use `ConcurrentDictionary` for safe lookup and insertion. Each wallet serializes credit/debit operations with a private lock, so concurrent sells cannot oversell. This MVP is process-local and loses state on restart; it is not durable or suitable for multi-instance trading. Buy/sell workflows are synchronous within a single API process; production-grade atomic persistence would require a transactional store and idempotency keys.

Orders are immutable in their financial fields after construction. Tags and note can be updated; pending orders can be cancelled. Buy and sell domain event types are defined as extension points; publishing and event persistence are not wired in this MVP.

## Git hygiene

This workspace was provided without a Git repository. Initialize Git and make small Conventional Commits at the project milestone boundaries before submitting the assessment.
