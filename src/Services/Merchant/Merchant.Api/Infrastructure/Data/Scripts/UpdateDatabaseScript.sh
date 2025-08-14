#!/bin/bash

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}" )" && pwd)"

# Define paths relative to the script directory
PROJECT_PATH="$SCRIPT_DIR/../../../Merchant.Api.csproj"
STARTUP_PROJECT_PATH="$SCRIPT_DIR/../../../../Merchant.Api/Merchant.Api.csproj"

# Run the dotnet ef database update command
dotnet ef database update -p "$PROJECT_PATH" -s "$STARTUP_PROJECT_PATH"

# Check if the command was successful
if [ $? -eq 0 ]; then
  echo "Database updated successfully."
else
  echo "Failed to update the database."
fi

