#!/usr/bin/env bash
set -euo pipefail

# run-simulator.sh
# Runs the Simulator project from the repository root.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "Running MarketSpace Simulator (project: tests/Seeds/Simulator/Simulator.csproj)"
dotnet run --project "$REPO_ROOT/tests/Seeds/Simulator/Simulator.csproj" --no-launch-profile
