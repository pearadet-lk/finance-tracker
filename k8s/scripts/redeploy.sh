#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════
#  Finance — Redeploy Script
#  Rebuilds one or both images and triggers a rolling update.
#
#  Usage:
#    ./k8s/scripts/redeploy.sh           # rebuild + redeploy all
#    ./k8s/scripts/redeploy.sh api       # only API
#    ./k8s/scripts/redeploy.sh frontend  # only frontend
# ═══════════════════════════════════════════════════════════════════
set -euo pipefail

CYAN='\033[0;36m'; GREEN='\033[0;32m'; BOLD='\033[1m'; NC='\033[0m'
info()    { echo -e "${CYAN}[INFO]${NC}  $*"; }
success() { echo -e "${GREEN}[OK]${NC}    $*"; }

K8S_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ROOT_DIR="$(cd "$K8S_DIR/.." && pwd)"
TARGET="${1:-all}"

# Point at Minikube's Docker
eval "$(minikube docker-env)"

rebuild_api() {
  info "Rebuilding API image..."
  docker build -t finance-tracker/api:local \
    -f "$ROOT_DIR/backend/Dockerfile" \
    "$ROOT_DIR/backend"
  info "Rolling out API deployment..."
  kubectl rollout restart deployment/finance-api -n finance-tracker
  kubectl rollout status  deployment/finance-api -n finance-tracker --timeout=120s
  success "API redeployed"
}

rebuild_frontend() {
  info "Rebuilding Frontend image..."
  docker build -t finance-tracker/frontend:local \
    -f "$ROOT_DIR/frontend/Dockerfile" \
    "$ROOT_DIR/frontend"
  info "Rolling out Frontend deployment..."
  kubectl rollout restart deployment/finance-frontend -n finance-tracker
  kubectl rollout status  deployment/finance-frontend -n finance-tracker --timeout=60s
  success "Frontend redeployed"
}

case "$TARGET" in
  api)      rebuild_api ;;
  frontend) rebuild_frontend ;;
  all|*)    rebuild_api; rebuild_frontend ;;
esac

echo ""
echo -e "${BOLD}${GREEN}  ✅  Redeploy complete${NC}"
kubectl get pods -n finance-tracker
echo ""
