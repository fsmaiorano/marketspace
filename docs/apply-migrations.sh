#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

# Navigate to the solution root (assuming docs/ is one level deep)
cd "$SCRIPT_DIR/.."

echo "Starting migration application for all services..."

# Array of project paths
PROJECTS=(
    "src/Services/Order/Order.Api/Order.Api.csproj"
    "src/Services/Basket/Basket.Api/Basket.Api.csproj"
    "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj"
    "src/Services/Payment/Payment.Api/Payment.Api.csproj"
    "src/Services/Merchant/Merchant.Api/Merchant.Api.csproj"
    "src/Services/User/User.Api/User.Api.csproj"
)

# Iterate and apply migrations
for project in "${PROJECTS[@]}"; do
    if [ -f "$project" ]; then
        echo "--------------------------------------------------"
        echo "Applying migrations for: $project"
        dotnet ef database update --project "$project"
    else
        echo "Warning: Project file not found at $project"
    fi
done

echo "--------------------------------------------------"
echo "✅ All migrations applied successfully!"

