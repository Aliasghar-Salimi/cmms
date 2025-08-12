# Kibana Import Fix

## Problem Description

The original Kibana setup was failing with the following errors:

1. **HTTP 415 Unsupported Media Type** when trying to import JSON files
2. **"Bad Request: Invalid file extension .json"** in the Kibana UI

## Root Cause

Kibana expects saved objects to be imported in **NDJSON format** (Newline Delimited JSON), not regular JSON format. The original scripts were trying to import JSON files directly, which caused the import to fail.

## Solution

### 1. Fixed Conversion Script

The `convert-to-ndjson.sh` script was updated to properly convert Kibana export JSON files to NDJSON format:

- **Before**: The script was not correctly extracting objects from the JSON structure
- **After**: The script now properly extracts each object from the `objects` array and writes them as separate lines in NDJSON format

### 2. New NDJSON Setup Script

Created `setup-kibana-ndjson.sh` that:

- Uses the converted NDJSON files for import
- Sets the correct `Content-Type: application/x-ndjson` header
- Provides better error handling and fallback instructions

### 3. Updated Main Setup Script

Modified `setup-elk-stack.sh` to prefer the NDJSON version of the setup script.

## Files Modified/Created

### Fixed Files
- `elk/convert-to-ndjson.sh` - Fixed JSON to NDJSON conversion
- `elk/setup-elk-stack.sh` - Updated to use NDJSON setup script

### New Files
- `elk/kibana/setup-kibana-ndjson.sh` - New setup script using NDJSON format
- `elk/test-ndjson-import.sh` - Test script to verify NDJSON import
- `elk/kibana/dashboards/import-ready/` - Directory containing converted NDJSON files

## How to Use

### Option 1: Automated Setup (Recommended)
```bash
# Run the complete setup with NDJSON support
./elk/setup-elk-stack.sh
```

### Option 2: Manual Conversion and Import
```bash
# 1. Convert JSON files to NDJSON
cd elk
./convert-to-ndjson.sh

# 2. Test the import (optional)
./test-ndjson-import.sh

# 3. Import manually in Kibana UI
# - Open Kibana: http://localhost:5601
# - Go to Stack Management → Saved Objects
# - Click 'Import' button
# - Import the .ndjson files from kibana/dashboards/import-ready/
```

### Option 3: Manual Import Order
If importing manually, use this order:
1. `cmms-audit-logs-index-pattern.ndjson`
2. All visualization files (any order)
3. `audit-logs-overview.ndjson` (dashboard)

## NDJSON Format

The NDJSON format looks like this:
```
{"id":"cmms-audit-logs-*","type":"index-pattern","attributes":{...}}
{"id":"audit-logs-timeline","type":"visualization","attributes":{...}}
{"id":"audit-logs-overview","type":"dashboard","attributes":{...}}
```

Each line is a separate JSON object, which is what Kibana expects for import.

## Verification

After successful import, you should be able to access:
- **Main Dashboard**: http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview
- **Discover**: http://localhost:5601/app/discover
- **Stack Management**: http://localhost:5601/app/management/kibana/objects

## Troubleshooting

### If import still fails:
1. Check that Kibana is running: `curl http://localhost:5601/api/status`
2. Verify NDJSON files exist: `ls -la elk/kibana/dashboards/import-ready/`
3. Test with a single file: `./elk/test-ndjson-import.sh`
4. Check Kibana logs for detailed error messages

### If you get permission errors:
```bash
chmod +x elk/convert-to-ndjson.sh
chmod +x elk/kibana/setup-kibana-ndjson.sh
chmod +x elk/test-ndjson-import.sh
```

### If jq is not installed:
```bash
# Ubuntu/Debian
sudo apt-get install jq

# macOS
brew install jq

# CentOS/RHEL
sudo yum install jq
```

## Technical Details

### API Endpoint
The correct API endpoint for importing NDJSON files:
```
POST /api/saved_objects/_import?overwrite=true
Content-Type: application/x-ndjson
```

### File Structure
Original JSON files contain:
```json
{
  "version": "8.11.0",
  "objects": [
    { "id": "...", "type": "...", "attributes": {...} },
    { "id": "...", "type": "...", "attributes": {...} }
  ]
}
```

Converted NDJSON files contain:
```
{"id":"...","type":"...","attributes":{...}}
{"id":"...","type":"...","attributes":{...}}
```

This fix ensures compatibility with Kibana's import API and resolves the HTTP 415 and file extension errors. 