#!/bin/bash

MAX_RETRIES=3
RETRY_DELAY=30

echo "Starting Docker build with retry mechanism..."

for i in $(seq 1 $MAX_RETRIES); do
    echo "Attempt $i of $MAX_RETRIES"
    
    if docker-compose up --build -d; then
        echo "Build completed successfully!"
        exit 0
    else
        echo "Build failed on attempt $i"
        
        if [ $i -lt $MAX_RETRIES ]; then
            echo "Waiting $RETRY_DELAY seconds before retry..."
            sleep $RETRY_DELAY
        else
            echo "All retry attempts failed. Please check your network connection."
            exit 1
        fi
    fi
done 