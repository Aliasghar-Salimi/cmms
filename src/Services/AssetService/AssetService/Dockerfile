FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore --verbosity detailed

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "AssetService.dll"]
