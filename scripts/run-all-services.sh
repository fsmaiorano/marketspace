#!/bin/bash

# MarketSpace - Run All Services in Watch Mode
# This script starts all backend services and BFF in development mode

echo "🚀 Starting MarketSpace Services..."
echo ""
echo ""
echo "🎉 All services startup initiated!"
echo "================================================"
echo "📋 Service URLs (HTTPS only):"
echo "   • Merchant API:  https://localhost:5055"
echo "   • Catalog API:   https://localhost:5050"
echo "   • Basket API:    https://localhost:5051"
echo "   • Order API:     https://localhost:5053"
echo "   • BFF API:       https://localhost:4041"
echo ""
echo "📋 Swagger URLs:"
echo "   • Merchant API:  https://localhost:5055/index.html"
echo "   • Catalog API:   https://localhost:5050/index.html"
echo "   • Basket API:    https://localhost:5051/index.html"
echo "   • Order API:     https://localhost:5053/index.html"
echo "   • BFF API:       https://localhost:4041/index.html"
echo ""
echo "🛑 To stop all services, run: ./scripts/stop-all-services.sh"e Development Environment - HTTPS Only"
echo "=================================================="
echo "Starting all services in watch mode..."
echo ""
echo "Services will be available at:"
echo "   • Merchant API:  https://localhost:5055"
echo "   • Catalog API:   https://localhost:5050"
echo "   • Basket API:    https://localhost:5051"
echo "   • Order API:     https://localhost:5053"
echo "   • BFF API:       https://localhost:4041"
echo ""
echo "Swagger endpoints:"
echo "   • Merchant API:  https://localhost:5055/index.html"
echo "   • Catalog API:   https://localhost:5050/index.html"
echo "   • Basket API:    https://localhost:5051/index.html"
echo "   • Order API:     https://localhost:5053/index.html"
echo "   • BFF API:       https://localhost:4041/index.html"
echo ""de..."
echo "================================================"

MARKETSPACE_PORTS=(5055 5050 5051 5053 4041)

