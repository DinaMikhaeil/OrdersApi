# OrdersApiWithRedis

This is a simple Orders API built with .NET Core, using:

- **Entity Framework Core** for database access
- **Redis** for caching orders
- **Async/Await** and proper **Dependency Injection**
- CRUD operations for orders: Create, Read, List, Delete

## Endpoints

- `POST /orders` - Create an order
- `GET /orders/{id}` - Get a single order (uses Redis cache)
- `GET /orders` - List all orders
- `DELETE /orders/{id}` - Delete an order

## Redis

- Connected on `localhost:6380`
- Cache TTL: 5 minutes

## Instructions

1. Run the API in Visual Studio
2. Ensure Redis server is running on port 6380
3. Use Postman to test endpoints
