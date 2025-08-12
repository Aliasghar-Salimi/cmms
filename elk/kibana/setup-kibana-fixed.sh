#!/bin/bash

# CMMS ELK Stack - Kibana Setup Script (Fixed Version)
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

# Function to create index pattern directly
create_index_pattern() {
    echo -e "${YELLOW}Creating index pattern for CMMS audit logs...${NC}"
    
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
        echo -e "${YELLOW}⚠ Index pattern may already exist or failed to create (HTTP ${http_code})${NC}"
        echo -e "${YELLOW}Response: ${response_body}${NC}"
        return 0  # Continue anyway
    fi
}

# Function to set default index pattern
set_default_index_pattern() {
    echo -e "${YELLOW}Setting default index pattern...${NC}"
    
    # Set default index pattern
    response=$(curl -s -X POST \
        "${KIBANA_URL}/api/kibana/settings" \
        -H "kbn-xsrf: true" \
        -H "Content-Type: application/json" \
        -d '{"changes":{"defaultIndex":"cmms-audit-logs-*"}}')
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Set default index pattern${NC}"
    else
        echo -e "${YELLOW}⚠ Could not set default index pattern${NC}"
    fi
}

# Function to provide manual import instructions
provide_manual_instructions() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Manual Import Instructions${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${YELLOW}Since automated import has issues, please import manually:${NC}"
    echo ""
    echo -e "${GREEN}1.${NC} Open Kibana: ${KIBANA_URL}"
    echo -e "${GREEN}2.${NC} Go to Stack Management → Saved Objects"
    echo -e "${GREEN}3.${NC} Click 'Import' button"
    echo -e "${GREEN}4.${NC} Import these files in order:"
    echo "   - elk/kibana/dashboards/visualizations/audit-logs-timeline.json"
    echo "   - elk/kibana/dashboards/visualizations/total-audit-logs.json"
    echo "   - elk/kibana/dashboards/visualizations/logs-by-action.json"
    echo "   - elk/kibana/dashboards/visualizations/logs-by-user.json"
    echo "   - elk/kibana/dashboards/visualizations/logs-by-entity.json"
    echo "   - elk/kibana/dashboards/visualizations/recent-audit-logs-table.json"
    echo "   - elk/kibana/dashboards/visualizations/logs-by-hour.json"
    echo "   - elk/kibana/dashboards/visualizations/error-logs.json"
    echo "   - elk/kibana/dashboards/visualizations/access-denied-logs.json"
    echo "   - elk/kibana/dashboards/visualizations/top-ip-addresses.json"
    echo "   - elk/kibana/dashboards/audit-logs-overview.json"
    echo ""
    echo -e "${GREEN}5.${NC} After import, access the dashboard at:"
    echo "   ${KIBANA_URL}/app/dashboards#/view/cmms-audit-logs-overview"
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
    
    # Set default index pattern
    set_default_index_pattern
    
    # Provide manual import instructions
    provide_manual_instructions
    
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}  Kibana Setup Complete!${NC}"
    echo -e "${GREEN}================================${NC}"
    echo -e "${BLUE}Access Kibana at: ${KIBANA_URL}${NC}"
    echo -e "${YELLOW}Please import the dashboard files manually as shown above${NC}"
}

# Run main function
main "$@"
