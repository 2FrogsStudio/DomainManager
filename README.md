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
