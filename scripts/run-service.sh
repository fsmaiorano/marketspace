#!/bin/bash

# MarketSpace - Run Individual Service
# This script allows you to run a specific service in watch mode

if [ $# -eq 0 ]; then
    echo "🚀 MarketSpace Individual Service Runner"
    echo "========================================"
    echo ""
    echo "Usage: ./run-service.sh <service-name>"
    echo ""
    echo "Available services:"
    echo "  merchant    - Merchant API (port 5055)"
    echo "  catalog     - Catalog API (port 5050)"
    echo "  basket      - Basket API (port 5051)"
    echo "  order       - Order API (port 5053)"
    echo "  bff         - Backend For Frontend API (port 4041)"
    echo "  webapp      - Web Application (port 3041)"
    echo ""
    echo "Examples:"
    echo "  ./run-service.sh merchant"
    echo "  ./run-service.sh catalog"
    echo "  ./run-service.sh bff"
    exit 1
fi

SERVICE=$1

case $SERVICE in
    "merchant")
        echo "🚀 Starting Merchant API..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Merchant/Merchant.Api"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:5055"
        ;;
    "catalog")
        echo "🚀 Starting Catalog API..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Catalog/Catalog.Api"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:5050"
        ;;
    "basket")
        echo "🚀 Starting Basket API..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Basket/Basket.Api"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:5051"
        ;;
    "order")
        echo "🚀 Starting Order API..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Order/Order.Api"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:5053"
        ;;
    "bff")
        echo "🚀 Starting Backend For Frontend API..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Edges/BackendForFrontend.Api"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:4041"
        ;;
    "webapp")
        echo "🚀 Starting Web Application..."
        cd "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Uis/WebApp"
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet watch run --urls "https://localhost:3041"
        ;;
    *)
        echo "❌ Unknown service: $SERVICE"
        echo ""
        echo "Available services: merchant, catalog, basket, order, bff, webapp"
        exit 1
        ;;
esac
