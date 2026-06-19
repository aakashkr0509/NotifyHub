# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files first for layer caching
COPY NotifyHub.Domain/NotifyHub.Domain.csproj           NotifyHub.Domain/
COPY NotifyHub.Application/NotifyHub.Application.csproj NotifyHub.Application/
COPY NotifyHub.Infrastructure/NotifyHub.Infrastructure.csproj NotifyHub.Infrastructure/
COPY NotifyHub.API/NotifyHub.API.csproj                 NotifyHub.API/

# Restore dependencies
RUN dotnet restore NotifyHub.API/NotifyHub.API.csproj

# Copy all source code
COPY NotifyHub.Domain/       NotifyHub.Domain/
COPY NotifyHub.Application/  NotifyHub.Application/
COPY NotifyHub.Infrastructure/ NotifyHub.Infrastructure/
COPY NotifyHub.API/          NotifyHub.API/

# Build and publish
WORKDIR /src/NotifyHub.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "NotifyHub.API.dll"]