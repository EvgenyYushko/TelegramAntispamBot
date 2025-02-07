#!/bin/bash

# Конфигурация
BACKUP_FILE="backup.dump"
# Используйте переменные окружения в CI/CD, не хардкодите их
RENDER_API_KEY="rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ"
RENDER_SERVICE_ID="dpg-cu365mt2ng1s73c6t8b0-a"

 Шаг 1: Получение данных текущей БД
echo "🔄 Получение данных БД..."
DB_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$RENDER_SERVICE_ID" \
  -H "accept: application/json" \
  -H "authorization: Bearer $RENDER_API_KEY")

echo $DB_INFO

# Извлечение параметров подключения
DB_HOST=$(echo "$DB_INFO" | jq -r '.serviceDetails.connectionDetails.host')
DB_PORT=$(echo "$DB_INFO" | jq -r '.serviceDetails.connectionDetails.port')
DB_USER=$(echo "$DB_INFO" | jq -r '.serviceDetails.connectionDetails.user')
DB_PASSWORD=$(echo "$DB_INFO" | jq -r '.serviceDetails.connectionDetails.password')
DB_NAME=$(echo "$DB_INFO" | jq -r '.serviceDetails.connectionDetails.database')

# Проверка наличия всех данных
if [ -z "$DB_HOST" ] || [ -z "$DB_PORT" ] || [ -z "$DB_USER" ] || [ -z "$DB_PASSWORD" ] || [ -z "$DB_NAME" ]; then
  echo "❌ Ошибка: Не удалось извлечь все параметры подключения."
  exit 1
fi

echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER DB_PASSWORD=$DB_PASSWORD"

# Шаг 2: Создание бэкапа
#echo "🔄 Creating backup..."
#PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -Fc -f $BACKUP_FILE
