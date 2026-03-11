#!/bin/bash
# docker-entrypoint.sh - Ensures default assets exist in persistent volumes

echo "Starting GestionProduccion Entrypoint..."

# Ensure the avatars directory exists
mkdir -p /app/wwwroot/img/avatars

# Copy default avatars if they don't exist in the mounted volume
# We copy them from a temporary backup location created during build
if [ -d "/app/wwwroot/img/avatars_defaults" ]; then
    echo "Checking for default avatars in /app/wwwroot/img/avatars..."
    cp -n /app/wwwroot/img/avatars_defaults/* /app/wwwroot/img/avatars/ 2>/dev/null || true
    echo "Default avatars check complete."
fi

# Run the main application
echo "Launching GestionProduccion..."
exec dotnet GestionProduccion.dll "$@"