# Function to forcefully kill processes on a port
kill_port() {
    local port=$1
    echo "🔍 Checking port $port..."
    
    # Find processes using the port
    local pids=$(lsof -ti:$port 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        echo "🔸 Found processes on port $port: $pids"
        for pid in $pids; do
            # Try graceful kill first
            if kill -TERM "$pid" 2>/dev/null; then
                echo "   Sent TERM signal to PID $pid"
                sleep 2
                
                # Check if still running, force kill if needed
                if kill -0 "$pid" 2>/dev/null; then
                    echo "   Force killing PID $pid"
                    kill -KILL "$pid" 2>/dev/null || true
                fi
            fi
        done
    fi
}

# Function to wait for port to be free
wait_for_port_free() {
    local port=$1
    local max_attempts=10
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if ! lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
            echo "✅ Port $port is now free"
            return 0
        fi
        echo "⏳ Waiting for port $port to be free (attempt $((attempt + 1))/$max_attempts)..."
        sleep 1
        attempt=$((attempt + 1))
    done
    
    echo "⚠️  Port $port is still in use after $max_attempts attempts"
    return 1
}

# Function to start a service in background
start_service() {
    local service_name=$1
    local project_path=$2
    local https_port=$3
    
    echo ""
    echo "🔧 Starting $service_name..."
    
    # Final check if port is free
    if lsof -Pi :$https_port -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo "❌ HTTPS port $https_port is still in use, skipping $service_name"
        return 1
    fi
    
    # Start the service in background
    (
        cd "$project_path" || exit 1
        export ASPNETCORE_ENVIRONMENT=Development
        echo "🚀 Launching: dotnet watch run --urls https://localhost:$https_port in $project_path"
        dotnet watch run --urls "https://localhost:$https_port" 2>&1 | sed "s/^/[$service_name] /"
    ) &
    
    local pid=$!
    
    # Store the PID
    echo $pid >> /tmp/marketspace_pids.txt
    echo "✅ $service_name started on https://localhost:$https_port (PID: $pid)"
    
    # Give the service time to start
    sleep 3
}

# Clean up any existing PID file
rm -f /tmp/marketspace_pids.txt

# Step 1: Kill all existing dotnet watch processes
echo "🧹 Step 1: Stopping all dotnet watch processes..."
pkill -f "dotnet watch" 2>/dev/null && echo "✅ Stopped dotnet watch processes" || echo "ℹ️  No dotnet watch processes found"
sleep 2

# Step 2: Kill processes on MarketSpace ports
echo ""
echo "🧹 Step 2: Cleaning up MarketSpace ports..."
for port in "${MARKETSPACE_PORTS[@]}"; do
    kill_port $port
done

# Step 3: Wait for all ports to be free
echo ""
echo "⏳ Step 3: Waiting for all ports to be available..."
all_ports_free=true
for port in "${MARKETSPACE_PORTS[@]}"; do
    if ! wait_for_port_free $port; then
        all_ports_free=false
    fi
done

if [ "$all_ports_free" = false ]; then
    echo ""
    echo "⚠️  Some ports are still in use. You may need to:"
    echo "   1. Disable AirPlay Receiver (System Preferences > Sharing > AirPlay Receiver)"
    echo "   2. Manually kill processes using: sudo lsof -ti:PORT | xargs kill -9"
    echo "   3. Restart your machine"
    echo ""
    read -p "Do you want to continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Aborted"
        exit 1
    fi
fi

echo ""
echo "✅ Cleanup completed! Starting services..."
echo "========================================"

# Start all services with delays between them
start_service "Merchant API" \
    "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Merchant/Merchant.Api" \
    "5055"

start_service "Catalog API" \
    "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Catalog/Catalog.Api" \
    "5050"

start_service "Basket API" \
    "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Basket/Basket.Api" \
    "5051"

start_service "Order API" \
    "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Services/Order/Order.Api" \
    "5053"

start_service "BFF API" \
    "/Users/fsmaiorano/Development/projects/dotnet/MarketSpace/src/Edges/BackendForFrontend.Api" \
    "4041"

# Wait a moment for all services to fully initialize
sleep 5

echo ""
echo "🎉 All services startup initiated!"
echo "================================================"
echo "📋 Service URLs:"
echo "   • Merchant API:  https://localhost:5055"
echo "   • Catalog API:   https://localhost:5050"
echo "   • Basket API:    https://localhost:5051"
echo "   • Order API:     https://localhost:5053"
echo "   • BFF API:       https://localhost:4041"
echo ""
echo "� Swagger URLs:"
echo "   • Merchant API:  https://localhost:5055/index.html"
echo "   • Catalog API:   https://localhost:5050/index.html"
echo "   • Basket API:    https://localhost:5051/index.html"
echo "   • Order API:     https://localhost:5053/index.html"
echo "   • BFF API:       https://localhost:4041/index.html"
echo ""
echo "🛑 To stop all services, run: ./scripts/stop-all-services.sh"
echo "📊 To check service status, run: ./scripts/check-services.sh"
echo ""
echo "⏳ Services are starting... Please wait ~30 seconds for all services to be fully ready."
echo "🔍 All service logs are prefixed with [ServiceName] for easy identification"

# Wait for user input
echo ""
echo "Press Ctrl+C to stop all services and exit..."

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "🛑 Stopping all services..."
    
    if [ -f /tmp/marketspace_pids.txt ]; then
        while read -r pid; do
            if kill -0 "$pid" 2>/dev/null; then
                echo "Stopping process $pid..."
                kill -TERM "$pid" 2>/dev/null || true
                sleep 1
                # Force kill if still running
                if kill -0 "$pid" 2>/dev/null; then
                    kill -KILL "$pid" 2>/dev/null || true
                fi
            fi
        done < /tmp/marketspace_pids.txt
        rm -f /tmp/marketspace_pids.txt
    fi
    
    # Kill any remaining dotnet watch processes
    pkill -f "dotnet watch" 2>/dev/null || true
    
    echo "✅ All services stopped"
    exit 0
}

# Set up signal handling
trap cleanup SIGINT SIGTERM

# Wait indefinitely
while true; do
    sleep 1
done
