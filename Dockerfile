FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend/src/Game.sln Backend/src/
COPY Backend/src/Game.Api/Game.Api.csproj Backend/src/Game.Api/
COPY Backend/src/Game.Application/Game.Application.csproj Backend/src/Game.Application/
COPY Backend/src/Game.Domain/Game.Domain.csproj Backend/src/Game.Domain/
COPY Backend/src/Game.Infrastructure/Game.Infrastructure.csproj Backend/src/Game.Infrastructure/

RUN dotnet restore Backend/src/Game.Api/Game.Api.csproj

COPY Backend/src Backend/src

RUN dotnet publish Backend/src/Game.Api/Game.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 10000

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet Game.Api.dll"]
