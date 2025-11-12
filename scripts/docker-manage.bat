@echo off
REM Rardi Docker Management Script for Windows
REM This script helps manage the Docker containers for the Rardi project

setlocal enabledelayedexpansion

REM Function to check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Parse command line arguments
set COMMAND=%1
set ENV=%2

if "%COMMAND%"=="" set COMMAND=help
if "%ENV%"=="" set ENV=dev

REM Main command processing
if "%COMMAND%"=="start" (
    call :start_services %ENV%
) else if "%COMMAND%"=="stop" (
    call :stop_services %ENV%
) else if "%COMMAND%"=="restart" (
    call :restart_services %ENV%
) else if "%COMMAND%"=="logs" (
    call :view_logs %2 %ENV%
) else if "%COMMAND%"=="build" (
    call :build_services %ENV%
) else if "%COMMAND%"=="status" (
    call :show_status
) else if "%COMMAND%"=="migrate" (
    call :run_migrations %ENV%
) else if "%COMMAND%"=="cleanup" (
    call :cleanup
) else if "%COMMAND%"=="help" (
    call :show_help
) else (
    echo [ERROR] Unknown command: %COMMAND%
    call :show_help
    exit /b 1
)

goto :eof

:start_services
echo [INFO] Starting Rardi services in %1 environment...
if "%1"=="prod" (
    docker-compose up -d
) else (
    docker-compose -f docker-compose.dev.yml up -d
)
echo [SUCCESS] Services started successfully!

if "%1"=="dev" (
    echo [INFO] Development services:
    echo   - PostgreSQL: localhost:5433
    echo   - Redis: localhost:6379
    echo   - pgAdmin: http://localhost:8080 ^(admin@rardi.dev / admin123^)
    echo   - Redis Commander: http://localhost:8081
    echo   - Prometheus: http://localhost:9090
) else (
    echo [INFO] Production services:
    echo   - Web Application: http://localhost:80
    echo   - Security Service: http://localhost:5001
    echo   - Customer Service: http://localhost:5002
    echo   - Inventory Service: http://localhost:5003
    echo   - Payment Service: http://localhost:5004
    echo   - Prometheus: http://localhost:9090
    echo   - Grafana: http://localhost:3000 ^(admin / admin^)
)
goto :eof

:stop_services
echo [INFO] Stopping Rardi services in %1 environment...
if "%1"=="prod" (
    docker-compose down
) else (
    docker-compose -f docker-compose.dev.yml down
)
echo [SUCCESS] Services stopped successfully!
goto :eof

:restart_services
echo [INFO] Restarting Rardi services in %1 environment...
call :stop_services %1
call :start_services %1
goto :eof

:view_logs
if "%1"=="" (
    echo [INFO] Viewing logs for all services...
    if "%2"=="prod" (
        docker-compose logs -f
    ) else (
        docker-compose -f docker-compose.dev.yml logs -f
    )
) else (
    echo [INFO] Viewing logs for %1...
    if "%2"=="prod" (
        docker-compose logs -f %1
    ) else (
        docker-compose -f docker-compose.dev.yml logs -f %1
    )
)
goto :eof

:build_services
echo [INFO] Building Rardi services for %1 environment...
if "%1"=="prod" (
    docker-compose build
) else (
    echo [WARNING] Development environment uses external images only
)
echo [SUCCESS] Build completed successfully!
goto :eof

:show_status
echo [INFO] Docker container status:
docker ps -a --filter "name=rardi"
echo.
echo [INFO] Docker images:
docker images --filter "reference=*rardi*"
echo.
echo [INFO] Docker volumes:
docker volume ls --filter "name=rardi"
goto :eof

:run_migrations
echo [INFO] Running database migrations...
echo [INFO] Waiting for database to be ready...
timeout /t 10 /nobreak >nul

if "%1"=="prod" (
    echo Running migrations for each service...
    docker-compose exec security-service dotnet ef database update 2>nul || echo [WARNING] Security service migration failed or not needed
    docker-compose exec customer-service dotnet ef database update 2>nul || echo [WARNING] Customer service migration failed or not needed
    docker-compose exec inventory-service dotnet ef database update 2>nul || echo [WARNING] Inventory service migration failed or not needed
    docker-compose exec payment-service dotnet ef database update 2>nul || echo [WARNING] Payment service migration failed or not needed
) else (
    echo [WARNING] In development mode, run migrations manually from your IDE or use the production environment
)
echo [SUCCESS] Migrations completed!
goto :eof

:cleanup
echo [INFO] Cleaning up Docker resources...
docker-compose down --remove-orphans 2>nul
docker-compose -f docker-compose.dev.yml down --remove-orphans 2>nul
docker image prune -f
echo [WARNING] This will remove unused Docker volumes. Continue? ^(y/N^)
set /p response=
if /i "%response%"=="y" (
    docker volume prune -f
)
echo [SUCCESS] Cleanup completed!
goto :eof

:show_help
echo Rardi Docker Management Script
echo.
echo Usage: %~nx0 [COMMAND] [OPTIONS]
echo.
echo Commands:
echo   start [dev^|prod]     Start services ^(default: dev^)
echo   stop [dev^|prod]      Stop services ^(default: dev^)
echo   restart [dev^|prod]   Restart services ^(default: dev^)
echo   logs [service] [env] View logs for service or all services
echo   build [dev^|prod]     Build services ^(default: prod^)
echo   status               Show container status
echo   migrate [dev^|prod]   Run database migrations
echo   cleanup              Clean up Docker resources
echo   help                 Show this help message
echo.
echo Examples:
echo   %~nx0 start dev         Start development environment
echo   %~nx0 start prod        Start production environment
echo   %~nx0 logs postgres dev View PostgreSQL logs in dev environment
echo   %~nx0 cleanup           Clean up unused Docker resources
goto :eof