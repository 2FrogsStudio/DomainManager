# Domain Manager

## Self-hosted quick start

### Requirements

- Docker https://docs.docker.com/engine/install/

### Steps

- Clone this repo or download content of `.hosting` directory 
- Setup your Telegram Bot with [BotFather](https://t.me/BotFather) and get the bot API token
- Make changes to `.hosting/.env` file by adding the token you received
- Open `.hosting` directory in the your terminal and run 
```sh
docker compose -f docker-compose.yaml -f docker-compose.postgres.yaml up
```

### Optioinally .env variables

- SENTRY_DSN="YOUR_SENTRY_DSN"    # send errors to Sentry
- ConnectionStrings__Postgres=""  # use external postgres connection

## Local development

### Requirements

- Dotnet SDK https://dotnet.microsoft.com/en-us/download
- Docker https://docs.docker.com/engine/install/
### Steps

- Setup your Telegram Bot with [BotFather](https://t.me/BotFather) and get the bot API token
- Set [User secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in project DomainManager like this:
```json
{
    "Bot": {
        "Token": "BOT_API_TOKEN_HERE"
    }
}
```
- Open `src` directory in the your terminal and run
```sh
# build project
make build
# build and start postgre and application locally
make run 
# stop local postgre instance
make stop 
```
- Or if you do not have `make` command
```sh
# build project
dotnet build
# build and start postgre and application locally
docker compose -f "../.hosting/docker-compose.postgres.local.yaml" up -d
dotnet run --project DomainManager
# stop local postgre instance
docker compose -f "../.hosting/docker-compose.postgres.local.yaml" down
```