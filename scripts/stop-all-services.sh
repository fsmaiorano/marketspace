#!/bin/bash

# MarketSpace - Stop All Services
# This script stops all running MarketSpace services

echo "🛑 Stopping MarketSpace Services..."
echo "=================================="

# Kill processes by PID file if it exists
if [ -f /tmp/marketspace_pids.txt ]; then
    echo "📋 Stopping services using PID file..."
    while read pid; do
        if kill -0 "$pid" 2>/dev/null; then
            echo "🔸 Stopping process $pid..."
            kill "$pid"
        else
            echo "🔸 Process $pid already stopped"
        fi
    done < /tmp/marketspace_pids.txt
    rm -f /tmp/marketspace_pids.txt
    echo "✅ PID file cleaned up"
fi

# Kill any remaining dotnet watch processes
echo "🔍 Checking for remaining dotnet watch processes..."
dotnet_processes=$(pgrep -f "dotnet watch" || true)

if [ -n "$dotnet_processes" ]; then
    echo "🔸 Found remaining dotnet watch processes, stopping them..."
    pkill -f "dotnet watch"
    echo "✅ All dotnet watch processes stopped"
else
    echo "✅ No remaining dotnet watch processes found"
fi

# Kill any processes on the specific ports (according to docs/port-mapping.md)
echo "🔍 Checking for processes on MarketSpace ports..."
ports=(5055 5050 5051 5053 4041 3041)

for port in "${ports[@]}"; do
    pid=$(lsof -ti:$port 2>/dev/null || true)
    if [ -n "$pid" ]; then
        echo "🔸 Stopping process on port $port (PID: $pid)..."
        kill "$pid" 2>/dev/null || true
    fi
done

echo ""
echo "✅ All MarketSpace services have been stopped!"
echo "🆔 You can verify by running: ps aux | grep dotnet"
