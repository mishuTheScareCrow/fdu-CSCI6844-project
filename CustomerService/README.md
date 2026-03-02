# CustomerService

## Purpose

Manages customer information.

## Model

- `Customer`
  - `Id: int`
  - `Name: string`
  - `Email: string`

## API

- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `DELETE /api/customers/{id}`

## Swagger

- Host URL: `http://localhost:5001/swagger/index.html`

## Quick Test

```bash
curl -s -X POST http://localhost:5001/api/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@example.com"}'
```
