# ProductService

## Purpose

Handles catalog, price, and stock.

## Model

- `Product`
  - `Id: int`
  - `Name: string`
  - `Price: decimal`
  - `Stock: int`

## API

- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

## Swagger

- Host URL: `http://localhost:5002/swagger/index.html`

## Quick Test

```bash
curl -s -X POST http://localhost:5002/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","price":999.99,"stock":10}'
```
