# Kibana Import Guide - Fixed Version

## Problem Solved

The original issues have been fixed:
1. ✅ **HTTP 415 Error** - Fixed by using proper NDJSON format
2. ✅ **"Could not find saved search reference 'search_0'"** - Fixed by creating the missing saved search and updating references

## What Was Fixed

### 1. NDJSON Format
- Fixed the conversion script to properly format NDJSON files
- Each object is now on a separate line as required by Kibana

### 2. Missing Saved Search
- Created `audit-logs-search.json` - the base search that visualizations reference
- Updated all visualizations to reference `audit-logs-search` instead of `search_0`

### 3. Proper Import Order
- Index pattern must be imported first
- Saved search must be imported before visualizations
- Dashboard must be imported last

## Import Instructions

### Step 1: Prepare Files
The NDJSON files are ready in: `elk/kibana/dashboards/import-ready/`

### Step 2: Import in Kibana UI

1. **Open Kibana**: http://localhost:5601
2. **Navigate to**: Stack Management → Saved Objects
3. **Click**: "Import" button
4. **Import files in this exact order**:

#### Import Order (CRITICAL):
1. `cmms-audit-logs-index-pattern.ndjson` - Index pattern (required first)
2. `audit-logs-search.ndjson` - Saved search (required before visualizations)
3. All visualization files (any order):
   - `access-denied-logs.ndjson`
   - `audit-logs-timeline.ndjson`
   - `error-logs.ndjson`
   - `logs-by-action.ndjson`
   - `logs-by-entity.ndjson`
   - `logs-by-hour.ndjson`
   - `logs-by-user.ndjson`
   - `recent-audit-logs-table.ndjson`
   - `top-ip-addresses.ndjson`
   - `total-audit-logs.ndjson`
4. `audit-logs-overview.ndjson` - Dashboard (import last)

### Step 3: Verify Import

After successful import, you should be able to access:
- **Main Dashboard**: http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview
- **Discover**: http://localhost:5601/app/discover
- **Saved Objects**: http://localhost:5601/app/management/kibana/objects

## Automated Import

You can also use the automated script:
```bash
./elk/setup-elk-stack.sh
```

This will automatically import all files in the correct order.

## Troubleshooting

### If you still get "Could not find saved search reference":
1. Make sure you imported `audit-logs-search.ndjson` before the visualizations
2. Check that the import order was followed exactly
3. Try deleting all saved objects and re-importing in the correct order

### If you get import errors:
1. Check that Kibana is running: `curl http://localhost:5601/api/status`
2. Verify the NDJSON files exist: `ls -la elk/kibana/dashboards/import-ready/`
3. Check file permissions and format

### If visualizations don't load:
1. Make sure the index pattern `cmms-audit-logs-*` exists
2. Check that there's data in the Elasticsearch index
3. Verify the time range in the visualizations

## File Structure

```
elk/kibana/dashboards/import-ready/
├── cmms-audit-logs-index-pattern.ndjson  # Index pattern
├── audit-logs-search.ndjson              # Saved search
├── audit-logs-timeline.ndjson            # Timeline visualization
├── total-audit-logs.ndjson               # Total logs visualization
├── logs-by-action.ndjson                 # Logs by action visualization
├── logs-by-user.ndjson                   # Logs by user visualization
├── logs-by-entity.ndjson                 # Logs by entity visualization
├── recent-audit-logs-table.ndjson        # Recent logs table
├── logs-by-hour.ndjson                   # Logs by hour visualization
├── error-logs.ndjson                     # Error logs visualization
├── access-denied-logs.ndjson             # Access denied logs visualization
├── top-ip-addresses.ndjson               # Top IP addresses visualization
└── audit-logs-overview.ndjson            # Main dashboard
```

## Success Indicators

After successful import, you should see:
- ✅ No error messages in Kibana
- ✅ All visualizations load without "Could not find saved search reference" errors
- ✅ Dashboard displays all visualizations correctly
- ✅ Index pattern is available in Discover
- ✅ Saved search is available in Saved Objects

## Next Steps

Once imported successfully:
1. Start your CMMS microservices to generate audit logs
2. Data will automatically appear in the dashboard
3. Use Discover to search and explore the logs
4. Customize visualizations as needed

The dashboard will show real-time audit logs from your CMMS system once data starts flowing through the ELK stack. 