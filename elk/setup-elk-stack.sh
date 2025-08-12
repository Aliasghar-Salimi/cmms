#!/bin/bash

# CMMS ELK Stack - Complete Setup Script
# This script sets up the entire ELK stack for CMMS audit log visualization

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ELK_DIR="$PROJECT_ROOT/elk"
DOCKER_COMPOSE_FILE="$PROJECT_ROOT/docker-compose.yml"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  CMMS ELK Stack - Setup${NC}"
echo -e "${BLUE}================================${NC}"
echo -e "${PURPLE}Project Root: $PROJECT_ROOT${NC}"
echo -e "${PURPLE}ELK Directory: $ELK_DIR${NC}"

# Function to check prerequisites
check_prerequisites() {
    echo -e "${YELLOW}Checking prerequisites...${NC}"
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}✗ Docker is not installed${NC}"
        echo -e "${YELLOW}Please install Docker first: https://docs.docker.com/get-docker/${NC}"
        exit 1
    fi
    
    # Check if Docker Compose is installed
    if ! command -v docker-compose &> /dev/null; then
        echo -e "${RED}✗ Docker Compose is not installed${NC}"
        echo -e "${YELLOW}Please install Docker Compose first: https://docs.docker.com/compose/install/${NC}"
        exit 1
    fi
    
    # Check if Docker daemon is running
    if ! docker info &> /dev/null; then
        echo -e "${RED}✗ Docker daemon is not running${NC}"
        echo -e "${YELLOW}Please start Docker daemon first${NC}"
        exit 1
    fi
    
    # Check if docker-compose.yml exists
    if [ ! -f "$DOCKER_COMPOSE_FILE" ]; then
        echo -e "${RED}✗ docker-compose.yml not found at $DOCKER_COMPOSE_FILE${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ All prerequisites met${NC}"
}

# Function to wait for service to be ready
wait_for_service() {
    local service_name=$1
    local service_url=$2
    local timeout=${3:-300}
    local interval=${4:-10}
    
    echo -e "${YELLOW}Waiting for ${service_name} to be ready...${NC}"
    
    local elapsed=0
    while [ $elapsed -lt $timeout ]; do
        if curl -s -f "$service_url" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ ${service_name} is ready!${NC}"
            return 0
        fi
        
        echo -e "${YELLOW}  ${service_name} not ready yet, waiting ${interval}s... (${elapsed}/${timeout}s)${NC}"
        sleep $interval
        elapsed=$((elapsed + interval))
    done
    
    echo -e "${RED}✗ Timeout waiting for ${service_name} to be ready${NC}"
    return 1
}

# Function to start ELK services
start_elk_services() {
    echo -e "${BLUE}Starting ELK services...${NC}"
    
    # Navigate to project root
    cd "$PROJECT_ROOT"
    
    # Start only ELK services
    echo -e "${YELLOW}Starting Elasticsearch, Logstash, and Kibana...${NC}"
    docker-compose up -d elasticsearch logstash kibana
    
    # Wait for services to be ready
    echo -e "${YELLOW}Waiting for services to start...${NC}"
    sleep 30
    
    # Wait for Elasticsearch
    if ! wait_for_service "Elasticsearch" "http://localhost:9200/_cluster/health" 300 15; then
        echo -e "${RED}Failed to start Elasticsearch${NC}"
        docker-compose logs elasticsearch
        exit 1
    fi
    
    # Wait for Kibana
    if ! wait_for_service "Kibana" "http://localhost:5601/api/status" 300 15; then
        echo -e "${RED}Failed to start Kibana${NC}"
        docker-compose logs kibana
        exit 1
    fi
    
    # Wait for Logstash
    if ! wait_for_service "Logstash" "http://localhost:9600" 300 15; then
        echo -e "${RED}Failed to start Logstash${NC}"
        docker-compose logs logstash
        exit 1
    fi
    
    echo -e "${GREEN}✓ All ELK services are running${NC}"
}

# Function to setup Kibana
setup_kibana() {
    echo -e "${BLUE}Setting up Kibana dashboards...${NC}"
    
    # Navigate to ELK directory
    cd "$ELK_DIR"
    
    # Run Kibana setup script (prefer NDJSON version)
    if [ -f "kibana/setup-kibana-ndjson.sh" ]; then
        echo -e "${YELLOW}Running Kibana setup script (NDJSON version)...${NC}"
        ./kibana/setup-kibana-ndjson.sh
    elif [ -f "kibana/setup-kibana.sh" ]; then
        echo -e "${YELLOW}Running Kibana setup script...${NC}"
        ./kibana/setup-kibana.sh
    else
        echo -e "${RED}✗ Kibana setup script not found${NC}"
        exit 1
    fi
}

# Function to test the setup
test_setup() {
    echo -e "${BLUE}Testing ELK stack setup...${NC}"
    
    # Navigate to ELK directory
    cd "$ELK_DIR"
    
    # Run test script
    if [ -f "test-elk-setup.sh" ]; then
        echo -e "${YELLOW}Running ELK stack tests...${NC}"
        ./test-elk-setup.sh
    else
        echo -e "${YELLOW}⚠ Test script not found, skipping tests${NC}"
    fi
}

# Function to display service information
show_service_info() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Service Information${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${GREEN}Elasticsearch:${NC} http://localhost:9200"
    echo -e "${GREEN}Kibana:${NC} http://localhost:5601"
    echo -e "${GREEN}Logstash API:${NC} http://localhost:9600"
    echo -e "${GREEN}Kafka:${NC} localhost:9092"
    echo -e "${GREEN}Kafka UI:${NC} http://localhost:8080"
    echo ""
    echo -e "${BLUE}Main Dashboard:${NC} http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview"
    echo -e "${BLUE}Discover:${NC} http://localhost:5601/app/discover"
    echo -e "${BLUE}Dev Tools:${NC} http://localhost:5601/app/dev_tools#/console"
}

# Function to display next steps
show_next_steps() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Next Steps${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${YELLOW}1.${NC} Start your CMMS microservices to generate audit logs"
    echo -e "${YELLOW}2.${NC} Access Kibana at http://localhost:5601"
    echo -e "${YELLOW}3.${NC} View the main dashboard for real-time monitoring"
    echo -e "${YELLOW}4.${NC} Use Discover to search and explore audit logs"
    echo -e "${YELLOW}5.${NC} Create custom visualizations as needed"
    echo ""
    echo -e "${GREEN}For more information, see:${NC} $ELK_DIR/README.md"
}

# Function to cleanup on exit
cleanup() {
    echo -e "${YELLOW}Cleaning up...${NC}"
    # Add any cleanup tasks here if needed
}

# Set trap for cleanup
trap cleanup EXIT

# Main execution
main() {
    echo -e "${BLUE}Starting CMMS ELK Stack Setup...${NC}"
    
    # Check prerequisites
    check_prerequisites
    
    # Start ELK services
    start_elk_services
    
    # Setup Kibana
    setup_kibana
    
    # Test the setup
    test_setup
    
    # Show service information
    show_service_info
    
    # Show next steps
    show_next_steps
    
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}  ELK Stack Setup Complete! ✓${NC}"
    echo -e "${GREEN}================================${NC}"
}

# Run main function
main "$@" 