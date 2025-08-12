# Step 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Set environment variables for better NuGet performance and network resilience
ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
# ENV DOTNET_NOLOGO=1
# ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
# ENV NUGET_CREDENTIALPROVIDER_TIMEOUT=0
# ENV DOTNET_RESTORE_DISABLE_PARALLEL=1

# Copy NuGet.config first for better caching
COPY NuGet.config ./

# Copy csproj files for better layer caching
COPY *.csproj ./

# Restore with aggressive retry mechanism for sanctions-affected regions
RUN dotnet restore --verbosity detailed --configfile NuGet.config

# Copy everything else
COPY . ./

# Build and publish with explicit restore to ensure all packages are available
RUN dotnet restore --verbosity minimal --configfile NuGet.config && \
    dotnet publish -c Release -o /app/publish --verbosity minimal

# Step 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Configure environment
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "AuditLogService.dll"] 