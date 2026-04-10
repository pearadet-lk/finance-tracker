# 💰 Finance — Personal Finance Tracker

A production-grade, full-stack personal finance tracker built with:

- **Frontend**: React 18 + Vite + Redux Toolkit + Recharts + Tailwind CSS
- **Backend**: .NET 10 Web API — Clean Architecture + CQRS + MediatR
- **Cache**: Redis (StackExchange.Redis)
- **Database**: PostgreSQL (Neon-compatible)
- **Auth**: JWT Bearer tokens
- **Infra**: Docker Compose + GitHub Actions CI/CD

---

## 🏗️ Architecture

```
finance-tracker/
├── backend/
│   ├── src/
│   │   ├── FinanceTracker.Api            # Controllers, Middleware, Program.cs
│   │   ├── FinanceTracker.Application   # CQRS Commands/Queries, DTOs, Validators
│   │   ├── FinanceTracker.Domain        # Entities, Enums (pure C#, no dependencies)
│   │   └── FinanceTracker.Infrastructure # EF Core DbContext, Redis Service
│   └── tests/
│       ├── FinanceTracker.UnitTests
│       └── FinanceTracker.IntegrationTests
├── frontend/
│   └── src/
│       ├── app/          # Redux store + typed hooks
│       ├── features/     # auth | transactions | dashboard | categories slices
│       ├── pages/        # Dashboard, Transactions, Login, Register
│       ├── components/   # Layout, shared UI
│       └── services/     # Axios instance with JWT interceptor
├── docker/
│   └── docker-compose.yml
└── .github/workflows/ci.yml
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [Docker + Docker Compose](https://docs.docker.com/compose/)

---

### Option A — Docker Compose (recommended)

```bash
# Clone the repo
git clone https://github.com/pearadet-lk/finance-tracker.git
cd finance-tracker

# Start everything
docker compose -f docker/docker-compose.yml up --build
```

| Service  | URL                        |
|----------|----------------------------|
| Frontend | http://localhost:3000       |
| API      | http://localhost:5001       |
| Swagger  | http://localhost:5001/swagger |

---

### Option B — Local Development

**1. Start infrastructure**
```bash
docker compose -f docker/docker-compose.yml up redis db -d
```

**2. Backend**
```bash
cd backend/src/FinanceTracker.Api

# Update appsettings.json connection strings if needed
dotnet run
# API at https://localhost:5001 | Swagger at https://localhost:5001/swagger
```

**3. Frontend**
```bash
cd frontend
npm install
npm run dev
# App at http://localhost:5173
```

---

## 🔑 Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__DefaultConnection` | see appsettings.json | PostgreSQL connection |
| `ConnectionStrings__Redis` | `localhost:6379` | Redis host |
| `Jwt__Secret` | (must change!) | Min 32-char signing key |
| `Jwt__Issuer` | `FinanceTracker` | JWT issuer |
| `Jwt__Audience` | `FinanceTrackerClient` | JWT audience |
| `VITE_API_URL` | `https://localhost:5001/api` | Frontend API base URL |

---

## 📡 API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Create account | ❌ |
| POST | `/api/auth/login` | Get JWT token | ❌ |
| GET | `/api/transactions` | List with pagination & filters | ✅ |
| POST | `/api/transactions` | Create transaction | ✅ |
| DELETE | `/api/transactions/{id}` | Soft delete | ✅ |
| GET | `/api/categories` | List categories | ✅ |
| GET | `/api/dashboard` | Summary + charts data | ✅ |

### Query parameters for `GET /api/transactions`
```
?page=1&pageSize=20&type=Expense&categoryId=<guid>&from=2025-01-01&to=2025-12-31
```

---

## ⚡ Redis Caching Strategy

| Cache Key | TTL | Invalidated On |
|-----------|-----|----------------|
| `transactions:{userId}:{filters}` | 5 min | Create / Delete transaction |
| `dashboard:{userId}:{year}` | 10 min | Create / Delete transaction |

---

## 🧪 Running Tests

```bash
# Unit tests
dotnet test backend/tests/FinanceTracker.UnitTests

# Integration tests (requires running DB + Redis)
dotnet test backend/tests/FinanceTracker.IntegrationTests

# All tests
dotnet test backend/FinanceTracker.sln
```

---

## 🗄️ Database Migrations

```bash
cd backend/src/FinanceTracker.Api

# Add new migration
dotnet ef migrations add MigrationName \
  --project ../FinanceTracker.Infrastructure \
  --startup-project .

# Apply migration
dotnet ef database update \
  --project ../FinanceTracker.Infrastructure \
  --startup-project .
```

---

## ☁️ Azure Deployment

| Component | Azure Service |
|-----------|--------------|
| Backend   | Azure App Service (.NET 10) |
| Frontend  | Azure Static Web Apps |
| Redis     | Azure Cache for Redis |
| Database  | Neon PostgreSQL (serverless) |

```bash
# Build and push Docker images
docker build -t your-acr.azurecr.io/finance-api ./backend
docker push your-acr.azurecr.io/finance-api
```

---

## 🧩 Key Design Decisions

- **CQRS via MediatR** — Commands (writes) and Queries (reads) are fully separated
- **Clean Architecture** — Domain has zero external dependencies; all infra details live in the outer layer
- **Global soft delete** — EF Core query filters ensure deleted records are never returned
- **MediatR Pipeline** — `ValidationBehavior<T>` automatically validates every command via FluentValidation before the handler runs
- **Global Exception Middleware** — maps domain exceptions to correct HTTP status codes + structured JSON errors
- **Serilog** — structured logging to console and rolling files; request logging via `UseSerilogRequestLogging()`

---

## 📦 Tech Stack Summary

| Layer | Technology |
|-------|-----------|
| Frontend framework | React 18 + Vite |
| State management | Redux Toolkit |
| HTTP client | Axios (with JWT interceptor) |
| Charts | Recharts |
| Styling | Tailwind CSS v3 |
| Backend framework | .NET 10 Web API |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 16 |
| Caching | Redis 7 (StackExchange.Redis) |
| Auth | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| Logging | Serilog |
| Testing | xUnit + Moq + WebApplicationFactory |
| CI/CD | GitHub Actions |
| Containers | Docker + Docker Compose |
