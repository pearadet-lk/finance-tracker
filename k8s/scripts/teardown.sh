#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════
#  Finance — Minikube Teardown Script
#  Removes all k8s resources.  Optionally stops Minikube.
#
#  Usage:
#    ./k8s/scripts/teardown.sh           # delete namespace only
#    ./k8s/scripts/teardown.sh --stop    # also stop Minikube
#    ./k8s/scripts/teardown.sh --purge   # stop + delete Minikube VM
# ═══════════════════════════════════════════════════════════════════
set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; BOLD='\033[1m'; NC='\033[0m'
info()    { echo -e "${CYAN}[INFO]${NC}  $*"; }
success() { echo -e "${GREEN}[OK]${NC}    $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC}  $*"; }

ACTION="${1:-}"

info "Deleting finance-tracker namespace and all its resources..."
kubectl delete namespace finance-tracker --ignore-not-found=true
success "Namespace deleted"

info "Removing PersistentVolumes (cluster-scoped)..."
kubectl delete pv postgres-pv redis-pv --ignore-not-found=true
success "PersistentVolumes deleted"

# Clean /etc/hosts (same logic as setup-minikube.sh — no sudo on Git Bash / some Windows shells)
if [[ -f /etc/hosts ]]; then
  # sed -i creates a temp file in the same directory; -w on the file alone is not enough
  if [[ -w /etc/hosts && -w "$(dirname /etc/hosts)" ]]; then
    HOSTS_WRITE_CMD=""
  elif command -v sudo >/dev/null 2>&1; then
    HOSTS_WRITE_CMD="sudo"
  else
    warn "Cannot auto-edit /etc/hosts (no write access and no sudo). Remove finance.local manually if needed."
    HOSTS_WRITE_CMD="MANUAL"
  fi

  if [[ "$HOSTS_WRITE_CMD" != "MANUAL" ]] && grep -q "finance.local" /etc/hosts; then
    ${HOSTS_WRITE_CMD} sed -i '' "/finance.local/d" /etc/hosts 2>/dev/null \
      || ${HOSTS_WRITE_CMD} sed -i "/finance.local/d" /etc/hosts
    success "Removed finance.local from /etc/hosts"
  fi
fi

if [[ "$ACTION" == "--stop" ]]; then
  warn "Stopping Minikube..."
  minikube stop
  success "Minikube stopped"
elif [[ "$ACTION" == "--purge" ]]; then
  warn "Deleting Minikube cluster entirely..."
  minikube delete --all
  success "Minikube deleted"
fi

echo ""
echo -e "${BOLD}${GREEN}  ✅  Teardown complete${NC}"
echo ""
