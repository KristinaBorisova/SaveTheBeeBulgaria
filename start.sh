#!/bin/bash

# Railway startup script for HoneyWebPlatform
echo "Starting HoneyWebPlatform application..."

# Set environment variables for Railway
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:$PORT

# Run the application
dotnet HoneyWebPlatform.Web.dll
