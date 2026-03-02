# PaymentService

## Purpose

Validates and handles payments linked to orders.

## Model

- `Payment`
  - `Id: int`
  - `OrderId: int`
  - `Amount: decimal`
  - `Status: string`

## API

- `GET /api/payments`
- `GET /api/payments/{id}`
- `POST /api/payments`
- `PUT /api/payments/{id}`
- `DELETE /api/payments/{id}`

## Swagger

- Host URL: `http://localhost:5004/swagger/index.html`

## Quick Test

```bash
curl -s -X POST http://localhost:5004/api/payments \
  -H "Content-Type: application/json" \
  -d '{"orderId":1,"amount":999.99,"status":"Authorized"}'
```
