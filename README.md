# fdu-CSCI6844-project

Containerized eCommerce microservices backend using ASP.NET Core, EF Core, SQLite, RabbitMQ, and an API Gateway (YARP).

## Architecture

- API Gateway is the single public entry point: `http://localhost:5050`
- Internal services (not exposed to host):
  - `customerservice`
  - `productservice`
  - `orderservice`
  - `paymentservice`
- Messaging broker (internal):
  - `rabbitmq` on Docker network port `5672`

RabbitMQ Management UI is available at `http://localhost:15672`.
Default login: `guest` / `guest`.

Client traffic should go through the gateway only.

## Project Documentation

- Root overview: this file
- Final report: [docs/Final-Project-Report.md](docs/Final-Project-Report.md)
- ApiGateway: [ApiGateway](ApiGateway)
- CustomerService: [CustomerService/README.md](CustomerService/README.md)
- ProductService: [ProductService/README.md](ProductService/README.md)
- OrderService: [OrderService/README.md](OrderService/README.md)
- PaymentService: [PaymentService/README.md](PaymentService/README.md)

## Run with Docker

From project root:

```bash
docker compose up --build -d
docker compose ps
```

Stop services:

```bash
docker compose down
```

## Gateway Swagger

- Gateway Swagger UI: http://localhost:5050/swagger/index.html

## Gateway Routes

- Customers: `http://localhost:5050/api/customers/...`
- Products: `http://localhost:5050/api/products/...`
- Orders: `http://localhost:5050/api/orders/...`
- Payments: `http://localhost:5050/api/payments/...`
- Aggregated endpoint: `GET http://localhost:5050/api/orders/{id}/details`

## Quick Verification Checklist

1. Create Customer
2. Create Product
3. Create Order referencing both (through gateway)
4. Create invalid Order with bad IDs and confirm `400 BadRequest`
5. Verify async messaging: product stock is reduced automatically after order creation

### Example Commands

```bash
# 1) create customer
curl -s -X POST http://localhost:5050/api/customers \
	-H "Content-Type: application/json" \
	-d '{"name":"Alice","email":"alice@example.com"}'

# 2) create product
curl -s -X POST http://localhost:5050/api/products \
	-H "Content-Type: application/json" \
	-d '{"name":"Laptop","price":999.99,"stock":10}'

# 3) create valid order (expect HTTP:201)
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5050/api/orders \
	-H "Content-Type: application/json" \
	-d '{"customerId":1,"total":999.99,"status":"Created","items":[{"productId":1,"quantity":1}]}'

# 4) invalid order (expect HTTP:400)
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5050/api/orders \
	-H "Content-Type: application/json" \
	-d '{"customerId":999,"total":25.00,"status":"Created","items":[{"productId":999,"quantity":1}]}'

# 5) verify stock auto-reduced by ProductService consumer
curl -s http://localhost:5050/api/products/1

# 6) verify aggregated endpoint
curl -s http://localhost:5050/api/orders/1/details
```
