# 🐳 Local Docker Development Guide

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose plugin)
- Nothing else — no .NET SDK or Node.js required locally!

---

## 🚀 Start in 1 command

```bash
# Clone and go
git clone https://github.com/pearadet-lk/finance-tracker.git
cd finance-tracker

# Start everything (builds images automatically)
docker compose -f docker-compose.local.yml up --build
```

Or with the Makefile:

```bash
make up
```

## 🌐 Service URLs

| Service       | URL                              | Notes                        |
|---------------|----------------------------------|------------------------------|
| **Frontend**  | http://localhost:5173            | React + Vite HMR             |
| **API**       | http://localhost:5001            | .NET 10 with dotnet watch    |
| **Swagger**   | http://localhost:5001/swagger    | Interactive API docs         |
| **Prometheus**| http://localhost:9090            | API metrics + PromQL         |
| **Grafana**   | http://localhost:3001            | Dashboards (`admin`/`admin`) |
| **pgAdmin**   | http://localhost:5050            | `--profile tools` required   |
| **Redis UI**  | http://localhost:8081            | `--profile tools` required   |
| **PostgreSQL**| localhost:5432                   | Direct DB access             |
| **Redis**     | localhost:6379                   | Direct cache access          |

---

## 🛠️ Common Commands

### Start with GUI tools (pgAdmin + Redis Commander)
```bash
docker compose -f docker-compose.local.yml --profile tools up --build
# or
make up-tools
```

### Run in background (detached)
```bash
docker compose -f docker-compose.local.yml up -d
# or
make up-d
```

### Watch logs for a specific service
```bash
docker compose -f docker-compose.local.yml logs -f api
docker compose -f docker-compose.local.yml logs -f frontend
# or
make logs s=api
make logs s=frontend
```

### Restart a single service after config change
```bash
docker compose -f docker-compose.local.yml restart api
# or
make restart s=api
```

### Open a shell inside the API container
```bash
docker compose -f docker-compose.local.yml exec api /bin/bash
# or
make shell-api
```

### Open psql (PostgreSQL shell)
```bash
docker compose -f docker-compose.local.yml exec db psql -U admin -d finance_tracker
# or
make shell-db
```

### Run tests
```bash
docker compose -f docker-compose.local.yml run --rm api \
  dotnet test tests/FinanceTracker.UnitTests
# or
make test
```

### Add an EF Core migration
```bash
make migrate name=AddUserPreferences
```

---

## 🔥 Hot Reload

Both services support hot reload — **no restarts needed** when you edit code:

| Service  | Hot Reload Engine | Behaviour                              |
|----------|-------------------|----------------------------------------|
| Backend  | `dotnet watch`    | Recompiles and restarts API on save    |
| Frontend | Vite HMR          | Updates browser instantly without reload |

Source folders are **volume-mounted** into the containers, so edits on your host machine take effect immediately.

---

## 🗄️ pgAdmin Setup

pgAdmin is pre-configured to connect to the local PostgreSQL container automatically.

1. Start with tools profile: `make up-tools`
2. Open http://localhost:5050
3. Login: `admin@local.dev` / `admin`
4. The **FinanceTracker Local** server is already listed — click it to explore

---

## 🔴 Redis Commander

1. Start with tools profile: `make up-tools`
2. Open http://localhost:8081
3. Browse all cached keys in real-time

---

## 🧹 Full Reset (delete all data)

```bash
docker compose -f docker-compose.local.yml --profile tools down -v --remove-orphans
# or
make clean
```

This removes all containers **and volumes** (PostgreSQL data, Redis data, etc.).

---

## 🔐 Local Credentials

| Service    | Username | Password  |
|------------|----------|-----------|
| PostgreSQL | admin    | password  |
| pgAdmin    | admin@local.dev | admin |

> ⚠️ These are for local development only. Never use in production.

---

## 💡 Tips

- **Port conflict?** Change the host port in `docker-compose.local.yml` (left side of `ports: "HOST:CONTAINER"`)
- **Slow first start?** Normal — Docker is pulling images and building. Subsequent starts are fast.
- **NuGet packages** are cached in a Docker volume (`dotnet_cache`) so restores are fast after the first build.
- **node_modules** are cached in a Docker volume (`node_modules_cache`) so `npm ci` only runs once.
