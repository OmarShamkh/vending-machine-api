# Vending Machine API

A comprehensive REST API for a vending machine system built with .NET 8, featuring role-based authentication, product management, and transaction handling.

## Features

- **Role-based Authentication**: JWT-based authentication with Buyer and Seller roles
- **Product Management**: CRUD operations for products (Sellers only)
- **Coin Management**: Support for 5, 10, 20, 50, and 100 cent coins
- **Transaction Processing**: Secure purchase transactions with change calculation
- **Comprehensive Logging**: Structured logging with Serilog
- **Database Transactions**: ACID compliance for financial operations
- **API Documentation**: Swagger/OpenAPI documentation
- **Unit Testing**: Comprehensive test coverage

## System Architecture

### Database Schema

**Users Table:**

- Id (Primary Key)
- Username (Unique)
- Password (Hashed)
- Deposit (Decimal)
- Role (Enum: Buyer/Seller)
- CreatedAt, UpdatedAt

**Products Table:**

- Id (Primary Key)
- ProductName
- AmountAvailable
- Cost
- SellerId (Foreign Key to Users)
- CreatedAt, UpdatedAt

**Transactions Table:**

- Id (Primary Key)
- BuyerId (Foreign Key to Users)
- ProductId (Foreign Key to Products)
- Amount, TotalSpent, Change
- CreatedAt

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user (no auth required)
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/user/{id}` - Get user details

### Products (Public)

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get specific product

### Products (Seller Only)

- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product (owner only)
- `DELETE /api/products/{id}` - Delete product (owner only)

### Buyer Operations (Buyer Only)

- `POST /api/buyer/deposit` - Deposit coins
- `POST /api/buyer/buy` - Purchase products
- `POST /api/buyer/reset` - Reset deposit to zero
- `GET /api/buyer/coins` - Get valid coin denominations

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:

```bash
git clone  https://github.com/OmarShamkh/vending-machine-api.git
cd VendingMachine
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Build the solution:

```bash
dotnet build
```

4. Run the API:

```bash
cd src/VendingMachine.API
dotnet run
```

5. Run tests:

```bash
cd tests/VendingMachine.Tests
dotnet test
```

### Configuration

Update `appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VendingMachineDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHere12345678901234567890",
    "ExpirationInMinutes": 60
  }
}
```

* Usage Examples

### 1. Register a Seller

```bash
curl -X POST "https://localhost:7001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "seller1",
    "password": "Password123!",
    "role": "Seller"
  }'
```

### 2. Register a Buyer

```bash
curl -X POST "https://localhost:7001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "buyer1",
    "password": "Password123!",
    "role": "Buyer"
  }'
```

### 3. Login

```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "seller1",
    "password": "Password123!"
  }'
```

### 4. Create Product (Seller)

```bash
curl -X POST "https://localhost:7001/api/products" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Coca Cola",
    "amountAvailable": 10,
    "cost": 150
  }'
```

### 5. Deposit Coins (Buyer)

```bash
curl -X POST "https://localhost:7001/api/buyer/deposit" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100
  }'
```

### 6. Buy Product (Buyer)

```bash
curl -X POST "https://localhost:7001/api/buyer/buy" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "amount": 2
  }'
```

## Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Separate permissions for Buyers and Sellers
- **Password Hashing**: Secure password storage using ASP.NET Core Identity
- **Input Validation**: Comprehensive validation for all inputs
- **SQL Injection Prevention**: Entity Framework with parameterized queries
- **Transaction Safety**: Database transactions for financial operations

## Edge Cases Handled

- **Concurrent Purchases**: Database transactions prevent race conditions
- **Insufficient Funds**: Validation before purchase
- **Insufficient Stock**: Stock validation before purchase
- **Invalid Coins**: Only accepts 5, 10, 20, 50, 100 cent coins
- **Product Ownership**: Sellers can only modify their own products
- **Change Calculation**: Optimal coin breakdown for change

## Testing

The project includes comprehensive unit tests:

```bash
# Run all tests
dotnet test

dotnet test tests/VendingMachine.Tests/
```


### Environment Variables

- `ConnectionStrings__DefaultConnection`: Database connection string
- `JwtSettings__SecretKey`: JWT secret key
- `JwtSettings__ExpirationInMinutes`: Token expiration time


## API Documentation

Once the application is running, visit:

- Swagger UI: `https://localhost:5000/swagger`


