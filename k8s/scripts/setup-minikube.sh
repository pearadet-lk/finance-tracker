#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════
#  Finance — Minikube Local Dev Setup Script
#
#  Usage:  chmod +x k8s/scripts/setup-minikube.sh
#          ./k8s/scripts/setup-minikube.sh
#
#  What it does:
#   1. Starts Minikube with enough resources
#   2. Enables required addons (ingress, metrics-server, dashboard)
#   3. Builds Docker images directly inside Minikube (no registry needed)
#   4. Applies all Kubernetes manifests in order
#   5. Patches /etc/hosts so finance.local resolves
#   6. Prints all service URLs
# ═══════════════════════════════════════════════════════════════════
set -euo pipefail

# ── Colours ──────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; NC='\033[0m'

info()    { echo -e "${CYAN}[INFO]${NC}  $*"; }
success() { echo -e "${GREEN}[OK]${NC}    $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC}  $*"; }
error()   { echo -e "${RED}[ERROR]${NC} $*"; exit 1; }
step()    { echo -e "\n${BOLD}${CYAN}▶ $*${NC}"; }

K8S_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ROOT_DIR="$(cd "$K8S_DIR/.." && pwd)"

# ── 1. Check prerequisites ────────────────────────────────────────
step "Checking prerequisites"

command -v minikube &>/dev/null || error "minikube not found. Install: https://minikube.sigs.k8s.io/docs/start/"
command -v kubectl  &>/dev/null || error "kubectl not found.  Install: https://kubernetes.io/docs/tasks/tools/"
command -v docker   &>/dev/null || error "docker not found.   Install: https://docs.docker.com/get-docker/"

success "All prerequisites found"

# ── 2. Start Minikube ─────────────────────────────────────────────
step "Starting Minikube"

if minikube status --format='{{.Host}}' 2>/dev/null | grep -q "Running"; then
  success "Minikube already running"
else
  info "Starting Minikube with 4 CPUs, 6GB RAM, 20GB disk..."
  minikube start \
    --driver=docker \
    --cpus=4 \
    --memory=6144 \
    --disk-size=20g \
    --kubernetes-version=v1.29.0 \
    --addons=ingress,metrics-server,dashboard
  success "Minikube started"
fi

# ── 3. Enable addons ──────────────────────────────────────────────
step "Enabling Minikube addons"

for addon in ingress metrics-server dashboard; do
  if minikube addons list | grep "$addon" | grep -q "enabled"; then
    info "$addon already enabled"
  else
    minikube addons enable "$addon"
    success "$addon enabled"
  fi
done

# ── 4. Point Docker CLI at Minikube's Docker daemon ───────────────
step "Configuring Docker to use Minikube's daemon"

eval "$(minikube docker-env)"
success "Docker now targets Minikube daemon — images built here are available to k8s"

# ── 5. Build Docker images ────────────────────────────────────────
step "Building API image (finance-tracker/api:local)"

docker build \
  -t finance-tracker/api:local \
  -f "$ROOT_DIR/backend/Dockerfile" \
  "$ROOT_DIR/backend"

success "API image built"

step "Building Frontend image (finance-tracker/frontend:local)"

docker build \
  -t finance-tracker/frontend:local \
  -f "$ROOT_DIR/frontend/Dockerfile" \
  "$ROOT_DIR/frontend"

success "Frontend image built"

# ── 6. Apply manifests in dependency order ────────────────────────
step "Applying Kubernetes manifests"

apply() {
  local file="$1"
  info "Applying $file..."
  kubectl apply -f "$file"
}

apply "$K8S_DIR/namespace/namespace.yaml"

# ConfigMaps & Secrets (must exist before deployments)
apply "$K8S_DIR/configmaps/postgres-configmap.yaml"
apply "$K8S_DIR/configmaps/api-configmap.yaml"
apply "$K8S_DIR/configmaps/frontend-configmap.yaml"
apply "$K8S_DIR/secrets/secrets.yaml"

# Storage
apply "$K8S_DIR/postgres/postgres-pv.yaml"
apply "$K8S_DIR/redis/redis-pv.yaml"

# Stateful services (DB + Cache)
apply "$K8S_DIR/postgres/postgres-deployment.yaml"
apply "$K8S_DIR/redis/redis-deployment.yaml"
apply "$K8S_DIR/observability/seq-deployment.yaml"
apply "$K8S_DIR/observability/seq-service.yaml"
apply "$K8S_DIR/observability/jaeger-deployment.yaml"
apply "$K8S_DIR/observability/jaeger-service.yaml"
apply "$K8S_DIR/observability/otel-collector.yaml"
apply "$K8S_DIR/observability/prometheus.yaml"
apply "$K8S_DIR/observability/grafana.yaml"

# Wait for DB to be ready before applying API
info "Waiting for PostgreSQL to be ready..."
kubectl rollout status deployment/postgres -n finance-tracker --timeout=120s
success "PostgreSQL ready"

