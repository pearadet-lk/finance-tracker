# ☸️ Minikube Local Microservices Deployment

Deploy the full **Finance** stack on a local Minikube cluster as individual microservices — no cloud account needed.

---

## 📐 Architecture on Kubernetes

```
                    ┌─────────────────────────────────────┐
                    │         Minikube Cluster             │
                    │                                      │
  Browser  ──────▶  │  Ingress (nginx)                    │
                    │     ├── finance.local ──▶ Frontend   │
                    │     └── api.finance.local ──▶ API    │
                    │                                      │
                    │  ┌──────────┐   ┌──────────────┐    │
                    │  │ Frontend │   │  API Service  │    │
                    │  │  (nginx) │   │  (.NET 10)    │    │
                    │  │ 1 replica│   │  2 replicas   │    │
                    │  └──────────┘   └──────┬───────┘    │
                    │                        │             │
                    │              ┌─────────┴──────────┐  │
                    │              │                    │  │
                    │        ┌─────▼─────┐   ┌────────▼─┐│
                    │        │ PostgreSQL │   │  Redis   ││
                    │        │ (stateful) │   │ (cache)  ││
                    │        └─────┬──────┘   └──────────┘│
                    │              │ PVC                   │
                    │        ┌─────▼──────┐                │
                    │        │  Host Path │                │
                    │        │  /mnt/data │                │
                    └────────────────────────────────────-─┘
```

---

## 🧰 Prerequisites

Install these before running:

| Tool | Install |
|------|---------|
| **Docker Desktop** | https://www.docker.com/products/docker-desktop/ |
| **Minikube** | `brew install minikube` or https://minikube.sigs.k8s.io/docs/start/ |
| **kubectl** | `brew install kubectl` or https://kubernetes.io/docs/tasks/tools/ |

Verify:
```bash
minikube version    # v1.32+
kubectl version     # v1.28+
docker info         # Docker running
```

---

## 🚀 Quick Start (1 command)

```bash
# From project root:
make k8s-up

# Or directly:
chmod +x k8s/scripts/setup-minikube.sh
./k8s/scripts/setup-minikube.sh
```

The script will:
1. Start Minikube (4 CPU, 6 GB RAM)
2. Enable `ingress`, `metrics-server`, `dashboard` addons
3. Point Docker at Minikube's daemon (no registry needed)
4. Build `finance-tracker/api:local` and `finance-tracker/frontend:local`
5. Apply all manifests in dependency order
6. Wait for each deployment to be healthy
7. Patch `/etc/hosts` so `finance.local` resolves to Minikube IP

---

## 🌐 Service URLs

| Service | URL |
|---------|-----|
| **Frontend** | http://finance.local |
| **API** | http://api.finance.local |
| **Swagger** | http://api.finance.local/swagger |
| **Seq** | http://localhost:30080 |
| **Jaeger** | http://localhost:30686 |
| **Prometheus** | http://localhost:30090 |
| **Grafana** | http://localhost:30300 (`admin` / `admin`) |
| **Kubernetes Dashboard** | `make k8s-dashboard` |

If you prefer **not** to use Ingress, run port-forwarding instead:
```bash
make k8s-forward
# Frontend → http://localhost:3000
# API      → http://localhost:8080
# Swagger  → http://localhost:8080/swagger
# Seq      → http://localhost:30080
# Jaeger   → http://localhost:30686
# Prometheus → http://localhost:30090
# Grafana → http://localhost:30300
# Postgres → localhost:5432
# Redis    → localhost:6379
```

---

## 📁 Manifest Structure

```
k8s/
├── namespace/
│   └── namespace.yaml              # finance-tracker namespace
├── configmaps/
│   ├── api-configmap.yaml          # API env vars (non-secret)
│   ├── frontend-configmap.yaml     # Nginx config with /api proxy
│   └── postgres-configmap.yaml     # DB init SQL
├── secrets/
│   └── secrets.yaml                # DB password, JWT secret (base64)
├── postgres/
│   ├── postgres-pv.yaml            # PersistentVolume + PVC
│   └── postgres-deployment.yaml    # Deployment + ClusterIP Service
├── redis/
│   ├── redis-pv.yaml               # PersistentVolume + PVC
│   └── redis-deployment.yaml       # Deployment + ClusterIP Service
├── api/
│   └── api-deployment.yaml         # Deployment (2 replicas) + Service
│                                   # Includes initContainers for DB/Redis wait
├── frontend/
│   └── frontend-deployment.yaml    # Deployment + Service
├── ingress/
│   └── ingress.yaml                # Nginx Ingress routing
├── hpa/
│   └── api-hpa.yaml                # HPA: 1–5 pods, CPU 60% / Mem 70%
├── observability/
│   ├── seq-deployment.yaml         # Seq deployment
│   ├── seq-service.yaml            # Seq NodePort service
│   ├── jaeger-deployment.yaml      # Jaeger all-in-one deployment
│   ├── jaeger-service.yaml         # Jaeger UI/agent service
│   ├── prometheus.yaml             # Prometheus config + deployment + service
│   └── grafana.yaml                # Grafana + preconfigured Prometheus datasource
└── scripts/
    ├── setup-minikube.sh           # Full cluster setup
    ├── teardown.sh                 # Delete all resources
    ├── redeploy.sh                 # Rebuild image + rolling restart
    └── port-forward.sh             # Forward all ports to localhost
```

