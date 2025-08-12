#!/bin/bash

# CMMS ELK Stack - Monitoring Script
# This script monitors the health and status of ELK stack components

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Configuration
ELASTICSEARCH_URL="http://localhost:9200"
KIBANA_URL="http://localhost:5601"
LOGSTASH_URL="http://localhost:9600"
KAFKA_BOOTSTRAP="localhost:9092"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  CMMS ELK Stack - Monitoring${NC}"
echo -e "${BLUE}================================${NC}"

# Function to check service status
check_service() {
    local service_name=$1
    local service_url=$2
    local endpoint=$3
    
    echo -e "${YELLOW}Checking ${service_name}...${NC}"
    
    if curl -s -f "${service_url}${endpoint}" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ ${service_name} is healthy${NC}"
        return 0
    else
        echo -e "${RED}✗ ${service_name} is not responding${NC}"
        return 1
    fi
}

# Function to get Elasticsearch cluster health
get_elasticsearch_health() {
    echo -e "${YELLOW}Elasticsearch Cluster Health:${NC}"
    
    response=$(curl -s "${ELASTICSEARCH_URL}/_cluster/health?pretty")
    
    if [ $? -eq 0 ]; then
        echo -e "${BLUE}$response${NC}"
    else
        echo -e "${RED}Failed to get cluster health${NC}"
    fi
}

# Function to get Elasticsearch indices
get_elasticsearch_indices() {
    echo -e "${YELLOW}Elasticsearch Indices:${NC}"
    
    response=$(curl -s "${ELASTICSEARCH_URL}/_cat/indices/cmms-audit-logs-*?v&h=index,docs.count,store.size,health")
    
    if [ -n "$response" ]; then
        echo -e "${BLUE}$response${NC}"
    else
        echo -e "${YELLOW}No cmms-audit-logs indices found${NC}"
    fi
}

# Function to get Logstash pipeline status
get_logstash_status() {
    echo -e "${YELLOW}Logstash Pipeline Status:${NC}"
    
    response=$(curl -s "${LOGSTASH_URL}/_node/pipeline?pretty")
    
    if [ $? -eq 0 ]; then
        # Extract pipeline info
        pipeline_info=$(echo "$response" | grep -A 10 -B 5 "pipeline" || echo "No pipeline info found")
        echo -e "${BLUE}$pipeline_info${NC}"
    else
        echo -e "${RED}Failed to get Logstash status${NC}"
    fi
}

# Function to get Kibana status
get_kibana_status() {
    echo -e "${YELLOW}Kibana Status:${NC}"
    
    response=$(curl -s "${KIBANA_URL}/api/status?pretty")
    
    if [ $? -eq 0 ]; then
        # Extract status info
        status_info=$(echo "$response" | grep -A 5 -B 5 "status" || echo "No status info found")
        echo -e "${BLUE}$status_info${NC}"
    else
        echo -e "${RED}Failed to get Kibana status${NC}"
    fi
}

# Function to check Kafka connectivity
check_kafka() {
    echo -e "${YELLOW}Checking Kafka connectivity...${NC}"
    
    if command -v kafka-topics >/dev/null 2>&1; then
        if kafka-topics --bootstrap-server $KAFKA_BOOTSTRAP --list | grep -q "cmms-audit-logs"; then
            echo -e "${GREEN}✓ Kafka topic 'cmms-audit-logs' exists${NC}"
            
            # Get topic details
            echo -e "${YELLOW}Kafka Topic Details:${NC}"
            kafka-topics --bootstrap-server $KAFKA_BOOTSTRAP --describe --topic cmms-audit-logs
        else
            echo -e "${YELLOW}⚠ Kafka topic 'cmms-audit-logs' does not exist${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ kafka-topics command not available${NC}"
    fi
}

# Function to get recent audit logs
get_recent_logs() {
    echo -e "${YELLOW}Recent Audit Logs (last 10):${NC}"
    
    response=$(curl -s "${ELASTICSEARCH_URL}/cmms-audit-logs-*/_search?pretty&size=10&sort=@timestamp:desc")
    
    if [ $? -eq 0 ]; then
        # Extract log entries
        logs=$(echo "$response" | grep -A 20 -B 5 "hits" || echo "No logs found")
        echo -e "${BLUE}$logs${NC}"
    else
        echo -e "${RED}Failed to get recent logs${NC}"
    fi
}

# Function to get system resources
get_system_resources() {
    echo -e "${YELLOW}System Resources:${NC}"
    
    # Get Docker container stats
    echo -e "${BLUE}Docker Container Status:${NC}"
    docker ps --filter "name=cmms-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    echo -e "${BLUE}Memory Usage:${NC}"
    docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"
}

# Function to display summary
display_summary() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Monitoring Summary${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${GREEN}Elasticsearch:${NC} http://localhost:9200"
    echo -e "${GREEN}Kibana:${NC} http://localhost:5601"
    echo -e "${GREEN}Logstash API:${NC} http://localhost:9600"
    echo -e "${GREEN}Kafka:${NC} localhost:9092"
    echo -e "${GREEN}Main Dashboard:${NC} http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview"
}

# Main monitoring function
main() {
    local all_healthy=true
    
    # Check service health
    if ! check_service "Elasticsearch" "$ELASTICSEARCH_URL" "/_cluster/health"; then
        all_healthy=false
    fi
    
    if ! check_service "Kibana" "$KIBANA_URL" "/api/status"; then
        all_healthy=false
    fi
    
    if ! check_service "Logstash" "$LOGSTASH_URL" ""; then
        all_healthy=false
    fi
    
    # Get detailed information
    echo ""
    get_elasticsearch_health
    echo ""
    get_elasticsearch_indices
    echo ""
    get_logstash_status
    echo ""
    get_kibana_status
    echo ""
    check_kafka
    echo ""
    get_recent_logs
    echo ""
    get_system_resources
    echo ""
    display_summary
    
    # Final status
    echo -e "${BLUE}================================${NC}"
    if [ "$all_healthy" = true ]; then
        echo -e "${GREEN}  All ELK Services are Healthy! ✓${NC}"
    else
        echo -e "${RED}  Some ELK Services are Unhealthy! ✗${NC}"
    fi
    echo -e "${BLUE}================================${NC}"
}

# Run main function
main "$@" 