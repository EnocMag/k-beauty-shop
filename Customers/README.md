# Customers Service

Customers microservice for the **k-beauty-shop** platform, responsible for managing customer accounts and addresses. Built with .NET 10 and MongoDB using Vertical Slice Architecture.

---

## Tech Stack

- **Framework**: [.NET 10](https://dotnet.microsoft.com/) (`Microsoft.NET.Sdk.Web`)
- **Database**: [MongoDB 8](https://www.mongodb.com/) via [MongoDB.EntityFrameworkCore](https://www.nuget.org/packages/MongoDB.EntityFrameworkCore)
- **API Framework**: [FastEndpoints](https://fast-endpoints.com/) (REPR pattern - Request-Endpoint-Response)
- **CQRS & Messaging**: [MediatR](https://github.com/jbogard/MediatR)
- **Validation**: [FluentValidation](https://fluentvalidation.net/) integrated into MediatR pipeline behaviors
- **Logging**: [Serilog](https://serilog.net/) with structured console output
- **Health Checks**: ASP.NET Core Diagnostics with EF Core DbContext health checks

---

## MongoDB Container Setup

### Prerequisites
- [Docker](https://docs.docker.com/get-docker/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 1. Run the MongoDB Docker Container

Run the following command to start MongoDB 8 with credentials matching `appsettings.json`:

```bash
docker run -d \
  --name mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD='sjG29*+HQw17' \
  -v mongodb_customers_data:/data/db \
  mongo:8
```

> [!NOTE]
> Ensure the port mapping is `-p 27017:27017` (host:container). Port `27017` is the default port MongoDB listens on inside the container.

### 2. Connection String Configuration

The connection string is configured in [`src/Customers.Api/appsettings.json`](src/Customers.Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://admin:sjG29%2A%2BHQw17@localhost:27017"
  },
  "DatabaseName": "CustomersDb"
}
```

> [!IMPORTANT]
> Special characters in the password are URL-encoded in the connection string (`*` becomes `%2A`, `+` becomes `%2B`).

---

## Vertical Slice Architecture

This service follows **Vertical Slice Architecture (VSA)**. Rather than organizing code by horizontal technical layers (Controllers, Services, Repositories), code is grouped by distinct **business features / capabilities** (vertical slices).

### Why Vertical Slice?
- **High Cohesion**: Everything required to deliver a feature (Endpoint, Request, Command/Query, Handler, Validator) lives in that feature's directory.
- **Low Coupling**: Slices do not depend on one another. Changes to one feature don't impact other features.
- **Maintainability**: New features can be added without modifying existing slices or bloated generic service classes.

### Project Structure

```text
src/Customers.Api/
├── Domain/                          # Enterprise domain models & value objects
│   └── Entities/
│       ├── Customer.cs              # Customer entity (with Address & multiple ShippingAddresses)
│       └── Address.cs               # Address owned entity / value object
│
├── Infrastructure/                  # Data access and external concerns
│   └── DbContexts/
│       ├── CustomersDbContext.cs    # EF Core MongoDB DbContext
│       └── Configuration/
│           └── CustomerConfiguration.cs # Document & owned-type mapping (OwnsOne, OwnsMany)
│
├── Features/                        # Vertical Slices
│   └── Customers/
│       ├── Common/                  # Shared response contracts across customer slices
│       │   └── CustomerResponse.cs
│       │
│       ├── Create/                  # Slice: Create a customer
│       │   ├── CreateCustomerEndpoint.cs   # HTTP Route & FastEndpoints handler
│       │   ├── CreateCustomerRequest.cs    # Request DTO
│       │   ├── CreateCustomerCommand.cs    # MediatR Command & CommandHandler (co-located)
│       │   └── CreateCustomerValidator.cs  # FluentValidation rules
│       │
│       └── GetById/                 # Slice: Retrieve customer by ID
│           ├── GetCustomerByIdEndpoint.cs  # HTTP Route & FastEndpoints handler
│           ├── GetCustomerByIdRequest.cs   # Request DTO (Route parameter: Id)
│           ├── GetCustomerByIdQuery.cs     # MediatR Query & QueryHandler (co-located)
│           └── GetCustomerByIdValidator.cs # FluentValidation rules
│
├── Behaviors/                       # MediatR pipeline behaviors
│   └── ValidationBehavior.cs        # Automatic request validation before handlers execute
│
├── Utils/                           # Shared cross-cutting utilities
│   ├── Result.cs                    # Operation outcome wrapper
│   ├── ProblemsException.cs         # ProblemDetails exception handler
│   └── MsgConstants.cs              # Consistent API messaging
│
└── Program.cs                       # Web application bootstrap and DI registration
```

### Key Architectural Patterns

1. **Co-located Command/Query and Handler**:
   In accordance with the vertical slice strategy, commands and their handlers (as well as queries and query handlers) are maintained within the **same physical class file** (e.g., [`CreateCustomerCommand.cs`](src/Customers.Api/Features/Customers/Create/CreateCustomerCommand.cs) contains both `CreateCustomerCommand` and `CreateCustomerCommandHandler`). This minimizes file switching and keeps request definitions directly adjacent to their execution logic.

2. **REPR Pattern with FastEndpoints**:
   Endpoints inherit from `Endpoint<TRequest, TResponse>`, defining route, HTTP method, and response status explicitly without heavy MVC controllers.

3. **Validation Pipeline**:
   Requests dispatched via MediatR pass through [`ValidationBehavior`](src/Customers.Api/Behaviors/ValidationBehavior.cs) where FluentValidation rules execute before reaching the handler.

4. **Document Model with Owned Types**:
   MongoDB EF Core maps `Customer` as a collection (`customers`), where `Address` is mapped as an owned embedded subdocument (`OwnsOne`) and `ShippingAddresses` is mapped as an embedded subdocument list (`OwnsMany`).

---

## Running the Application

### 1. Build the API

```bash
dotnet build
```

### 2. Run the API

```bash
dotnet run --project src/Customers.Api
```

The service will start on:
- API / Swagger: `http://localhost:5050/swagger`
- Health Check: `http://localhost:5050/health`

### 3. Testing the Endpoints

Use the included [`src/Customers.Api/Customers.http`](src/Customers.Api/Customers.http) file or curl:

#### Health Check
```bash
curl -i http://localhost:5050/health
```

#### Create Customer
```bash
curl -i -X POST http://localhost:5050/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "phoneNumber": "+1-555-123-4567",
    "address": {
      "street": "123 Beauty Lane",
      "city": "Seoul",
      "state": "Gangnam",
      "postalCode": "06000",
      "country": "South Korea"
    },
    "shippingAddresses": [
      {
        "street": "456 Shopping St",
        "city": "Busan",
        "state": "Haeundae",
        "postalCode": "48000",
        "country": "South Korea"
      }
    ]
  }'
```

#### Get Customer By ID
```bash
curl -i http://localhost:5050/api/customers/<CUSTOMER_GUID>
```
