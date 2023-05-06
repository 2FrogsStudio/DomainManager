NAME = 2FrogsStudio/domain-manager

.PHONY: build run

build:
	dotnet build
run:
	docker compose -f "../.hosting/docker-compose.postgres.local.yaml" up -d
	dotnet run --project DomainManager
stop:
	docker compose -f "../.hosting/docker-compose.postgres.local.yaml" down
