#!/bin/bash

echo "Preparing NuGet cache to reduce network dependencies..."

# Create local NuGet cache directory
mkdir -p .nuget-cache

# Restore packages locally first
echo "Restoring packages locally..."
dotnet restore --packages .nuget-cache

echo "NuGet cache prepared. You can now try building with Docker."
echo "Run: docker-compose up --build -d" 