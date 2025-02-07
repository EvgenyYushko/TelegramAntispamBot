#!/bin/bash

# Конфигурация
BACKUP_FILE="backup.dump"
RENDER_API_KEY="rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ"
RENDER_SERVICE_ID="dpg-cu365mt2ng1s73c6t8b0-a"

# Шаг 1: Получение данных текущей БД
DB_INFO=$(curl -s -X GET "https://api.render.com/v1/services/$RENDER_SERVICE_ID/env-vars" \
  -H "Authorization: Bearer $RENDER_API_KEY" | jq -r '.[] | select(.key | endswith("DATABASE_URL")) | .value')

# Извлечение параметров подключения
DB_HOST=$(echo $DB_INFO | awk -F'@' '{print $2}' | cut -d':' -f1)
DB_PORT=$(echo $DB_INFO | awk -F':' '{print $4}' | cut -d'/' -f1)
DB_USER=$(echo $DB_INFO | awk -F':' '{print $2}' | cut -d'/' -f3)
DB_PASSWORD=$(echo $DB_INFO | awk -F':' '{print $3}' | cut -d'@' -f1)
DB_NAME=$(echo $DB_INFO | awk -F'/' '{print $4}')

echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER DB_PASSWORD=$DB_PASSWORD"

# Шаг 2: Создание бекапа
echo "🔄 Creating backup..."
PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -Fc -f $BACKUP_FILE
