#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Running IdentityService Unit Tests with Coverage...${NC}"

# Clean previous test results
echo "Cleaning previous test results..."
rm -rf ./TestResults
rm -rf ./coverage

# Restore packages
echo "Restoring packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests with coverage
echo "Running tests with coverage..."
dotnet test --no-build --verbosity normal --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults \
    --logger "console;verbosity=normal"

# Check if tests passed
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ All tests passed!${NC}"
else
    echo -e "${RED}✗ Some tests failed!${NC}"
    exit 1
fi

# Generate coverage report
echo "Generating coverage report..."
dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.2.4

reportgenerator \
    -reports:./TestResults/**/coverage.opencover.xml \
    -targetdir:./TestResults/CoverageReport \
    -reporttypes:Html;Cobertura;TextSummary

# Display coverage summary
echo -e "${YELLOW}Coverage Report Generated:${NC}"
echo "HTML Report: ./TestResults/CoverageReport/index.html"
echo "Cobertura Report: ./TestResults/CoverageReport/Cobertura.xml"

# Open coverage report in browser (if available)
if command -v xdg-open &> /dev/null; then
    xdg-open ./TestResults/CoverageReport/index.html
elif command -v open &> /dev/null; then
    open ./TestResults/CoverageReport/index.html
fi

echo -e "${GREEN}Test execution completed successfully!${NC}" 