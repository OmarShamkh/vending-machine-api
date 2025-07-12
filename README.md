# Vending Machine API

A robust REST API for a vending machine system built with .NET 8, featuring role-based authentication, product management, transaction handling, and full support for optimistic concurrency (RowVersion).

---

## Features

- **Role-based Authentication:** JWT-based authentication with Buyer and Seller roles
- **Product Management:** CRUD operations for products (Sellers only)
- **Coin Management:** Support for 5, 10, 20, 50, and 100 cent coins
- **Transaction Processing:** Secure purchase transactions with change calculation
- **Optimistic Concurrency:** RowVersion-based conflict detection for safe concurrent updates
- **Comprehensive Logging:** Structured logging with Serilog
- **Database Transactions:** ACID compliance for financial operations
- **API Documentation:** Swagger/OpenAPI documentation
- **Unit & Integration Testing:** Comprehensive test coverage

---

## System Architecture

### Database Schema

**Users Table:**
- Id (Primary Key)
- Username (Unique)
- Password (Hashed)
- Deposit (Decimal)
- Role (Enum: Buyer/Seller)
- CreatedAt, UpdatedAt
- RowVersion (for concurrency)

**Products Table:**
- Id (Primary Key)
- ProductName
- AmountAvailable
- Cost
- SellerId (Foreign Key to Users)
- CreatedAt, UpdatedAt
- RowVersion (for concurrency)

**Transactions Table:**
- Id (Primary Key)
- BuyerId (Foreign Key to Users)
- ProductId (Foreign Key to Products)
- Amount, TotalSpent, Change
- CreatedAt

---

## Project Structure

```
Vending-machine/
├── src/
│   ├── VendingMachine.API/           # ASP.NET Core Web API
│   ├── VendingMachine.Core/          # Entities, DTOs, Enums, Interfaces
│   ├── VendingMachine.Infrastructure/# EF Core DbContext, Services, Migrations
├── tests/
│   └── VendingMachine.Tests/         # Unit and integration tests
```

---

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user (no auth required)
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/user/{id}` - Get user details (returns RowVersion)

### Products (Public)

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get specific product

### Products (Seller Only)

- `POST /api/products` - Create new product (returns RowVersion)
- `PUT /api/products/{id}` - Update product (owner only, requires RowVersion)
- `DELETE /api/products/{id}` - Delete product (owner only, returns RowVersion)

### Buyer Operations (Buyer Only)

- `POST /api/buyer/deposit` - Deposit coins (requires RowVersion, returns new RowVersion)
- `POST /api/buyer/buy` - Purchase products (returns new RowVersion)
- `POST /api/buyer/reset` - Reset deposit to zero (requires RowVersion, returns new RowVersion)
- `GET /api/buyer/coins` - Get valid coin denominations

---

## Optimistic Concurrency (RowVersion)

**How it works:**
- Every update/delete request must include the latest `rowVersion` value (from the last GET/response).
- If the row was changed by someone else, the API returns a `409 Conflict`.
- The client should re-fetch the resource and retry if needed.

**Example:**
1. **Fetch product:**
   ```json
   {
     "id": 1,
     "productName": "Coca Cola",
     "rowVersion": "AAAAAAAAB9E="
   }
   ```
2. **Update product:**
   ```json
   {
     "productName": "Diet Coke",
     "rowVersion": "AAAAAAAAB9E="
   }
   ```
3. **On success:** New `rowVersion` is returned.
4. **On conflict:** API returns 409 and error message.

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code

### Installation

```bash
git clone https://github.com/OmarShamkh/vending-machine-api.git
cd Vending-machine
dotnet restore
dotnet build
```

### Database Setup

- Update `appsettings.json` with your connection string.
- Run migrations:
  ```bash
  dotnet ef database update --project src/VendingMachine.Infrastructure --startup-project src/VendingMachine.API
  ```

### Run the API

```bash
cd src/VendingMachine.API
dotnet run
```

### Run Tests

```bash
dotnet test
```

---

## Usage Examples

### Register, Login, and Use RowVersion

See the [API Endpoints](#api-endpoints) section for endpoint details.

**Deposit with RowVersion:**
```bash
curl -X POST "https://localhost:7001/api/buyer/deposit" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100,
    "rowVersion": "AAAAAAAAB9E="
  }'
```

**Handle 409 Conflict:**
- If you get a 409, re-fetch the user/product, get the new `rowVersion`, and retry.

---

## Testing

- **Unit Tests:**  
  Located in `tests/VendingMachine.Tests/Services/`.  
  Run with `dotnet test`.

- **Integration Tests:**  
  (Add your own or extend the provided suite.)

- **Test Coverage:**  
  Use Coverlet or Visual Studio to generate a coverage report.

---

## Security Features

- **JWT Authentication:** Secure token-based authentication
- **Role-based Authorization:** Separate permissions for Buyers and Sellers
- **Password Hashing:** Secure password storage
- **Input Validation:** Comprehensive validation for all inputs
- **SQL Injection Prevention:** Entity Framework with parameterized queries
- **Transaction Safety:** Database transactions for financial operations
- **Optimistic Concurrency:** Prevents lost updates and race conditions

---

## Troubleshooting

- **409 Conflict:**  
  Indicates a concurrency conflict. Re-fetch the resource and retry with the new `rowVersion`.
- **Database errors:**  
  Ensure your connection string is correct and migrations are up to date.
- **JWT errors:**  
  Make sure your token is valid and not expired.

---

## Contributing

1. Fork the repo and create your feature branch.
2. Commit your changes and add tests.
3. Push to the branch and open a pull request.

---

## API Documentation

Once the application is running, visit:

- Swagger UI: `https://localhost:5000/swagger`

---

If you need more examples or want to see a sample client implementation, just ask!


