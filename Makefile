# ──────────────────────────────────────────────────
#  Finance — Developer Makefile
#  Usage: make <target>
# ──────────────────────────────────────────────────

.PHONY: up down restart logs ps build clean test migrate shell-api shell-db \
        k8s-up k8s-down k8s-stop k8s-purge k8s-status k8s-logs k8s-redeploy \
        k8s-forward k8s-dashboard k8s-shell-api k8s-shell-db

# ── Start core services (API + Frontend + DB + Redis) ──
up:
	docker compose -f docker-compose.local.yml up --build

# ── Start core + GUI tools (pgAdmin + Redis Commander) ──
up-tools:
	docker compose -f docker-compose.local.yml --profile tools up --build

# ── Detached mode ──
up-d:
	docker compose -f docker-compose.local.yml up --build -d

# ── Stop all ──
down:
	docker compose -f docker-compose.local.yml --profile tools down

# ── Stop and remove volumes (full reset) ──
clean:
	docker compose -f docker-compose.local.yml --profile tools down -v --remove-orphans
	@echo "✅ All containers and volumes removed"

# ── Restart a single service: make restart s=api ──
restart:
	docker compose -f docker-compose.local.yml restart $(s)

# ── Follow logs: make logs s=api ──
logs:
	docker compose -f docker-compose.local.yml logs -f $(s)

# ── Show running containers ──
ps:
	docker compose -f docker-compose.local.yml ps

# ── Rebuild without cache ──
build:
	docker compose -f docker-compose.local.yml build --no-cache

# ── Run backend tests inside container ──
test:
	docker compose -f docker-compose.local.yml run --rm api \
		dotnet test tests/FinanceTracker.UnitTests --logger "console;verbosity=normal"

# ── EF Core: add migration (usage: make migrate name=InitialCreate) ──
migrate:
	docker compose -f docker-compose.local.yml run --rm api \
		dotnet ef migrations add $(name) \
		--project src/FinanceTracker.Infrastructure \
		--startup-project src/FinanceTracker.Api

# ── Open shell in API container ──
shell-api:
	docker compose -f docker-compose.local.yml exec api /bin/bash

# ── Open psql in DB container ──
shell-db:
	docker compose -f docker-compose.local.yml exec db psql -U admin -d finance_tracker


# ════════════════════════════════════════════════════════════════
#  MINIKUBE / KUBERNETES
# ════════════════════════════════════════════════════════════════

# ── Start Minikube, build images, apply all k8s manifests ──
k8s-up:
	@chmod +x k8s/scripts/setup-minikube.sh && k8s/scripts/setup-minikube.sh

# ── Delete namespace + all resources (keeps Minikube running) ──
k8s-down:
	@chmod +x k8s/scripts/teardown.sh && k8s/scripts/teardown.sh

# ── Stop Minikube VM (data preserved) ──
k8s-stop:
	minikube stop

# ── Delete Minikube cluster entirely ──
k8s-purge:
	@chmod +x k8s/scripts/teardown.sh && k8s/scripts/teardown.sh --purge

# ── Show pods / services / deployments / HPA / ingress ──
k8s-status:
	@echo "\n── Pods ──────────────────────────────────────────────"
	@kubectl get pods -n finance-tracker -o wide
	@echo "\n── Services ──────────────────────────────────────────"
	@kubectl get svc -n finance-tracker
	@echo "\n── Deployments ───────────────────────────────────────"
	@kubectl get deployments -n finance-tracker
	@echo "\n── HPA ───────────────────────────────────────────────"
	@kubectl get hpa -n finance-tracker
	@echo "\n── Ingress ───────────────────────────────────────────"
	@kubectl get ingress -n finance-tracker

# ── Tail logs for a deployment: make k8s-logs s=finance-api ──
k8s-logs:
	kubectl logs -f deploy/$(s) -n finance-tracker

# ── Rebuild image + rolling restart: make k8s-redeploy s=api ──
k8s-redeploy:
	@chmod +x k8s/scripts/redeploy.sh && k8s/scripts/redeploy.sh $(if $(s),$(s),all)

# ── Port-forward all services to localhost (bypass Ingress) ──
k8s-forward:
	@chmod +x k8s/scripts/port-forward.sh && k8s/scripts/port-forward.sh

# ── Open Kubernetes dashboard in browser ──
k8s-dashboard:
	minikube dashboard

# ── Bash shell inside API pod ──
k8s-shell-api:
	kubectl exec -it deploy/finance-api -n finance-tracker -- /bin/bash

# ── psql inside PostgreSQL pod ──
k8s-shell-db:
	kubectl exec -it deploy/postgres -n finance-tracker -- psql -U admin -d finance_tracker
