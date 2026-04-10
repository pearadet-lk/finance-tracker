#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════
#  Finance — Port Forward Script
#  Opens local ports to all services for direct debugging.
#  Use this when you don't want to depend on Ingress.
#
#  Usage:  ./k8s/scripts/port-forward.sh
#  Stop:   Ctrl+C
# ═══════════════════════════════════════════════════════════════════
set -euo pipefail

CYAN='\033[0;36m'; BOLD='\033[1m'; GREEN='\033[0;32m'; NC='\033[0m'

NS="finance-tracker"

echo -e "${BOLD}${CYAN}Starting port forwards for all services...${NC}"
echo ""

# Kill any existing port-forwards for this project
pkill -f "kubectl port-forward.*finance-tracker" 2>/dev/null || true
sleep 1

# Start forwards in background
kubectl port-forward svc/api-service       8080:8080 -n $NS &
kubectl port-forward svc/frontend-service  3000:80   -n $NS &
kubectl port-forward svc/postgres-service  5432:5432 -n $NS &
kubectl port-forward svc/redis-service     6379:6379 -n $NS &
kubectl port-forward svc/seq               30080:80  -n $NS &
kubectl port-forward svc/jaeger            30686:16686 -n $NS &
kubectl port-forward svc/prometheus        30090:9090 -n $NS &
kubectl port-forward svc/grafana           30300:3000 -n $NS &

# Trap Ctrl+C to kill all background jobs
cleanup() {
  echo -e "\n${CYAN}Stopping all port forwards...${NC}"
  pkill -f "kubectl port-forward.*finance-tracker" 2>/dev/null || true
  kill $(jobs -p) 2>/dev/null || true
}
trap cleanup INT TERM

echo -e "${BOLD}${GREEN}Port forwards active:${NC}"
echo ""
echo -e "  Frontend  → ${BOLD}http://localhost:3000${NC}"
echo -e "  API       → ${BOLD}http://localhost:8080${NC}"
echo -e "  Swagger   → ${BOLD}http://localhost:8080/swagger${NC}"
echo -e "  Seq       → ${BOLD}http://localhost:30080${NC}"
echo -e "  Jaeger    → ${BOLD}http://localhost:30686${NC}"
echo -e "  Prometheus→ ${BOLD}http://localhost:30090${NC}"
echo -e "  Grafana   → ${BOLD}http://localhost:30300${NC}  (admin/admin)"
echo -e "  PostgreSQL→ ${BOLD}localhost:5432${NC}  (user: admin / password: password)"
echo -e "  Redis     → ${BOLD}localhost:6379${NC}"
echo ""
echo -e "  Press ${BOLD}Ctrl+C${NC} to stop all forwards"
echo ""

# Wait until killed
wait
