# RardiV2 - AI Coding Agent Instructions

## Architecture Overview
RardiV2 is a **microservices-based POS platform for Mechanics mainly in Cambodia** using Apollo GraphQL Federation with:
- **4 Backend Services**: Security, Customer, Inventory, Payment (ASP.NET Core 9.0 + PostgreSQL + HotChocolate GraphQL)
- **3 Frontend Apps**: Blazor Web (SSR/WASM), Blazor MAUI (mobile), Shared Component Library
- **Infrastructure**: Docker Compose, Apollo Router (GraphQL Gateway), Redis cache, Prometheus/Grafana monitoring

Each microservice follows **schema-per-service** database isolation with **Apollo Federation supergraph** pattern for unified schema composition and **JWT-based authentication** from SecurityService.

## Key Development Patterns

### Service Structure (Apply to all *Service/ directories)
```
{ServiceName}Service/
├── Dockerfile                    # Multi-stage build with health checks
├── {ServiceName}/
    ├── Program.cs               # DI registration + GraphQL server setup
    ├── Data/
    │   ├── {ServiceName}Context.cs  # EF DbContext with factory pattern
    │   └── Model/               # EF entities (NOT DTOs)
    ├── Graphql/
    │   ├── Query.cs            # [Authorize] + service injection
    │   └── Mutation.cs         # Transaction-wrapped operations
    ├── Services/
    │   └── {ServiceName}Service.cs  # Business logic with cancellation tokens
    └── Assets/Domain/          # GraphQL DTOs (separate from EF models)
```

### Critical DbContext Pattern
**Always use `IDbContextFactory<T>` pattern** - services registered as factories, not scoped:
```csharp
// ❌ Wrong - causes scoping issues
builder.Services.AddDbContext<SecurityContext>(...)

// ✅ Correct - use factory pattern
builder.Services.AddDbContextFactory<SecurityContext>(...)

// In services, inject factory and create contexts:
public class SecurityService(IDbContextFactory<SecurityContext> contextFactory)
{
    public async Task<User> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        using var context = contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync(ct);
        // ... operations
    }
}
```

### GraphQL Federation Setup
Each service registers GraphQL with HotChocolate Federation:
```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddAuthorization()  // Required for [Authorize] attributes
    .AddApolloFederation()  // Enable Apollo Federation v2
    .AddProjections()
    .AddFiltering()
    .AddSorting();
```

**Apollo Federation Supergraph Pattern:**
- Each service defines federated entities with `@key` directive
- Apollo Router composes supergraph from subgraph schemas
- Frontend uses **StrawberryShake** client against unified supergraph endpoint

### Authentication Flow
1. **SecurityService** handles login/registration → issues JWT tokens
2. **Other services** validate JWT using shared RSA public key
3. **Frontend** stores tokens in localStorage, passes in Authorization headers

## Development Workflows

### Docker Development
```bash
# Start infrastructure only (recommended for development)
./scripts/docker-manage.sh start dev
# OR on Windows:
.\scripts\docker-manage.bat start dev

# Run services locally with: dotnet run --project {Service}/{Service}.csproj
# They'll connect to Dockerized PostgreSQL on port 5433
```

### Database Migrations
```bash
# Add migration (from service directory):
dotnet ef migrations add MigrationName --context {Service}Context

# Apply to Docker database:
./scripts/docker-manage.sh migrate dev
```

### Frontend Development
- **Rardi.Shared**: Component library + GraphQL client
- **Rardi.Web**: Blazor Server/WASM hybrid
- **Rardi**: MAUI app (mobile/desktop)

Run with: `dotnet run --project Rardi/Rardi.Web/`

## Project-Specific Conventions

### Service Dependencies
- **Never cross-service database calls** - use GraphQL federation or HTTP
- **JWT tokens carry user claims** - extract user ID from `ClaimsPrincipal`
- **Cancellation tokens required** - all async methods must accept `CancellationToken`

### Data Handling Patterns
- **EF Models ≠ GraphQL DTOs** - use Mapster for conversion in `Assets/Domain/`
- **Khmer text normalization** in InventoryService for multi-language product names
- **CSV import functionality** in PaymentService with robust date parsing

### Error Handling
```csharp
// GraphQL mutations should return domain objects, not generic responses
public async Task<Product> CreateProduct(CreateProductInput input, CancellationToken ct)
{
    // HotChocolate handles exceptions → GraphQL errors automatically
    using var context = _contextFactory.CreateDbContext();
    // ... business logic
}
```

## Key Integration Points

### Service Communication
- **Apollo Router Gateway**: Composes supergraph from federated subgraph services
- **Federation Entities**: Cross-service data resolution via `@key` directives and reference resolvers
- **Health Checks**: All services expose `/health` endpoint for Docker
- **Monitoring**: Prometheus metrics + Grafana dashboards

### Security Integration
- **JWT Public Key sharing**: RSA key from SecurityService config
- **Role-based authorization**: `[Authorize(Roles = "Admin")]` in GraphQL resolvers
- **CORS configuration**: Allows Blazor WebAssembly origin

### Frontend-Backend Contract
- **Schema-first GraphQL**: Update `.graphql` files in `Rardi.Shared/` when backend schemas change
- **StrawberryShake codegen**: Run `dotnet build` to regenerate client code
- **MudBlazor components**: Use for consistent UI across Blazor apps

## Common Debugging

### Docker Issues
```bash
# Check service health
docker compose ps
# View service logs
./scripts/docker-manage.sh logs security-service
# Rebuild after code changes
./scripts/docker-manage.sh build
```

### Database Connection Issues
- **Check connection strings** point to `localhost:5433` (Docker PostgreSQL)
- **Verify migrations applied**: Check `__EFMigrationsHistory` table
- **DbContext factory errors**: Ensure using `IDbContextFactory<T>`, not `DbContext` directly

### GraphQL Schema Issues  
- **Schema mismatch**: Regenerate client with `dotnet build Rardi.Shared/`
- **Authorization failures**: Verify JWT token in request headers
- **CORS errors**: Check allowed origins in service startup