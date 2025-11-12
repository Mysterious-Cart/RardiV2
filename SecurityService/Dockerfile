# Security Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["SecurityService/Security/Security.csproj", "SecurityService/Security/"]
RUN dotnet restore "SecurityService/Security/Security.csproj"

# Copy source code
COPY SecurityService/ SecurityService/
WORKDIR "/src/SecurityService/Security"
RUN dotnet build "Security.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Security.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create Keys directory for JWT keys
RUN mkdir -p /app/Keys

ENTRYPOINT ["dotnet", "Security.dll"]

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1