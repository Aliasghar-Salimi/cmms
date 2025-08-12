# CMMS ELK Stack Setup

This directory contains the configuration for the ELK (Elasticsearch, Logstash, Kibana) stack used to visualize audit logs from the CMMS microservices.

## Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Microservices │───▶│     Kafka       │───▶│    Logstash     │───▶│  Elasticsearch  │
│   (Audit Logs)  │    │                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
                                                                              │
                                                                              ▼
                                                                     ┌─────────────────┐
                                                                     │     Kibana      │
                                                                     │  (Visualization)│
                                                                     └─────────────────┘
```

## Components

### 1. Elasticsearch
- **Purpose**: Distributed search and analytics engine
- **Port**: 9200 (HTTP), 9300 (Transport)
- **Configuration**: Single-node setup for development
- **Data**: Stores audit logs with daily indices (`cmms-audit-logs-YYYY.MM.DD`)

### 2. Logstash
- **Purpose**: Data processing pipeline
- **Port**: 5044 (Beats), 9600 (API)
- **Configuration**: 
  - Input: Kafka topic `cmms-audit-logs`
  - Filters: JSON parsing, timestamp conversion, field mapping
  - Output: Elasticsearch with daily index rotation

### 3. Kibana
- **Purpose**: Data visualization and exploration
- **Port**: 5601
- **Features**: Pre-configured dashboards for real-time monitoring

## Quick Start

### 1. Start the ELK Stack

```bash
# Start all services including ELK
docker-compose up -d

# Or start only ELK services
docker-compose up -d elasticsearch logstash kibana
```

### 2. Wait for Services to be Ready

```bash
# Check Elasticsearch
curl http://localhost:9200/_cluster/health

# Check Kibana
curl http://localhost:5601/api/status
```

### 3. Setup Kibana Dashboards

```bash
# Navigate to the elk directory
cd elk

# Run the setup script
./kibana/setup-kibana.sh
```

### 4. Access Kibana

Open your browser and navigate to: http://localhost:5601

## Pre-configured Dashboards

### Main Dashboard: "CMMS Audit Logs - Real-time Overview"

This comprehensive dashboard includes:

#### Real-time Metrics
- **Audit Logs Timeline**: Real-time line chart showing log volume over time
- **Total Audit Logs**: Current count of all audit logs
- **Error Logs**: Count of error-related logs
- **Access Denied Logs**: Count of access denied events

#### Analytics Panels
- **Logs by Action**: Pie chart showing breakdown by action type (Create, Update, Delete, Login, etc.)
- **Logs by User**: Bar chart showing most active users
- **Logs by Entity**: Bar chart showing most accessed entities
- **Logs by Hour**: Hourly distribution of logs over the last 24 hours
- **Top IP Addresses**: Most frequent source IP addresses

#### Data Tables
- **Recent Audit Logs**: Detailed table of recent audit events with filtering capabilities

## Data Flow

### 1. Log Generation
Microservices generate audit logs and publish them to Kafka:

```csharp
// Example from ElasticsearchPublisherService
var logData = new
{
    id = auditLog.Id.ToString(),
    timestamp = auditLog.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
    user_id = auditLog.UserId.ToString(),
    user_name = auditLog.UserName,
    action = auditLog.Action,
    entity_name = auditLog.EntityName,
    entity_id = auditLog.EntityId?.ToString(),
    ip_address = auditLog.IpAddress,
    data_before = auditLog.DataBefore,
    data_after = auditLog.DataAfter,
    correlation_id = auditLog.CorrelationId,
    meta_data = auditLog.MetaData,
    service = "cmms",
    log_level = GetLogLevel(auditLog.Action)
};
```

### 2. Kafka Processing
Logstash consumes from Kafka topic `cmms-audit-logs`:

```conf
input {
  kafka {
    bootstrap_servers => "kafka:29092"
    topics => ["cmms-audit-logs"]
    codec => json
    client_id => "cmms-logstash"
    group_id => "cmms-logstash-group"
    auto_offset_reset => "earliest"
    consumer_threads => 1
  }
}
```

### 3. Data Transformation
Logstash processes and enriches the data:

```conf
filter {
  # Parse timestamps
  if [timestamp] {
    date {
      match => [ "timestamp", "ISO8601" ]
      target => "@timestamp"
    }
  }
  
  # Parse JSON fields
  if [data_before] and [data_before] != "" {
    json {
      source => "data_before"
      target => "parsed_data_before"
    }
  }
  
  # Add log levels based on action
  if [action] =~ /(Access Denied|Error)/ {
    mutate {
      add_field => { "log_level" => "WARN" }
    }
  } else {
    mutate {
      add_field => { "log_level" => "INFO" }
    }
  }
}
```

### 4. Elasticsearch Storage
Data is stored in daily indices with proper mapping:

```conf
output {
  elasticsearch {
    hosts => ["elasticsearch:9200"]
    index => "cmms-audit-logs-%{+YYYY.MM.dd}"
    document_id => "%{id}"
    action => "index"
  }
}
```

## Configuration Files

### Logstash Configuration
- **Main Config**: `elk/logstash/config/logstash.yml`
- **Pipeline**: `elk/logstash/pipeline/logstash.conf`

### Kibana Dashboards
- **Main Dashboard**: `elk/kibana/dashboards/audit-logs-overview.json`
- **Visualizations**: `elk/kibana/dashboards/visualizations/`
- **Index Pattern**: `elk/kibana/dashboards/index-patterns/`

## Monitoring and Maintenance

### Health Checks
```bash
# Elasticsearch health
curl http://localhost:9200/_cluster/health?pretty

