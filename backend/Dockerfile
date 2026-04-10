# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FinanceTracker.Api/FinanceTracker.Api.csproj src/FinanceTracker.Api/
COPY src/FinanceTracker.Application/FinanceTracker.Application.csproj src/FinanceTracker.Application/
COPY src/FinanceTracker.Domain/FinanceTracker.Domain.csproj src/FinanceTracker.Domain/
COPY src/FinanceTracker.Infrastructure/FinanceTracker.Infrastructure.csproj src/FinanceTracker.Infrastructure/

RUN dotnet restore src/FinanceTracker.Api/FinanceTracker.Api.csproj

COPY . .

RUN dotnet publish src/FinanceTracker.Api/FinanceTracker.Api.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Use a numeric UID so the container doesn't rely on `adduser`/package managers.
# This avoids failures on minimal base images that don't ship user-management tools.
COPY --from=build /app/publish .
# Serilog writes to logs/; non-root UID 10001 cannot create dirs under default root-owned /app
RUN mkdir -p /app/logs && chown -R 10001:10001 /app
USER 10001

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FinanceTracker.Api.dll"]
