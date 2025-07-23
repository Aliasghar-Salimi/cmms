#!/bin/bash

# SQL Server Container Management Script
# Usage: ./sqlserver.sh [start|stop|restart|status|logs|connect|reset]

read -p "Enter SQL Server container name [cmms-sqlserver]: " CONTAINER_NAME
CONTAINER_NAME=${CONTAINER_NAME:-cmms-sqlserver}
read -s -p "Enter SA password [Ali@1234]: " SA_PASSWORD
echo
SA_PASSWORD=${SA_PASSWORD:-Ali@1234}

case "$1" in
    start)
        echo "🚀 Starting SQL Server container..."
        docker run -d --name $CONTAINER_NAME \
            -e "ACCEPT_EULA=Y" \
            -e "SA_PASSWORD=$SA_PASSWORD" \
            -e "MSSQL_PID=Developer" \
            -p 1433:1433 \
            mcr.microsoft.com/mssql/server:2022-latest
        echo "✅ SQL Server container started"
        ;;
    stop)
        echo "🛑 Stopping SQL Server container..."
        docker stop $CONTAINER_NAME
        echo "✅ SQL Server container stopped"
        ;;
    restart)
        echo "🔄 Restarting SQL Server container..."
        docker restart $CONTAINER_NAME
        echo "✅ SQL Server container restarted"
        ;;
    status)
        echo "📊 SQL Server container status:"
        docker ps -a --filter "name=$CONTAINER_NAME"
        ;;
    logs)
        echo "📋 SQL Server container logs:"
        docker logs $CONTAINER_NAME
        ;;
    connect)
        echo "🔌 Connecting to SQL Server..."
        docker exec -it $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C
        ;;
    test)
        echo "🧪 Testing SQL Server connection..."
        docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT @@VERSION"
        ;;
    reset)
        echo "🗑️ Removing SQL Server container and data..."
        docker stop $CONTAINER_NAME 2>/dev/null
        docker rm $CONTAINER_NAME 2>/dev/null
        docker volume rm $(docker volume ls -q --filter "name=sqlserver_data") 2>/dev/null
        echo "✅ SQL Server container and data removed"
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status|logs|connect|test|reset}"
        echo ""
        echo "Commands:"
        echo "  start   - Start SQL Server container"
        echo "  stop    - Stop SQL Server container"
        echo "  restart - Restart SQL Server container"
        echo "  status  - Show container status"
        echo "  logs    - Show container logs"
        echo "  connect - Connect to SQL Server interactively"
        echo "  test    - Test SQL Server connection"
        echo "  reset   - Remove container and data (clean slate)"
        exit 1
        ;;
esac 