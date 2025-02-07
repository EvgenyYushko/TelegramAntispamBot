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

# Проверка успешности запроса
if [ -z "$DB_INFO" ]; then
  echo "❌ Ошибка: Не удалось получить данные БД."
  exit 1
fi

# Извлечение параметров подключения
DB_HOST=$(echo "$DB_INFO" | jq -r '.name + ".oregon-postgres.render.com"')
DB_PORT=5432  # Порт PostgreSQL по умолчанию
DB_USER=$(echo "$DB_INFO" | jq -r '.databaseUser')
DB_NAME=$(echo "$DB_INFO" | jq -r '.databaseName')

# Проверка наличия всех данных
if [ -z "$DB_HOST" ] || [ -z "$DB_USER" ] || [ -z "$DB_NAME" ]; then
  echo "❌ Ошибка: Не удалось извлечь все параметры подключения."
  echo "DB_INFO: $DB_INFO"
  exit 1
fi

echo "✅ Данные БД получены:"
echo "DB_NAME=$DB_NAME DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER

curl --request POST \
     --url https://api.render.com/v1/postgres/dpg-cu365mt2ng1s73c6t8b0-a/backup \
     --header 'accept: application/json' \
     --header 'authorization: Bearer rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ'

# Шаг 2: Создание бекапа
echo "🔄 Создание бекапа..."
pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -Fc -f $BACKUP_FILE

if [ $? -eq 0 ]; then
  echo "✅ Бекап успешно создан: $BACKUP_FILE"
else
  echo "❌ Ошибка: Не удалось создать бекап."
  exit 1
fi
