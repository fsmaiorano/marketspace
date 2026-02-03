#!/bin/bash

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Define paths relative to the script directory
OUT_DIR="$SCRIPT_DIR/../Migrations"  # src/Services/Payment/Payment.Api/Infrastructure/Data
PROJECT_PATH="$SCRIPT_DIR/../../../Payment.Api.csproj"
STARTUP_PROJECT_PATH="$SCRIPT_DIR/../../../../Payment.Api/Payment.Api.csproj"

# Prompt the user for a migration name
read -p "Enter the migration name: " migration_name

# Check if the migration name is empty
if [ -z "$migration_name" ]; then
  echo "Migration name cannot be empty. Exiting."
  exit 1
fi

# Run the dotnet ef migrations add command
dotnet ef migrations add "$migration_name" -o "$OUT_DIR" -p "$PROJECT_PATH" -s "$STARTUP_PROJECT_PATH"

# Check if the command was successful
if [ $? -eq 0 ]; then
  echo "Migration '$migration_name' added successfully."
  # Ask the user if they want to update the database
  read -p "Do you want to update the database now? (y/n): " update_db
  if [[ "$update_db" =~ ^[Yy]$ ]]; then
    dotnet ef database update -p "$PROJECT_PATH" -s "$STARTUP_PROJECT_PATH"
    if [ $? -eq 0 ]; then
      echo "Database updated successfully."
    else
      echo "Failed to update the database."
    fi
  else
    echo "Database update skipped."
  fi
else
  echo "Failed to add migration '$migration_name'."
fi