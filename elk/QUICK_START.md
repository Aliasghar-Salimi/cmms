# CMMS ELK Stack - Quick Start Guide

This guide will help you quickly set up and start using the ELK stack for CMMS audit log visualization.

## Prerequisites

- Docker and Docker Compose installed
- At least 4GB of available RAM
- Ports 9200, 5601, 9600, 9092 available

## Quick Setup (5 minutes)

### 1. Start the ELK Stack

```bash
# Navigate to the project root
cd /path/to/cmms

# Run the complete setup script
./elk/setup-elk-stack.sh
```

This script will:
- Check prerequisites
- Start Elasticsearch, Logstash, and Kibana
- Wait for services to be ready
- Import pre-configured dashboards
- Test the setup

### 2. Verify Setup

```bash
# Check if everything is working
./elk/monitor-elk.sh
```

### 3. Access Kibana

Open your browser and go to: **http://localhost:5601**

## Manual Setup (if needed)

### Step 1: Start Services

```bash
# Start only ELK services
docker-compose up -d elasticsearch logstash kibana

# Wait for services to be ready (about 2-3 minutes)
```

### Step 2: Setup Kibana

```bash
# Navigate to elk directory
cd elk

# Run Kibana setup
./kibana/setup-kibana.sh
```

### Step 3: Test Setup

```bash
# Test the ELK stack
./test-elk-setup.sh
```

## Using the Dashboards

### Main Dashboard

1. Go to **http://localhost:5601**
2. Navigate to **Dashboard** in the left sidebar
3. Open **"CMMS Audit Logs - Real-time Overview"**

### Dashboard Features

#### Real-time Monitoring
- **Timeline**: See audit logs in real-time
- **Total Count**: Current number of audit logs
- **Error Count**: Number of error logs
- **Access Denied**: Number of access denied events

#### Analytics
- **By Action**: Breakdown of Create/Update/Delete/Login actions
- **By User**: Most active users
- **By Entity**: Most accessed entities
- **By Hour**: Hourly distribution
- **By IP**: Top source IP addresses

#### Data Exploration
- **Recent Logs Table**: Detailed view of recent events
- **Search & Filter**: Use Kibana's search capabilities

## Generating Test Data

### Start Your Microservices

```bash
# Start all CMMS services
docker-compose up -d

# Or start specific services
docker-compose up -d identity-service auditlog-service
```

### Manual Test Data

```bash
# Send test audit log to Kafka
echo '{
  "id": "test-123",
  "timestamp": "'$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)'",
  "user_id": "test-user",
  "user_name": "Test User",
  "action": "Test Action",
  "entity_name": "Test Entity",
  "entity_id": "test-entity",
  "ip_address": "192.168.1.100",
  "data_before": "{\"old\": \"value\"}",
  "data_after": "{\"new\": \"value\"}",
  "correlation_id": "test-correlation",
  "meta_data": "{\"test\": true}",
  "service": "cmms",
  "log_level": "INFO"
}' | kafka-console-producer --bootstrap-server localhost:9092 --topic cmms-audit-logs
```

## Monitoring and Maintenance

### Check Service Health

```bash
# Quick health check
./elk/monitor-elk.sh

# Check specific services
curl http://localhost:9200/_cluster/health  # Elasticsearch
curl http://localhost:5601/api/status       # Kibana
curl http://localhost:9600                  # Logstash
```

### View Logs

```bash
# View service logs
docker-compose logs elasticsearch
docker-compose logs logstash
docker-compose logs kibana
```

### Stop Services

```bash
# Stop ELK services
docker-compose stop elasticsearch logstash kibana

# Stop all services
docker-compose down
```

## Troubleshooting

### Common Issues

1. **Services not starting**
   ```bash
   # Check Docker resources
   docker system df
   docker stats
   
   # Increase Docker memory limit (at least 4GB)
   ```

2. **Kibana can't connect to Elasticsearch**
   ```bash
   # Wait longer for services to start
   sleep 60
   
   # Check Elasticsearch logs
   docker-compose logs elasticsearch
   ```

3. **No data in dashboards**
   ```bash
   # Check if Logstash is processing
   curl http://localhost:9600/_node/pipeline
   
   # Check Kafka topic
   kafka-topics --bootstrap-server localhost:9092 --describe --topic cmms-audit-logs
   ```

4. **High memory usage**
   ```bash
   # Adjust JVM heap sizes in docker-compose.yml
   # Elasticsearch: ES_JAVA_OPTS="-Xms512m -Xmx512m"
   # Logstash: LS_JAVA_OPTS="-Xmx256m -Xms256m"
   ```

### Reset Everything

```bash
# Stop all services
docker-compose down

# Remove volumes (WARNING: This will delete all data)
docker-compose down -v

# Start fresh
./elk/setup-elk-stack.sh
```

## Useful URLs

- **Kibana**: http://localhost:5601
- **Elasticsearch**: http://localhost:9200
- **Logstash API**: http://localhost:9600
- **Kafka UI**: http://localhost:8080
- **Main Dashboard**: http://localhost:5601/app/dashboards#/view/cmms-audit-logs-overview
- **Discover**: http://localhost:5601/app/discover
- **Dev Tools**: http://localhost:5601/app/dev_tools#/console

## Next Steps

1. **Customize Dashboards**: Modify visualizations in Kibana
2. **Add Alerts**: Set up alerting for critical events
3. **Scale Up**: Configure for production use
4. **Security**: Enable authentication and authorization
5. **Backup**: Set up regular data backups

## Support

- Check the main README: `elk/README.md`
- Review service logs for errors
- Test individual components
- Monitor system resources

---

**Happy Monitoring! 🚀** 