# Rardi Docker Setup

This document explains how to use Docker with the Rardi project for development and production environments.

## Prerequisites

- [Docker](https://www.docker.com/get-started) installed and running
- [Docker Compose](https://docs.docker.com/compose/) (usually included with Docker Desktop)
- At least 4GB of available RAM
- At least 10GB of available disk space

## Quick Start

### Development Environment

To start the development environment with just the infrastructure services (PostgreSQL, Redis, pgAdmin, etc.):

```bash
# Using the management script (recommended)
./scripts/docker-manage.sh start dev

# Or directly with Docker Compose
docker-compose -f docker-compose.dev.yml up -d
```

### Production Environment

To start the full production environment with all services:

```bash
# Using the management script (recommended)
./scripts/docker-manage.sh start prod

# Or directly with Docker Compose
docker-compose up -d
```

### Access Points

After starting the services, you can access:

- **GraphQL Playground**: http://localhost:4000/graphql (Apollo Router - unified supergraph)
- **Rardi Web App**: http://localhost:5000
- **Prometheus Metrics**: http://localhost:9090
- **Grafana Dashboard**: http://localhost:3000 (admin/admin)
- **pgAdmin**: http://localhost:5050 (admin@example.com/admin)

## Management Scripts

We provide management scripts for both Unix/Linux/macOS and Windows:

- `scripts/docker-manage.sh` - For Unix/Linux/macOS
- `scripts/docker-manage.bat` - For Windows

### Available Commands

```bash
# Start services
./scripts/docker-manage.sh start [dev|prod]

# Stop services
./scripts/docker-manage.sh stop [dev|prod]

# Restart services
./scripts/docker-manage.sh restart [dev|prod]

# View logs
./scripts/docker-manage.sh logs [service_name] [env]

# Build services
./scripts/docker-manage.sh build [dev|prod]

# Show status
./scripts/docker-manage.sh status

# Run database migrations
./scripts/docker-manage.sh migrate [dev|prod]

# Cleanup unused resources
./scripts/docker-manage.sh cleanup

# Show help
./scripts/docker-manage.sh help
```

## Environments

### Development Environment (`docker-compose.dev.yml`)

The development environment includes:

- **PostgreSQL**: Database server (port 5433)
- **Redis**: Cache server (port 6379)
- **pgAdmin**: Database administration tool (http://localhost:8080)
- **Redis Commander**: Redis GUI (http://localhost:8081)
- **Prometheus**: Monitoring (http://localhost:9090)

**Login credentials:**
- pgAdmin: `admin@rardi.dev` / `admin123`

### Production Environment (`docker-compose.yml`)

The production environment includes all development services plus:

- **Security Service**: Authentication & authorization (port 5001)
- **Customer Service**: Customer management (port 5002)
- **Inventory Service**: Product & inventory management (port 5003)
- **Payment Service**: Payment processing (port 5004)
- **Rardi Web**: Main web application (port 5000)
- **Apollo Router**: GraphQL Federation Gateway (port 4000)
- **Grafana**: Advanced monitoring dashboard (http://localhost:3000)

**Login credentials:**
- Grafana: `admin` / `admin`

## Service Architecture

```
┌─────────────────┐    ┌──────────────────┐
│  Apollo Router  │    │   Rardi Web App  │
│   (Port 4000)   │    │   (Port 5000)    │
└─────────────────┘    └──────────────────┘
         │                       │
         │ GraphQL Federation    │ Web Interface
         │                       │
         ├─ /graphql ──── Supergraph composed from:
         │  ├─ Security Service (Port 5001/graphql)
         │  ├─ Customer Service (Port 5002/graphql)
         │  ├─ Inventory Service (Port 5003/graphql)
         │  └─ Payment Service (Port 5004/graphql)

┌──────────────────┐    ┌──────────────────┐
│   PostgreSQL     │    │      Redis       │
│   (Port 5433)    │    │   (Port 6379)    │
└──────────────────┘    └──────────────────┘

┌──────────────────┐    ┌──────────────────┐
│   Prometheus     │    │     Grafana      │
│   (Port 9090)    │    │   (Port 3000)    │
└──────────────────┘    └──────────────────┘
```

## Database Schema

The PostgreSQL database is organized with separate schemas for each service:

- `security` - User authentication, roles, permissions
- `customer` - Customer profiles, vehicles, relationships
- `inventory` - Products, categories, stock management
- `payment` - Transactions, payment methods, financial data

## Development Workflow

### 1. Start Infrastructure

```bash
./scripts/docker-manage.sh start dev
```

### 2. Run Services Locally

Instead of running the .NET services in Docker, run them locally in your IDE for better debugging:

```bash
# Start each service from your IDE or terminal
cd SecurityService/Security && dotnet run
cd CustomerService/Customer && dotnet run
cd InventoryService/Inventory && dotnet run
cd PaymentService/Payment && dotnet run
cd Rardi/Rardi.Web && dotnet run
```

### 3. Update Connection Strings

Make sure your local services connect to the Docker database by updating `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5433;Database=rardi_db;Username=postgres;Password=Killergamerxlyy44;"
  }
}
```

### 4. Run Migrations

```bash
# From each service directory
dotnet ef database update
```

## Apollo Router & GraphQL Federation

### Overview

The project uses Apollo Federation v2 with Apollo Router as the gateway to compose a unified GraphQL supergraph from multiple microservice subgraphs.

### Architecture

- **Apollo Router**: Acts as the GraphQL gateway at port 4000
- **Subgraph Services**: Each microservice exposes its own GraphQL schema
- **Supergraph**: Unified schema composed from all subgraphs
- **Federation**: Enables cross-service entity resolution and type sharing

### GraphQL Endpoints

```bash
# Unified supergraph endpoint (recommended for clients)
http://localhost:4000/graphql

# Individual subgraph endpoints (for development/debugging)
http://localhost:5001/graphql  # Security Service
http://localhost:5002/graphql  # Customer Service
http://localhost:5003/graphql  # Inventory Service
http://localhost:5004/graphql  # Payment Service
```

### Updating the Supergraph Schema

When you modify any service's GraphQL schema, you need to regenerate the supergraph:

```bash
# Install Apollo CLI (if not already installed)
npm install -g @apollo/rover

# Compose the supergraph from all subgraphs
rover supergraph compose --config ./Router/supergraph-config.yaml > ./Router/supergraph.graphql

# Restart Apollo Router to pick up changes
docker-compose restart apollo-router
```

## Production Deployment

### 1. Build and Start All Services

```bash
./scripts/docker-manage.sh build prod
./scripts/docker-manage.sh start prod
```

### 2. Run Database Migrations

```bash
./scripts/docker-manage.sh migrate prod
```

### 3. Verify Services

Check that all services are healthy:

```bash
./scripts/docker-manage.sh status
```

Visit the application at: http://localhost

## Monitoring and Logs

### View Logs

```bash
# All services
./scripts/docker-manage.sh logs

# Specific service
./scripts/docker-manage.sh logs rardi-security-service

# Follow logs in real-time
docker-compose logs -f
```

### Monitoring

- **Prometheus**: http://localhost:9090 - Metrics collection
- **Grafana**: http://localhost:3000 - Dashboards and alerting
- **pgAdmin**: http://localhost:8080 - Database management

## Troubleshooting

### Common Issues

1. **Port conflicts**: Make sure ports 5433, 6379, 8080, 8081, 9090, 3000, 5000-5004 are available
2. **Out of memory**: Docker requires at least 4GB RAM
3. **Build failures**: Check that all project files are in place and dependencies are restored

### Reset Everything

```bash
# Stop all services
./scripts/docker-manage.sh stop dev
./scripts/docker-manage.sh stop prod

# Clean up resources
./scripts/docker-manage.sh cleanup

# Start fresh
./scripts/docker-manage.sh start dev
```

### Database Issues

```bash
# Connect to PostgreSQL container
docker exec -it rardi-postgres-dev psql -U postgres -d rardi_db

# View database logs
./scripts/docker-manage.sh logs postgres
```

### Service Health Checks

```bash
# Check service health
curl http://localhost:4000/health   # Apollo Router Gateway
curl http://localhost:5001/health   # Security Service
curl http://localhost:5002/health   # Customer Service
curl http://localhost:5003/health   # Inventory Service
curl http://localhost:5004/health   # Payment Service

# GraphQL endpoint (unified supergraph)
curl -X POST http://localhost:4000/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __schema { types { name } } }"}'
```

## Configuration Files

- `docker-compose.yml` - Production environment
- `docker-compose.dev.yml` - Development environment
- `Router/router.yaml` - Apollo Router configuration
- `Router/supergraph.graphql` - GraphQL Federation supergraph schema
- `Router/supergraph-config.yaml` - Supergraph composition configuration
- `docker/prometheus/prometheus.yml` - Prometheus monitoring configuration
- `docker/postgres/init/01-init-db.sql` - Database initialization
- `.dockerignore` - Files excluded from Docker build context

## Security Considerations

For production deployment:

1. Change all default passwords
2. Use environment variables for sensitive data
3. Enable HTTPS/TLS certificates
4. Configure proper firewall rules
5. Set up backup strategies for volumes
6. Enable Docker security scanning

## Volume Management

Persistent data is stored in Docker volumes:

- `postgres_data` / `postgres_dev_data` - PostgreSQL database
- `redis_data` / `redis_dev_data` - Redis cache
- `grafana_data` - Grafana dashboards and settings
- `prometheus_data` - Prometheus metrics data

### Backup Volumes

```bash
# Backup PostgreSQL data
docker run --rm -v rardiv2_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres-backup.tar.gz -C /data .

# Restore PostgreSQL data
docker run --rm -v rardiv2_postgres_data:/data -v $(pwd):/backup alpine tar xzf /backup/postgres-backup.tar.gz -C /data
```

## Support

For issues related to Docker setup, check:

1. Docker and Docker Compose versions
2. Available system resources (RAM, disk space)
3. Network connectivity
4. Service logs using the management scripts

For application-specific issues, refer to the individual service documentation.