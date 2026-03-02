# fdu-CSCI6844-project

Containerized eCommerce microservices backend using ASP.NET Core + EF Core + SQLite.

## Services

- CustomerService (`localhost:5001`)
- ProductService (`localhost:5002`)
- OrderService (`localhost:5003`)
- PaymentService (`localhost:5004`)

All services run in Docker and expose Swagger UI.

## Project Documentation

- Root overview: this file
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

## Swagger URLs

- Customer: http://localhost:5001/swagger/index.html
- Product: http://localhost:5002/swagger/index.html
- Order: http://localhost:5003/swagger/index.html
- Payment: http://localhost:5004/swagger/index.html

## Quick Verification Checklist

1. Create Customer
2. Create Product
3. Create Order referencing both
4. Create invalid Order with bad IDs and confirm `400 BadRequest`

### Example Commands

```bash
# 1) create customer
curl -s -X POST http://localhost:5001/api/customers \
	-H "Content-Type: application/json" \
	-d '{"name":"Alice","email":"alice@example.com"}'

# 2) create product
curl -s -X POST http://localhost:5002/api/products \
	-H "Content-Type: application/json" \
	-d '{"name":"Laptop","price":999.99,"stock":10}'

# 3) create valid order (expect HTTP:201)
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5003/api/orders \
	-H "Content-Type: application/json" \
	-d '{"customerId":1,"total":999.99,"status":"Created","items":[{"productId":1,"quantity":1}]}'

# 4) invalid order (expect HTTP:400)
curl -s -w "\nHTTP:%{http_code}\n" -X POST http://localhost:5003/api/orders \
	-H "Content-Type: application/json" \
	-d '{"customerId":999,"total":25.00,"status":"Created","items":[{"productId":999,"quantity":1}]}'
```
