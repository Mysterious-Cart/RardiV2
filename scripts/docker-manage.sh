#!/bin/bash

# Rardi Docker Management Script
# This script helps manage the Docker containers for the Rardi project

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
}

# Function to start services
start_services() {
    local env=${1:-"dev"}
    print_status "Starting Rardi services in $env environment..."
    
    if [ "$env" = "prod" ]; then
        docker-compose up -d
    else
        docker-compose -f docker-compose.dev.yml up -d
    fi
    
    print_success "Services started successfully!"
    
    if [ "$env" = "dev" ]; then
        print_status "Development services:"
        echo "  - PostgreSQL: localhost:5433"
        echo "  - Redis: localhost:6379"
        echo "  - pgAdmin: http://localhost:8080 (admin@rardi.dev / admin123)"
        echo "  - Redis Commander: http://localhost:8081"
        echo "  - Prometheus: http://localhost:9090"
    else
        print_status "Production services:"
        echo "  - Web Application: http://localhost:80"
        echo "  - Security Service: http://localhost:5001"
        echo "  - Customer Service: http://localhost:5002"
        echo "  - Inventory Service: http://localhost:5003"
        echo "  - Payment Service: http://localhost:5004"
        echo "  - Prometheus: http://localhost:9090"
        echo "  - Grafana: http://localhost:3000 (admin / admin)"
    fi
}

# Function to stop services
stop_services() {
    local env=${1:-"dev"}
    print_status "Stopping Rardi services in $env environment..."
    
    if [ "$env" = "prod" ]; then
        docker-compose down
    else
        docker-compose -f docker-compose.dev.yml down
    fi
    
    print_success "Services stopped successfully!"
}

# Function to restart services
restart_services() {
    local env=${1:-"dev"}
    print_status "Restarting Rardi services in $env environment..."
    stop_services $env
    start_services $env
}

# Function to view logs
view_logs() {
    local service=${1:-""}
    local env=${2:-"dev"}
    
    if [ -z "$service" ]; then
        print_status "Viewing logs for all services..."
        if [ "$env" = "prod" ]; then
            docker-compose logs -f
        else
            docker-compose -f docker-compose.dev.yml logs -f
        fi
    else
        print_status "Viewing logs for $service..."
        if [ "$env" = "prod" ]; then
            docker-compose logs -f $service
        else
            docker-compose -f docker-compose.dev.yml logs -f $service
        fi
    fi
}

# Function to build services
build_services() {
    local env=${1:-"prod"}
    print_status "Building Rardi services for $env environment..."
    
    if [ "$env" = "prod" ]; then
        docker-compose build
    else
        # For dev, we only need to build if we have custom images
        print_warning "Development environment uses external images only"
    fi
    
    print_success "Build completed successfully!"
}

# Function to clean up
cleanup() {
    print_status "Cleaning up Docker resources..."
    
    # Stop and remove containers
    docker-compose down --remove-orphans 2>/dev/null || true
    docker-compose -f docker-compose.dev.yml down --remove-orphans 2>/dev/null || true
    
    # Remove unused images
    docker image prune -f
    
    # Remove unused volumes (be careful with this)
    print_warning "This will remove unused Docker volumes. Continue? (y/N)"
    read -r response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        docker volume prune -f
    fi
    
    print_success "Cleanup completed!"
}

# Function to show status
show_status() {
    print_status "Docker container status:"
    docker ps -a --filter "name=rardi"
    
    print_status "Docker images:"
    docker images --filter "reference=*rardi*"
    
    print_status "Docker volumes:"
    docker volume ls --filter "name=rardi"
}

# Function to run database migrations
run_migrations() {
    local env=${1:-"dev"}
    print_status "Running database migrations..."
    
    # Wait for database to be ready
    print_status "Waiting for database to be ready..."
    sleep 10
    
    if [ "$env" = "prod" ]; then
        # Run migrations for each service
        docker-compose exec security-service dotnet ef database update || print_warning "Security service migration failed or not needed"
        docker-compose exec customer-service dotnet ef database update || print_warning "Customer service migration failed or not needed"
        docker-compose exec inventory-service dotnet ef database update || print_warning "Inventory service migration failed or not needed"
        docker-compose exec payment-service dotnet ef database update || print_warning "Payment service migration failed or not needed"
    else
        print_warning "In development mode, run migrations manually from your IDE or use the production environment"
    fi
    
    print_success "Migrations completed!"
}

# Function to show help
show_help() {
    echo "Rardi Docker Management Script"
    echo
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo
    echo "Commands:"
    echo "  start [dev|prod]     Start services (default: dev)"
    echo "  stop [dev|prod]      Stop services (default: dev)"
    echo "  restart [dev|prod]   Restart services (default: dev)"
    echo "  logs [service] [env] View logs for service or all services"
    echo "  build [dev|prod]     Build services (default: prod)"
    echo "  status               Show container status"
    echo "  migrate [dev|prod]   Run database migrations"
    echo "  cleanup              Clean up Docker resources"
    echo "  help                 Show this help message"
    echo
    echo "Examples:"
    echo "  $0 start dev         Start development environment"
    echo "  $0 start prod        Start production environment"
    echo "  $0 logs postgres dev View PostgreSQL logs in dev environment"
    echo "  $0 cleanup           Clean up unused Docker resources"
}

# Main script logic
main() {
    check_docker
    
    case "${1:-help}" in
        start)
            start_services "${2:-dev}"
            ;;
        stop)
            stop_services "${2:-dev}"
            ;;
        restart)
            restart_services "${2:-dev}"
            ;;
        logs)
            view_logs "${2:-}" "${3:-dev}"
            ;;
        build)
            build_services "${2:-prod}"
            ;;
        status)
            show_status
            ;;
        migrate)
            run_migrations "${2:-dev}"
            ;;
        cleanup)
            cleanup
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            show_help
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"