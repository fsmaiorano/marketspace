#!/usr/bin/env bash
set -euo pipefail

# run-seed.sh
# Runs the Seed project from the repository root.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "Running MarketSpace Seed (project: tests/Seeds/SeedApp/SeedApp.csproj)"
dotnet run --project "$REPO_ROOT/tests/Seeds/SeedApp/SeedApp.csproj" --no-launch-profile
