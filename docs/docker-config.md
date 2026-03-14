## Deploy docker-compose

### Start full stack
docker compose -f docker-compose.yml -f docker-compose.observability.yml up -d
### Or observability only (services running locally)
docker compose -f docker-compose.observability.yml up -d
