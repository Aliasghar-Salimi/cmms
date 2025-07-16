#!/bin/bash

echo "🚀 Building CMMS Identity Service with Docker..."

# Clean up any existing containers
echo "🧹 Cleaning up existing containers..."
docker-compose down

# Clean up any existing images with this name = "cmms-identity-service:latest"
docker rmi cmms-identity-service:latest

# Function to build with retry
build_with_retry() {
    local max_attempts=3
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        echo "📦 Build attempt $attempt of $max_attempts..."
        
        if docker-compose build identity-service; then
            echo "✅ Build successful!"
            return 0
        else
            echo "❌ Build attempt $attempt failed"
            if [ $attempt -lt $max_attempts ]; then
                echo "⏳ Waiting 30 seconds before retry..."
                sleep 30
            fi
            ((attempt++))
        fi
    done
    
    echo "💥 All build attempts failed"
    return 1
}


# Build with retry
if build_with_retry; then
    echo "🎉 Build completed successfully!"
    echo "🚀 Starting services..."
    docker-compose up -d
    
    echo "⏳ Waiting for services to be ready..."
    sleep 10
    
    echo "🔍 Checking service status..."
    docker-compose ps
    
    echo "🌐 Services should be available at:"
    echo "   - Identity Service: http://localhost:5000"
    echo "   - Health Check: http://localhost:5000/health"
    echo "   - Swagger: http://localhost:5000/swagger"
else
    echo "💥 Build failed. Please check the logs above for details."
    exit 1
fi 