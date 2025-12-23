# IAMS.Web Production Dockerfile
# Multi-stage build for optimized production image

###################
# Build Stage
###################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["InsuranceAgencyManagementSystem.sln", "./"]
COPY ["src/IAMS.Web/IAMS.Web.csproj", "src/IAMS.Web/"]
COPY ["src/IAMS.Application/IAMS.Application.csproj", "src/IAMS.Application/"]
COPY ["src/IAMS.Domain/IAMS.Domain.csproj", "src/IAMS.Domain/"]
COPY ["src/IAMS.Infrastructure/IAMS.Infrastructure.csproj", "src/IAMS.Infrastructure/"]
COPY ["src/IAMS.Persistence/IAMS.Persistence.csproj", "src/IAMS.Persistence/"]
COPY ["src/IAMS.Identity/IAMS.Identity.csproj", "src/IAMS.Identity/"]
COPY ["src/IAMS.MultiTenancy/IAMS.MultiTenancy.csproj", "src/IAMS.MultiTenancy/"]
COPY ["src/IAMS.Shared/IAMS.Shared.csproj", "src/IAMS.Shared/"]
COPY ["src/IAMS.KnowledgeBase/IAMS.KnowledgeBase.csproj", "src/IAMS.KnowledgeBase/"]

# Restore dependencies (cached layer if csproj files haven't changed)
RUN dotnet restore "src/IAMS.Web/IAMS.Web.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/IAMS.Web"
RUN dotnet build "IAMS.Web.csproj" -c Release -o /app/build --no-restore

###################
# Publish Stage
###################
FROM build AS publish
RUN dotnet publish "IAMS.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build \
    /p:UseAppHost=false

###################
# Runtime Stage
###################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN groupadd -r iams && useradd -r -g iams iamsuser

# Install necessary runtime dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories for logs and data
RUN mkdir -p /app/logs /app/data /app/wwwroot \
    && chown -R iamsuser:iams /app

# Switch to non-root user
USER iamsuser

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entrypoint
ENTRYPOINT ["dotnet", "IAMS.Web.dll"]
