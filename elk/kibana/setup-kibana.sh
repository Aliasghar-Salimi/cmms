#!/bin/bash

# CMMS ELK Stack - Kibana Setup Script
# This script sets up Kibana with pre-configured dashboards for CMMS audit logs

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
KIBANA_URL="http://localhost:5601"
ELASTICSEARCH_URL="http://localhost:9200"
WAIT_TIMEOUT=300
RETRY_INTERVAL=10

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  CMMS ELK Stack - Kibana Setup${NC}"
echo -e "${BLUE}================================${NC}"

# Function to wait for service to be ready
wait_for_service() {
    local service_name=$1
    local service_url=$2
    local timeout=$3
    
    echo -e "${YELLOW}Waiting for ${service_name} to be ready...${NC}"
    
    local elapsed=0
    while [ $elapsed -lt $timeout ]; do
        if curl -s -f "$service_url" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ ${service_name} is ready!${NC}"
            return 0
        fi
        
        echo -e "${YELLOW}  ${service_name} not ready yet, waiting ${RETRY_INTERVAL}s... (${elapsed}/${timeout}s)${NC}"
        sleep $RETRY_INTERVAL
        elapsed=$((elapsed + RETRY_INTERVAL))
    done
    
    echo -e "${RED}✗ Timeout waiting for ${service_name} to be ready${NC}"
    return 1
}

# Function to import Kibana objects
import_kibana_objects() {
    local file_path=$1
    local object_type=$2
    
    echo -e "${YELLOW}Importing ${object_type} from ${file_path}...${NC}"
    
    if [ ! -f "$file_path" ]; then
        echo -e "${RED}✗ File not found: ${file_path}${NC}"
        return 1
    fi
    
    # Import using Kibana API
    response=$(curl -s -w "%{http_code}" -X POST \
        "${KIBANA_URL}/api/saved_objects/_import?overwrite=true" \
        -H "kbn-xsrf: true" \
        -H "Content-Type: application/json" \
        --data-binary "@${file_path}")
    
    http_code="${response: -3}"
    response_body="${response%???}"
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}✓ Successfully imported ${object_type}${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed to import ${object_type} (HTTP ${http_code})${NC}"
        echo -e "${RED}Response: ${response_body}${NC}"
        return 1
    fi
}

# Function to create index pattern
create_index_pattern() {
    echo -e "${YELLOW}Creating index pattern for CMMS audit logs...${NC}"
    
    # Check if index pattern already exists
    if curl -s -f "${ELASTICSEARCH_URL}/_cat/indices/cmms-audit-logs-*" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Index pattern already exists${NC}"
        return 0
    fi
    
    # Create index pattern using Kibana API
    response=$(curl -s -w "%{http_code}" -X POST \
        "${KIBANA_URL}/api/index_patterns/index_pattern" \
        -H "kbn-xsrf: true" \
        -H "Content-Type: application/json" \
        -d '{
            "index_pattern": {
                "title": "cmms-audit-logs-*",
                "timeFieldName": "@timestamp"
            }
        }')
    
    http_code="${response: -3}"
    response_body="${response%???}"
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}✓ Successfully created index pattern${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed to create index pattern (HTTP ${http_code})${NC}"
        echo -e "${RED}Response: ${response_body}${NC}"
        return 1
    fi
}

# Function to set default index pattern
set_default_index_pattern() {
    echo -e "${YELLOW}Setting default index pattern...${NC}"
    
    # Get the index pattern ID
    response=$(curl -s "${KIBANA_URL}/api/saved_objects/_find?type=index-pattern&search_fields=title&search=cmms-audit-logs-*")
    
    if echo "$response" | grep -q "cmms-audit-logs-\\*"; then
        pattern_id=$(echo "$response" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
        
        # Set as default
        curl -s -X POST \
            "${KIBANA_URL}/api/kibana/settings" \
            -H "kbn-xsrf: true" \
            -H "Content-Type: application/json" \
            -d "{\"changes\":{\"defaultIndex\":\"${pattern_id}\"}}" > /dev/null
        
        echo -e "${GREEN}✓ Set default index pattern to ${pattern_id}${NC}"
    else
        echo -e "${YELLOW}⚠ Could not find index pattern to set as default${NC}"
    fi
}

# Main execution
main() {
    # Wait for Elasticsearch
    if ! wait_for_service "Elasticsearch" "${ELASTICSEARCH_URL}" $WAIT_TIMEOUT; then
        exit 1
    fi
    
    # Wait for Kibana
    if ! wait_for_service "Kibana" "${KIBANA_URL}/api/status" $WAIT_TIMEOUT; then
        exit 1
    fi
    
    echo -e "${GREEN}All services are ready! Starting Kibana setup...${NC}"
    
    # Create index pattern
    create_index_pattern
    
    # Import index pattern
    import_kibana_objects "kibana/dashboards/index-patterns/cmms-audit-logs-index-pattern.json" "index pattern"
    
    # Import visualizations
    echo -e "${BLUE}Importing visualizations...${NC}"
    import_kibana_objects "kibana/dashboards/visualizations/audit-logs-timeline.json" "timeline visualization"
    import_kibana_objects "kibana/dashboards/visualizations/total-audit-logs.json" "total logs visualization"
    import_kibana_objects "kibana/dashboards/visualizations/logs-by-action.json" "logs by action visualization"
    import_kibana_objects "kibana/dashboards/visualizations/logs-by-user.json" "logs by user visualization"
    import_kibana_objects "kibana/dashboards/visualizations/logs-by-entity.json" "logs by entity visualization"
    import_kibana_objects "kibana/dashboards/visualizations/recent-audit-logs-table.json" "recent logs table"
    import_kibana_objects "kibana/dashboards/visualizations/logs-by-hour.json" "logs by hour visualization"
    import_kibana_objects "kibana/dashboards/visualizations/error-logs.json" "error logs visualization"
    import_kibana_objects "kibana/dashboards/visualizations/access-denied-logs.json" "access denied logs visualization"
    import_kibana_objects "kibana/dashboards/visualizations/top-ip-addresses.json" "top IP addresses visualization"
    
    # Import main dashboard
    echo -e "${BLUE}Importing main dashboard...${NC}"
    import_kibana_objects "kibana/dashboards/audit-logs-overview.json" "main dashboard"
    
    # Set default index pattern
    set_default_index_pattern
    
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}  Kibana Setup Complete!${NC}"
    echo -e "${GREEN}================================${NC}"
    echo -e "${BLUE}Access Kibana at: ${KIBANA_URL}${NC}"
    echo -e "${BLUE}Main Dashboard: ${KIBANA_URL}/app/dashboards#/view/cmms-audit-logs-overview${NC}"
    echo -e "${YELLOW}Note: It may take a few minutes for all visualizations to load properly${NC}"
}

# Run main function
main "$@" 