#!/bin/bash

# CMMS ELK Stack - Test Script
# This script tests the ELK stack setup and data flow

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
KAFKA_BOOTSTRAP="localhost:9092"
TOPIC="cmms-audit-logs"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}  CMMS ELK Stack - Test Script${NC}"
echo -e "${BLUE}================================${NC}"

# Function to test service connectivity
test_service() {
    local service_name=$1
    local service_url=$2
    local endpoint=$3
    
    echo -e "${YELLOW}Testing ${service_name}...${NC}"
    
    if curl -s -f "${service_url}${endpoint}" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ ${service_name} is accessible${NC}"
        return 0
    else
        echo -e "${RED}✗ ${service_name} is not accessible${NC}"
        return 1
    fi
}

# Function to test Kafka connectivity
test_kafka() {
    echo -e "${YELLOW}Testing Kafka connectivity...${NC}"
    
    # Check if kafka-topics command is available
    if command -v kafka-topics >/dev/null 2>&1; then
        if kafka-topics --bootstrap-server $KAFKA_BOOTSTRAP --list | grep -q $TOPIC; then
            echo -e "${GREEN}✓ Kafka topic '${TOPIC}' exists${NC}"
            return 0
        else
            echo -e "${YELLOW}⚠ Kafka topic '${TOPIC}' does not exist${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠ kafka-topics command not available, skipping Kafka test${NC}"
        return 0
    fi
}

# Function to send test audit log
send_test_log() {
    echo -e "${YELLOW}Sending test audit log to Kafka...${NC}"
    
    # Create test audit log JSON
    test_log='{
        "id": "test-'$(date +%s)'",
        "timestamp": "'$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)'",
        "user_id": "test-user-123",
        "user_name": "Test User",
        "action": "Test Action",
        "entity_name": "Test Entity",
        "entity_id": "test-entity-456",
        "ip_address": "192.168.1.100",
        "data_before": "{\"old_value\": \"test\"}",
        "data_after": "{\"new_value\": \"updated\"}",
        "correlation_id": "test-correlation-789",
        "meta_data": "{\"test\": true}",
        "service": "cmms",
        "log_level": "INFO"
    }'
    
    # Send to Kafka using kafka-console-producer if available
    if command -v kafka-console-producer >/dev/null 2>&1; then
        echo "$test_log" | kafka-console-producer --bootstrap-server $KAFKA_BOOTSTRAP --topic $TOPIC
        echo -e "${GREEN}✓ Test log sent to Kafka${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠ kafka-console-producer not available, skipping test log${NC}"
        return 0
    fi
}

# Function to check for test log in Elasticsearch
check_test_log() {
    echo -e "${YELLOW}Checking for test log in Elasticsearch...${NC}"
    
    # Wait a bit for processing
    sleep 5
    
    # Search for test log
    response=$(curl -s "${ELASTICSEARCH_URL}/cmms-audit-logs-*/_search?q=user_name:Test%20User&size=1")
    
    if echo "$response" | grep -q "Test User"; then
        echo -e "${GREEN}✓ Test log found in Elasticsearch${NC}"
        return 0
    else
        echo -e "${RED}✗ Test log not found in Elasticsearch${NC}"
        echo -e "${YELLOW}Response: $response${NC}"
        return 1
    fi
}

# Function to test Kibana dashboard
test_kibana_dashboard() {
    echo -e "${YELLOW}Testing Kibana dashboard...${NC}"
    
    # Check if dashboard exists
    response=$(curl -s "${KIBANA_URL}/api/saved_objects/_find?type=dashboard&search_fields=title&search=cmms-audit-logs-overview")
    
    if echo "$response" | grep -q "cmms-audit-logs-overview"; then
        echo -e "${GREEN}✓ Kibana dashboard exists${NC}"
        return 0
    else
        echo -e "${RED}✗ Kibana dashboard not found${NC}"
        return 1
    fi
}

# Function to display index statistics
show_index_stats() {
    echo -e "${YELLOW}Elasticsearch Index Statistics:${NC}"
    
    # Get index stats
    stats=$(curl -s "${ELASTICSEARCH_URL}/_cat/indices/cmms-audit-logs-*?v&h=index,docs.count,store.size")
    
    if [ -n "$stats" ]; then
        echo -e "${BLUE}$stats${NC}"
    else
        echo -e "${YELLOW}No cmms-audit-logs indices found${NC}"
    fi
}

# Main test execution
main() {
    local all_tests_passed=true
    
    echo -e "${BLUE}Starting ELK Stack Tests...${NC}"
    
    # Test service connectivity
    if ! test_service "Elasticsearch" "$ELASTICSEARCH_URL" "/_cluster/health"; then
        all_tests_passed=false
    fi
    
    if ! test_service "Kibana" "$KIBANA_URL" "/api/status"; then
        all_tests_passed=false
    fi
    
    # Test Kafka
    if ! test_kafka; then
        all_tests_passed=false
    fi
    
    # Send test log if services are up
    if [ "$all_tests_passed" = true ]; then
        send_test_log
        
        # Check if test log was processed
        if check_test_log; then
            echo -e "${GREEN}✓ Data flow test passed${NC}"
        else
            echo -e "${RED}✗ Data flow test failed${NC}"
            all_tests_passed=false
        fi
        
        # Test Kibana dashboard
        if test_kibana_dashboard; then
            echo -e "${GREEN}✓ Kibana dashboard test passed${NC}"
        else
            echo -e "${RED}✗ Kibana dashboard test failed${NC}"
            all_tests_passed=false
        fi
    fi
    
    # Show statistics
    show_index_stats
    
    # Final result
    echo -e "${BLUE}================================${NC}"
    if [ "$all_tests_passed" = true ]; then
        echo -e "${GREEN}  All Tests Passed! ✓${NC}"
        echo -e "${GREEN}  ELK Stack is working correctly${NC}"
    else
        echo -e "${RED}  Some Tests Failed! ✗${NC}"
        echo -e "${YELLOW}  Check the output above for details${NC}"
    fi
    echo -e "${BLUE}================================${NC}"
    
    # Return appropriate exit code
    if [ "$all_tests_passed" = true ]; then
        exit 0
    else
        exit 1
    fi
}

# Run main function
main "$@" 