FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

COPY NuGet.config ./
COPY *.csproj ./
RUN dotnet restore --verbosity detailed


COPY . ./
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish --no-restore --verbosity detailed

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "AssetService.dll"]
