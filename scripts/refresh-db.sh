#!/bin/bash

# Конфигурация
BACKUP_FILE="backup.dump"
# Используйте переменные окружения в CI/CD, не хардкодите их
# RENDER_API_KEY="rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ"
# RENDER_SERVICE_ID="dpg-cu365mt2ng1s73c6t8b0-a"

# Шаг 1: Получение данных текущей БД
RAW_JSON=$(curl -s -X GET "https://api.render.com/v1/services/$RENDER_SERVICE_ID/env-vars" \
  -H "Authorization: Bearer $RENDER_API_KEY")

# Выводим сырой JSON для отладки
echo "DEBUG: $RAW_JSON"

# Попытка извлечь DATABASE_URL. Первый вариант — если JSON содержит ключ "envVars".
DB_INFO=$(echo "$RAW_JSON" | jq -r '.envVars[] | select(.key=="DATABASE_URL") | .value')

# Если DB_INFO всё ещё пустое, попробуйте предположить, что JSON возвращается как массив
if [ -z "$DB_INFO" ]; then
    DB_INFO=$(echo "$RAW_JSON" | jq -r '.[] | select(.key=="DATABASE_URL") | .value')
fi

echo "DB_INFO: $DB_INFO"

# Если DB_INFO не получен, завершаем выполнение
if [ -z "$DB_INFO" ]; then
    echo "❌ Не удалось получить строку подключения из Render API."
    exit 1
fi

# Извлечение параметров подключения из строки вида:
# postgres://username:password@host:port/database
DB_HOST=$(echo "$DB_INFO" | awk -F'@' '{print $2}' | cut -d':' -f1)
DB_PORT=$(echo "$DB_INFO" | awk -F':' '{print $4}' | cut -d'/' -f1)
DB_USER=$(echo "$DB_INFO" | awk -F':' '{print $2}' | cut -d'/' -f3)
DB_PASSWORD=$(echo "$DB_INFO" | awk -F':' '{print $3}' | cut -d'@' -f1)
DB_NAME=$(echo "$DB_INFO" | awk -F'/' '{print $4}')

echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER DB_PASSWORD=$DB_PASSWORD"

# Шаг 2: Создание бэкапа
echo "🔄 Creating backup..."
PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -Fc -f $BACKUP_FILE
