#!/bin/bash

# CMMS ELK Stack - Kibana Setup Script (NDJSON Version)
# This script sets up Kibana with pre-configured dashboards for CMMS audit logs
# Uses NDJSON format for importing saved objects

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
IMPORT_DIR="kibana/dashboards/import-ready"

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

# Function to import Kibana objects from NDJSON
import_kibana_objects_ndjson() {
    local file_path=$1
    local object_type=$2
    
    echo -e "${YELLOW}Importing ${object_type} from ${file_path}...${NC}"
    
    if [ ! -f "$file_path" ]; then
        echo -e "${RED}✗ File not found: ${file_path}${NC}"
        return 1
    fi
    
    # Import using Kibana API with NDJSON format
    response=$(curl -s -w "%{http_code}" -X POST \
        "${KIBANA_URL}/api/saved_objects/_import?overwrite=true" \
        -H "kbn-xsrf: true" \
        -H "Content-Type: application/x-ndjson" \
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

# Function to convert JSON files to NDJSON if needed
convert_to_ndjson() {
    echo -e "${YELLOW}Converting JSON files to NDJSON format...${NC}"
    
    # Check if convert script exists and run it
    if [ -f "../convert-to-ndjson.sh" ]; then
        echo -e "${YELLOW}Running conversion script...${NC}"
        cd ..
        ./convert-to-ndjson.sh
        cd kibana
    else
        echo -e "${RED}✗ Conversion script not found${NC}"
        return 1
    fi
}

# Function to import all NDJSON files
import_all_ndjson_files() {
    echo -e "${BLUE}Importing all NDJSON files...${NC}"
    
    # Import index pattern first
    if [ -f "${IMPORT_DIR}/cmms-audit-logs-index-pattern.ndjson" ]; then
        import_kibana_objects_ndjson "${IMPORT_DIR}/cmms-audit-logs-index-pattern.ndjson" "index pattern"
    fi
    
    # Import visualizations
    for file in "${IMPORT_DIR}"/*.ndjson; do
        if [ -f "$file" ] && [[ "$file" != *"index-pattern"* ]] && [[ "$file" != *"overview"* ]]; then
            filename=$(basename "$file" .ndjson)
            import_kibana_objects_ndjson "$file" "$filename visualization"
        fi
    done
    
    # Import dashboard last
    if [ -f "${IMPORT_DIR}/audit-logs-overview.ndjson" ]; then
        import_kibana_objects_ndjson "${IMPORT_DIR}/audit-logs-overview.ndjson" "main dashboard"
    fi
}

# Function to provide manual import instructions as fallback
provide_manual_instructions() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Manual Import Instructions${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${YELLOW}If automated import fails, please import manually:${NC}"
    echo ""
    echo -e "${GREEN}1.${NC} Open Kibana: ${KIBANA_URL}"
    echo -e "${GREEN}2.${NC} Go to Stack Management → Saved Objects"
    echo -e "${GREEN}3.${NC} Click 'Import' button"
    echo -e "${GREEN}4.${NC} Import these NDJSON files in order:"
    echo ""
    
    # List all NDJSON files
    for file in "${IMPORT_DIR}"/*.ndjson; do
        if [ -f "$file" ]; then
            filename=$(basename "$file")
            echo "   - ${IMPORT_DIR}/${filename}"
        fi
    done
    
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
    
    # Convert JSON files to NDJSON
    convert_to_ndjson
    
    # Create index pattern
    create_index_pattern
    
    # Set default index pattern
    set_default_index_pattern
    
    # Import all NDJSON files
    import_all_ndjson_files
    
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}  Kibana Setup Complete!${NC}"
    echo -e "${GREEN}================================${NC}"
    echo -e "${BLUE}Access Kibana at: ${KIBANA_URL}${NC}"
    echo -e "${BLUE}Main Dashboard: ${KIBANA_URL}/app/dashboards#/view/cmms-audit-logs-overview${NC}"
    echo -e "${YELLOW}Note: It may take a few minutes for all visualizations to load properly${NC}"
    
    # Provide manual instructions as backup
    echo ""
    provide_manual_instructions
}

# Run main function
main "$@" 