info "Waiting for Redis to be ready..."
kubectl rollout status deployment/redis -n finance-tracker --timeout=60s
success "Redis ready"

info "Waiting for Seq to be ready..."
kubectl rollout status deployment/seq -n finance-tracker --timeout=60s
success "Seq ready"

info "Waiting for Jaeger to be ready..."
kubectl rollout status deployment/jaeger -n finance-tracker --timeout=60s
success "Jaeger ready"

info "Waiting for OTel Collector to be ready..."
kubectl rollout status deployment/otel-collector -n finance-tracker --timeout=60s
success "OTel Collector ready"

info "Waiting for Prometheus to be ready..."
kubectl rollout status deployment/prometheus -n finance-tracker --timeout=60s
success "Prometheus ready"

info "Waiting for Grafana to be ready..."
kubectl rollout status deployment/grafana -n finance-tracker --timeout=60s
success "Grafana ready"

# Application services
apply "$K8S_DIR/api/api-deployment.yaml"
apply "$K8S_DIR/frontend/frontend-deployment.yaml"

# Autoscaling
apply "$K8S_DIR/hpa/api-hpa.yaml"

# Ingress (last — after services exist)
apply "$K8S_DIR/ingress/ingress.yaml"

success "All manifests applied"

# ── 7. Wait for rollout ───────────────────────────────────────────
step "Waiting for all deployments to be ready"

kubectl rollout status deployment/finance-api      -n finance-tracker --timeout=180s
kubectl rollout status deployment/finance-frontend -n finance-tracker --timeout=60s

success "All deployments ready"

# ── 8. Patch /etc/hosts ───────────────────────────────────────────
step "Patching /etc/hosts"

MINIKUBE_IP="$(minikube ip)"
HOSTS_ENTRY="$MINIKUBE_IP finance.local api.finance.local"

# sed -i needs a writable directory for its temp file (Git Bash often has -w hosts but not /etc)
if [[ -f /etc/hosts && -w /etc/hosts && -w "$(dirname /etc/hosts)" ]]; then
  HOSTS_WRITE_CMD=""
elif command -v sudo >/dev/null 2>&1; then
  HOSTS_WRITE_CMD="sudo"
else
  warn "Cannot auto-edit /etc/hosts (no write access and no sudo)."
  warn "Add this line manually: $HOSTS_ENTRY"
  HOSTS_WRITE_CMD="MANUAL"
fi

if [[ "$HOSTS_WRITE_CMD" != "MANUAL" ]]; then
  if grep -q "finance.local" /etc/hosts; then
    # Update existing entry
    ${HOSTS_WRITE_CMD} sed -i '' "s/.*finance.local.*/$HOSTS_ENTRY/" /etc/hosts 2>/dev/null \
      || ${HOSTS_WRITE_CMD} sed -i "s/.*finance.local.*/$HOSTS_ENTRY/" /etc/hosts
    info "Updated existing /etc/hosts entry"
  else
    echo "$HOSTS_ENTRY" | ${HOSTS_WRITE_CMD} tee -a /etc/hosts >/dev/null
    success "Added $HOSTS_ENTRY to /etc/hosts"
  fi
fi

# ── 9. Print summary ──────────────────────────────────────────────
echo ""
echo -e "${BOLD}${GREEN}════════════════════════════════════════════${NC}"
echo -e "${BOLD}${GREEN}  ✅  Finance is running on Minikube!${NC}"
echo -e "${BOLD}${GREEN}════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${BOLD}Frontend${NC}   → http://finance.local"
echo -e "  ${BOLD}API${NC}        → http://api.finance.local"
echo -e "  ${BOLD}Swagger${NC}    → http://api.finance.local/swagger"
echo -e "  ${BOLD}Seq${NC}        → http://localhost:30080"
echo -e "  ${BOLD}Jaeger${NC}     → http://localhost:30686"
echo -e "  ${BOLD}Prometheus${NC} → http://localhost:30090"
echo -e "  ${BOLD}Grafana${NC}    → http://localhost:30300 (admin/admin)"
echo -e "  ${BOLD}Dashboard${NC}  → run: minikube dashboard"
echo ""
echo -e "  ${BOLD}Minikube IP${NC}: $MINIKUBE_IP"
echo ""
echo -e "  ${CYAN}Useful commands:${NC}"
# For RedisInsight in Docker, port-forward Redis and connect to host.docker.internal:6379.
echo -e "    kubectl get all -n finance-tracker"
echo -e "    kubectl logs -f deploy/finance-api -n finance-tracker"
echo -e "    kubectl logs -f deploy/postgres    -n finance-tracker"
echo -e "    kubectl port-forward svc/redis-service 6379:6379 -n finance-tracker"
echo -e "    minikube dashboard"
echo ""
