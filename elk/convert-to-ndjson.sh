#!/bin/bash

# CMMS ELK Stack - Convert JSON to NDJSON for Kibana Import
# This script converts Kibana export JSON files to NDJSON format for import

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
VISUALIZATIONS_DIR="kibana/dashboards/visualizations"
INDEX_PATTERNS_DIR="kibana/dashboards/index-patterns"
SEARCHES_DIR="kibana/dashboards/searches"
DASHBOARDS_DIR="kibana/dashboards"
OUTPUT_DIR="kibana/dashboards/import-ready"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  Converting JSON to NDJSON${NC}"
echo -e "${BLUE}================================${NC}"

# Function to convert JSON to NDJSON
convert_json_to_ndjson() {
    local input_file=$1
    local output_file=$2
    
    echo -e "${YELLOW}Converting ${input_file} to ${output_file}...${NC}"
    
    if [ ! -f "$input_file" ]; then
        echo -e "${RED}✗ File not found: ${input_file}${NC}"
        return 1
    fi
    
    # Create output directory if it doesn't exist
    mkdir -p "$(dirname "$output_file")"
    
    # Extract objects from JSON and convert to NDJSON
    # Each object from the objects array becomes a separate line
    # Use jq to extract objects and format them properly
    jq -r '.objects[] | tojson' "$input_file" > "$output_file"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Successfully converted ${input_file}${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed to convert ${input_file}${NC}"
        return 1
    fi
}

# Function to create output directory
create_output_directory() {
    echo -e "${YELLOW}Creating output directory...${NC}"
    
    mkdir -p "$OUTPUT_DIR"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Created output directory: ${OUTPUT_DIR}${NC}"
    else
        echo -e "${RED}✗ Failed to create output directory${NC}"
        exit 1
    fi
}

# Function to convert all files
convert_all_files() {
    echo -e "${BLUE}Converting all files to NDJSON format...${NC}"
    
    # Convert index patterns
    if [ -f "${INDEX_PATTERNS_DIR}/cmms-audit-logs-index-pattern.json" ]; then
        convert_json_to_ndjson \
            "${INDEX_PATTERNS_DIR}/cmms-audit-logs-index-pattern.json" \
            "${OUTPUT_DIR}/cmms-audit-logs-index-pattern.ndjson"
    fi
    
    # Convert searches
    for file in "${SEARCHES_DIR}"/*.json; do
        if [ -f "$file" ]; then
            filename=$(basename "$file" .json)
            convert_json_to_ndjson "$file" "${OUTPUT_DIR}/${filename}.ndjson"
        fi
    done
    
    # Convert visualizations
    for file in "${VISUALIZATIONS_DIR}"/*.json; do
        if [ -f "$file" ]; then
            filename=$(basename "$file" .json)
            convert_json_to_ndjson "$file" "${OUTPUT_DIR}/${filename}.ndjson"
        fi
    done
    
    # Convert dashboard
    if [ -f "${DASHBOARDS_DIR}/audit-logs-overview.json" ]; then
        convert_json_to_ndjson \
            "${DASHBOARDS_DIR}/audit-logs-overview.json" \
            "${OUTPUT_DIR}/audit-logs-overview.ndjson"
    fi
}

# Function to provide import instructions
provide_import_instructions() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Import Instructions${NC}"
    echo -e "${BLUE}================================${NC}"
    echo -e "${GREEN}NDJSON files created in: ${OUTPUT_DIR}${NC}"
    echo ""
    echo -e "${YELLOW}To import in Kibana:${NC}"
    echo -e "${GREEN}1.${NC} Open Kibana: http://localhost:5601"
    echo -e "${GREEN}2.${NC} Go to Stack Management → Saved Objects"
    echo -e "${GREEN}3.${NC} Click 'Import' button"
    echo -e "${GREEN}4.${NC} Import these .ndjson files in order:"
    echo ""
    
    # List all NDJSON files
    for file in "${OUTPUT_DIR}"/*.ndjson; do
        if [ -f "$file" ]; then
            filename=$(basename "$file")
            echo "   - ${OUTPUT_DIR}/${filename}"
        fi
    done
    
    echo ""
    echo -e "${GREEN}5.${NC} After import, access the dashboard at:"
    echo "   http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview"
}

# Main execution
main() {
    # Check if jq is installed
    if ! command -v jq &> /dev/null; then
        echo -e "${RED}✗ jq is not installed. Please install jq first.${NC}"
        echo -e "${YELLOW}Install with: sudo apt-get install jq (Ubuntu/Debian)${NC}"
        echo -e "${YELLOW}Or: brew install jq (macOS)${NC}"
        exit 1
    fi
    
    # Create output directory
    create_output_directory
    
    # Convert all files
    convert_all_files
    
    # Provide import instructions
    provide_import_instructions
    
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}  Conversion Complete!${NC}"
    echo -e "${GREEN}================================${NC}"
}

# Run main function
main "$@"
