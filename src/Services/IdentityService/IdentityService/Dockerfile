# Step 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Set environment variables for better NuGet performance and network resilience
ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy NuGet.config first for better caching
COPY NuGet.config ./

# Copy csproj files for better layer caching
COPY *.csproj ./

# Restore with retry mechanism
RUN dotnet restore --verbosity minimal

# Copy everything else
COPY . ./

# Build and publish with optimized settings
RUN dotnet publish -c Release -o /app/publish --no-restore --verbosity minimal

# Step 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# Configure environment
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "IdentityService.dll"]
