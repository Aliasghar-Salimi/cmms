#!/bin/bash

# Update Kibana version from 8.11.0 to 8.5.0 in all JSON files
# This script updates version numbers to match your Kibana installation

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  Updating Kibana Version to 8.5.0${NC}"
echo -e "${BLUE}================================${NC}"

# Function to update version in a file
update_version_in_file() {
    local file=$1
    local temp_file="${file}.tmp"
    
    echo -e "${YELLOW}Updating ${file}...${NC}"
    
    # Replace all occurrences of 8.11.0 with 8.5.0
    sed 's/8\.11\.0/8.5.0/g' "$file" > "$temp_file"
    
    # Replace the original file
    mv "$temp_file" "$file"
    
    echo -e "${GREEN}✓ Updated ${file}${NC}"
}

# Update all JSON files in the dashboards directory
echo -e "${BLUE}Updating dashboard files...${NC}"

# Update index patterns
for file in kibana/dashboards/index-patterns/*.json; do
    if [ -f "$file" ]; then
        update_version_in_file "$file"
    fi
done

# Update searches
for file in kibana/dashboards/searches/*.json; do
    if [ -f "$file" ]; then
        update_version_in_file "$file"
    fi
done

# Update visualizations
for file in kibana/dashboards/visualizations/*.json; do
    if [ -f "$file" ]; then
        update_version_in_file "$file"
    fi
done

# Update dashboards
for file in kibana/dashboards/*.json; do
    if [ -f "$file" ]; then
        update_version_in_file "$file"
    fi
done

echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}  Version update complete!${NC}"
echo -e "${GREEN}================================${NC}"
echo -e "${BLUE}All files updated from 8.11.0 to 8.5.0${NC}"
echo -e "${YELLOW}Next step: Run ./convert-to-ndjson.sh to regenerate NDJSON files${NC}" 