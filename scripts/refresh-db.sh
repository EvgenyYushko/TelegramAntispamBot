#!/bin/bash

# Конфигурация
BACKUP_FILE="backup.dump"
RENDER_API_KEY="rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ"
RENDER_SERVICE_ID="dpg-cu365mt2ng1s73c6t8b0-a"

# Шаг 1: Получение данных текущей БД
echo "🔄 Получение данных БД..."
DB_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$RENDER_SERVICE_ID" \
  -H "accept: application/json" \
  -H "authorization: Bearer $RENDER_API_KEY")

# Шаг 2: Проверка успешности запроса
if [ -z "$DB_INFO" ]; then
  echo "❌ Ошибка: Не удалось получить данные БД."
  exit 1
fi

# Шаг 3: Получение конфиденциальных данных подключения к БД
echo "🔄 Получение данных подключения к БД..."
CONNECTION_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$RENDER_SERVICE_ID/connection-info" \
  -H "accept: application/json" \
  -H "authorization: Bearer $RENDER_API_KEY")

# Шаг 4: Проверка успешности запроса
if [ -z "$CONNECTION_INFO" ]; then
  echo "❌ Ошибка: Не удалось получить данные подключения."
  exit 1
fi

# Шаг 5: Извлечение параметров подключения
DB_NAME=$(echo "$DB_INFO" | jq -r '.databaseName')
DB_PORT=5432  # Порт PostgreSQL по умолчанию
DB_USER=$(echo "$DB_INFO" | jq -r '.databaseUser')
DB_HOST="$RENDER_SERVICE_ID.oregon-postgres.render.com"
DB_PASSWORD=$(echo "$CONNECTION_INFO" | jq -r '.password')

# Шаг 6: Проверка наличия всех данных
if [ -z "$DB_HOST" ] || [ -z "$DB_PORT" ] || [ -z "$DB_USER" ] || [ -z "$DB_PASSWORD" ] || [ -z "$DB_NAME" ]; then
  echo "❌ Ошибка: Не удалось извлечь все параметры подключения."
  echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER DB_PASSWORD=$DB_PASSWORD"
  echo "CONNECTION_INFO: $CONNECTION_INFO"
  echo "DB_INFO: $DB_INFO"
  exit 1
fi

echo "✅ Данные подключения получены:"
echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER DB_PASSWORD=$DB_PASSWORD"

# Шаг 7: Создание бекапа
echo "🔄 Создание бекапа..."
PGPASSWORD=$DB_PASSWORD 
pg_dump -h dpg-cu365mt2ng1s73c6t8b0-a.oregon-postgres.render.com -p $DB_PORT -U $DB_USER -d $DB_NAME -Fc -f $BACKUP_FILE

if [ $? -eq 0 ]; then
  echo "✅ Бекап успешно создан: $BACKUP_FILE"
else
  echo "❌ Ошибка: Не удалось создать бекап."
  exit 1
fi
