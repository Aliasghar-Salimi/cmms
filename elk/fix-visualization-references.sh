#!/bin/bash

# Fix visualization references to point to the correct saved search
# This script updates the savedSearchRefName in visualization files

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

VISUALIZATIONS_DIR="kibana/dashboards/visualizations"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  Fixing Visualization References${NC}"
echo -e "${BLUE}================================${NC}"

# Function to fix visualization file
fix_visualization_file() {
    local file=$1
    local temp_file="${file}.tmp"
    
    echo -e "${YELLOW}Fixing ${file}...${NC}"
    
    # Replace "search_0" with "audit-logs-search" in the savedSearchRefName
    sed 's/"savedSearchRefName": "search_0"/"savedSearchRefName": "audit-logs-search"/g' "$file" > "$temp_file"
    
    # Replace the original file
    mv "$temp_file" "$file"
    
    echo -e "${GREEN}✓ Fixed ${file}${NC}"
}

# Fix all visualization files
for file in "${VISUALIZATIONS_DIR}"/*.json; do
    if [ -f "$file" ]; then
        fix_visualization_file "$file"
    fi
done

echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}  All visualization references fixed!${NC}"
echo -e "${GREEN}================================${NC}" 