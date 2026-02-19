#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$ROOT_DIR/infra/MarketSpace.AppHost/aspire-output/.env"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "ERROR: env file not found: $ENV_FILE"
  exit 1
fi

# helper to read variable from .env (returns empty string when missing)
get() {
  awk -F'=' -v key="$1" '$1==key { $1=""; sub(/^=/,""); print; exit }' "$ENV_FILE" || true
}

# Compatibility: use indexed arrays instead of associative arrays (works on macOS bash)
KEYS=(
  "BACKENDFORFRONTEND_API_IMAGE"
  "BASKET_API_IMAGE"
  "CATALOG_API_IMAGE"
  "MERCHANT_API_IMAGE"
  "ORDER_API_IMAGE"
  "PAYMENT_API_IMAGE"
  "USER_API_IMAGE"
)

PATHS=(
  "src/Edges/BackendForFrontend.Api"
  "src/Services/Basket/Basket.Api"
  "src/Services/Catalog/Catalog.Api"
  "src/Services/Merchant/Merchant.Api"
  "src/Services/Order/Order.Api"
  "src/Services/Payment/Payment.Api"
  "src/Services/User/User.Api"
)

echo "Using env: $ENV_FILE"

for i in "${!KEYS[@]}"; do
  VAR="${KEYS[$i]}"
  SERVICE_DIR="$ROOT_DIR/${PATHS[$i]}"
  DOCKERFILE="$SERVICE_DIR/Dockerfile"

  IMAGE="$(get "$VAR")" || IMAGE=""
  # trim leading/trailing whitespace
  IMAGE="$(echo "$IMAGE" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"

  if [[ -z "$IMAGE" ]]; then
    echo "- Skipping $VAR (not set in .env)"
    continue
  fi

  # basic validation: image must not contain whitespace
  if [[ "$IMAGE" =~ [[:space:]] ]]; then
    echo "- Invalid image name in $VAR: '$IMAGE' (contains whitespace). Skipping."
    continue
  fi

  if [[ ! -f "$DOCKERFILE" ]]; then
    echo "- Skipping $IMAGE: Dockerfile not found at $DOCKERFILE"
    continue
  fi

  echo "- Building $IMAGE from $DOCKERFILE"
  docker build -t "$IMAGE" -f "$DOCKERFILE" "$ROOT_DIR"
done

echo "All requested images built (or skipped). You can now run:"
echo "  docker compose -f infra/MarketSpace.AppHost/aspire-output/docker-compose.yaml up"
