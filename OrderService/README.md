# OrderService

## Purpose

Creates and tracks orders. Validates referenced customer/product IDs through HTTP calls to other services.

## Models

- `Order`
  - `Id: int`
  - `CustomerId: int`
  - `Total: decimal`
  - `Status: string`
  - `Items: List<OrderItem>`
- `OrderItem`
  - `OrderItemId: int`
  - `OrderId: int`
  - `ProductId: int`
  - `Quantity: int`

## API

- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PUT /api/orders/{id}`
- `DELETE /api/orders/{id}`

## Validation Rules

- Rejects order create/update with `400 BadRequest` if `CustomerId` does not exist.
- Rejects order create/update with `400 BadRequest` if any `ProductId` in `Items` does not exist.

## Service URL Config (container-safe)

- `ServiceUrls__CustomerService`
- `ServiceUrls__ProductService`

## Swagger

- Host URL: `http://localhost:5003/swagger/index.html`

## Quick Test

```bash
# valid order (assuming CustomerId=1 and ProductId=1 exist)
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":1,"total":999.99,"status":"Created","items":[{"productId":1,"quantity":1}]}'

# invalid order -> expected 400
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":999,"total":25.00,"status":"Created","items":[{"productId":999,"quantity":1}]}'
```