---

## 🛠️ Common Commands

All available as `make k8s-*` targets:

```bash
# Status overview
make k8s-status

# Tail API logs
make k8s-logs s=finance-api

# Tail DB logs
make k8s-logs s=postgres

# Rebuild and redeploy everything
make k8s-redeploy

# Rebuild only the API (faster)
make k8s-redeploy s=api

# Shell into the API pod
make k8s-shell-api

# psql in the DB pod
make k8s-shell-db

# Open Kubernetes Dashboard
make k8s-dashboard

# Stop Minikube (data preserved)
make k8s-stop

# Delete everything (keep Minikube VM)
make k8s-down

# Delete Minikube entirely (nuclear)
make k8s-purge
```

---

## ⚖️ Autoscaling (HPA)

The API has a HorizontalPodAutoscaler configured:

| Setting | Value |
|---------|-------|
| Min replicas | 1 |
| Max replicas | 5 |
| Scale up trigger | CPU > 60% |
| Scale up trigger | Memory > 70% |
| Scale down cooldown | 2 minutes |

Check HPA status:
```bash
kubectl get hpa -n finance-tracker
kubectl describe hpa finance-api-hpa -n finance-tracker
```

> ⚠️ HPA requires the `metrics-server` addon. The setup script enables it automatically.

---

## 🔒 Secrets

Secrets are base64-encoded in `k8s/secrets/secrets.yaml`. For local dev the values are:

| Secret Key | Plain Value |
|------------|-------------|
| `postgres-password` | `password` |
| `postgres-user` | `admin` |
| `postgres-db` | `finance_tracker` |
| `jwt-secret` | `local-dev-secret-key-change-in-prod-32chars!!` |

To encode a new value:
```bash
echo -n "my-new-password" | base64
```

> ⚠️ Never commit real secrets. Use **Sealed Secrets** or **Vault** for production.

---

## 🏥 Health Checks

Both liveness and readiness probes call `GET /health` on the API pod. Before deploying, add the health endpoint to `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnectionString);

app.MapHealthChecks("/health");
```

See `k8s/HEALTH_CHECK_PATCH.md` for NuGet packages needed.

---

## 🐛 Troubleshooting

**Pods stuck in `Pending`**
```bash
kubectl describe pod <pod-name> -n finance-tracker
# Often: PV not bound, or resource limits too high for Minikube
```

**`ImagePullBackOff` for local images**
```bash
# Make sure you built inside Minikube's daemon:
eval $(minikube docker-env)
docker images | grep finance-tracker
```

**Ingress not routing**
```bash
# Check addon is enabled:
minikube addons list | grep ingress

# Check ingress controller pod:
kubectl get pods -n ingress-nginx

# Check /etc/hosts has the entry:
cat /etc/hosts | grep finance.local
```

**API can't reach PostgreSQL**
```bash
# Check initContainers succeeded:
kubectl describe pod -l app=finance-api -n finance-tracker | grep -A 10 "Init Containers"

# Test connectivity from API pod:
kubectl exec -it deploy/finance-api -n finance-tracker -- nc -z postgres-service 5432
```

**Reset everything and start fresh**
```bash
make k8s-purge    # deletes Minikube completely
make k8s-up       # fresh start
```

---

## 🔴 RedisInsight Connection

Use RedisInsight to inspect cache keys while running Minikube.

1. Start port-forwarding:
```bash
make k8s-forward
```
Keep this terminal open.

2. In RedisInsight, add a new Redis database:

| Field | Value |
|------|-------|
| Name | `finance-redis-local` |
| Host (desktop app) | `localhost` |
| Host (RedisInsight in Docker) | `host.docker.internal` |
| Port | `6379` |
| Username | *(empty)* |
| Password | *(empty)* |
| TLS | `Off` |

3. Save and connect. You should see keys as API traffic runs.

Quick check from terminal:
```bash
kubectl exec -n finance-tracker deploy/redis -- redis-cli ping
# PONG
```

If connection fails:
- Ensure `make k8s-forward` is still running
- Confirm local port: `Test-NetConnection localhost -Port 6379`
- Verify Redis pod is healthy: `kubectl get pods -n finance-tracker`
