#!/bin/bash

# Test script to verify NDJSON import functionality
# This script tests importing a single NDJSON file to verify the format is correct

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
KIBANA_URL="http://localhost:5601"
TEST_FILE="kibana/dashboards/import-ready/cmms-audit-logs-index-pattern.ndjson"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  Testing NDJSON Import${NC}"
echo -e "${BLUE}================================${NC}"

# Function to wait for Kibana
wait_for_kibana() {
    echo -e "${YELLOW}Waiting for Kibana to be ready...${NC}"
    
    local timeout=60
    local elapsed=0
    
    while [ $elapsed -lt $timeout ]; do
        if curl -s -f "${KIBANA_URL}/api/status" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ Kibana is ready!${NC}"
            return 0
        fi
        
        echo -e "${YELLOW}  Kibana not ready yet, waiting 5s... (${elapsed}/${timeout}s)${NC}"
        sleep 5
        elapsed=$((elapsed + 5))
    done
    
    echo -e "${RED}✗ Timeout waiting for Kibana${NC}"
    return 1
}

# Function to test import
test_import() {
    echo -e "${YELLOW}Testing import of ${TEST_FILE}...${NC}"
    
    if [ ! -f "$TEST_FILE" ]; then
        echo -e "${RED}✗ Test file not found: ${TEST_FILE}${NC}"
        return 1
    fi
    
    # Test import using Kibana API
    response=$(curl -s -w "%{http_code}" -X POST \
        "${KIBANA_URL}/api/saved_objects/_import?overwrite=true" \
        -H "kbn-xsrf: true" \
        -H "Content-Type: application/x-ndjson" \
        --data-binary "@${TEST_FILE}")
    
    http_code="${response: -3}"
    response_body="${response%???}"
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}✓ Successfully imported test file (HTTP ${http_code})${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed to import test file (HTTP ${http_code})${NC}"
        echo -e "${RED}Response: ${response_body}${NC}"
        return 1
    fi
}

# Function to show file content
show_file_info() {
    echo -e "${YELLOW}Test file information:${NC}"
    echo -e "${GREEN}File:${NC} ${TEST_FILE}"
    echo -e "${GREEN}Size:${NC} $(wc -c < "$TEST_FILE") bytes"
    echo -e "${GREEN}Lines:${NC} $(wc -l < "$TEST_FILE")"
    echo ""
    
    echo -e "${YELLOW}First line preview:${NC}"
    head -1 "$TEST_FILE" | jq '.' 2>/dev/null || head -1 "$TEST_FILE"
    echo ""
}

# Main execution
main() {
    # Check if test file exists
    if [ ! -f "$TEST_FILE" ]; then
        echo -e "${RED}✗ Test file not found. Please run convert-to-ndjson.sh first.${NC}"
        exit 1
    fi
    
    # Show file information
    show_file_info
    
    # Wait for Kibana
    if ! wait_for_kibana; then
        echo -e "${RED}✗ Kibana is not available. Please start the ELK stack first.${NC}"
        exit 1
    fi
    
    # Test import
    if test_import; then
        echo -e "${GREEN}================================${NC}"
        echo -e "${GREEN}  Test Passed! ✓${NC}"
        echo -e "${GREEN}================================${NC}"
        echo -e "${BLUE}The NDJSON format is correct and can be imported.${NC}"
        echo -e "${BLUE}You can now run the full setup with: ./elk/setup-elk-stack.sh${NC}"
    else
        echo -e "${RED}================================${NC}"
        echo -e "${RED}  Test Failed! ✗${NC}"
        echo -e "${RED}================================${NC}"
        echo -e "${YELLOW}The NDJSON format may have issues.${NC}"
        echo -e "${YELLOW}Please check the error message above.${NC}"
        exit 1
    fi
}

# Run main function
main "$@" 