# Logstash status
curl http://localhost:9600/?pretty

# Kibana status
curl http://localhost:5601/api/status
```

### Index Management
```bash
# List indices
curl http://localhost:9200/_cat/indices?v

# Delete old indices (older than 30 days)
curl -X DELETE "localhost:9200/cmms-audit-logs-$(date -d '30 days ago' +%Y.%m.%d)"
```

### Log Monitoring
```bash
# View Logstash logs
docker-compose logs -f logstash

# View Elasticsearch logs
docker-compose logs -f elasticsearch

# View Kibana logs
docker-compose logs -f kibana
```

## Troubleshooting

### Common Issues

1. **Kibana can't connect to Elasticsearch**
   - Check if Elasticsearch is running: `curl http://localhost:9200`
   - Verify network connectivity between containers
   - Check Elasticsearch logs for errors

2. **No data appearing in dashboards**
   - Verify Logstash is processing data: `curl http://localhost:9600`
   - Check if Kafka topic has data
   - Verify index pattern is created correctly

3. **High memory usage**
   - Adjust JVM heap size in docker-compose.yml
   - Monitor with: `docker stats`

### Performance Tuning

1. **Elasticsearch**
   ```yaml
   environment:
     - "ES_JAVA_OPTS=-Xms1g -Xmx1g"  # Adjust heap size
   ```

2. **Logstash**
   ```yaml
   environment:
     LS_JAVA_OPTS: "-Xmx512m -Xms512m"  # Adjust heap size
   ```

3. **Index Settings**
   ```bash
   # Optimize index settings
   curl -X PUT "localhost:9200/cmms-audit-logs-*/_settings" -H 'Content-Type: application/json' -d'
   {
     "index": {
       "number_of_replicas": 0,
       "refresh_interval": "30s"
     }
   }'
   ```

## Security Considerations

### Development Setup
- Security is disabled for development (`xpack.security.enabled=false`)
- No authentication required
- All services accessible on localhost

### Production Setup
- Enable X-Pack security
- Configure authentication and authorization
- Use HTTPS/TLS
- Implement proper network security
- Regular security updates

## Backup and Recovery

### Index Backup
```bash
# Create snapshot repository
curl -X PUT "localhost:9200/_snapshot/backup_repo" -H 'Content-Type: application/json' -d'
{
  "type": "fs",
  "settings": {
    "location": "/backup"
  }
}'

# Create snapshot
curl -X PUT "localhost:9200/_snapshot/backup_repo/snapshot_1?wait_for_completion=true"
```

### Data Export
```bash
# Export data to JSON
curl -X GET "localhost:9200/cmms-audit-logs-*/_search?size=10000" > audit_logs_export.json
```

## API Endpoints

### Elasticsearch
- **Health**: `GET http://localhost:9200/_cluster/health`
- **Indices**: `GET http://localhost:9200/_cat/indices`
- **Search**: `GET http://localhost:9200/cmms-audit-logs-*/_search`

### Kibana
- **Status**: `GET http://localhost:5601/api/status`
- **Saved Objects**: `GET http://localhost:5601/api/saved_objects/_find`

### Logstash
- **Status**: `GET http://localhost:9600`
- **Pipeline**: `GET http://localhost:9600/_node/pipeline`

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review service logs: `docker-compose logs [service-name]`
3. Verify configuration files
4. Test individual components

## Version Information

- **Elasticsearch**: 8.11.0
- **Logstash**: 8.11.0
- **Kibana**: 8.11.0
- **Kafka**: 7.4.0 