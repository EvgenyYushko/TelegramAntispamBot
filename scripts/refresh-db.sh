#!/bin/bash

# Конфигурация
BACKUP_FILE_NAME="backup.dump"
RENDER_API_KEY="rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ"
#DB_ID="dpg-culfvgdds78s73br3pdg-a"
WEB_SERVICE_ID="srv-ctaoq5hu0jms73f1l3q0"

NEW_DB_NAME="telergamdb"
NEW_DB_USER="telergamdb_user"

# Функция для гарантированного запуска сервиса при ошибке
trap 'handle_error' ERR
handle_error() {
  echo "❌ Script failed! Attempting to start the web service..."
  curl -X POST "https://api.render.com/v1/services/$WEB_SERVICE_ID/resume" \
    -H "Authorization: Bearer $RENDER_API_KEY"
  exit 1
}

ALL_DB=$(curl -s --request GET \
  --url 'https://api.render.com/v1/postgres?includeReplicas=true&limit=20' \
  --header 'accept: application/json' \
  --header "authorization: Bearer $RENDER_API_KEY")

DB_ID=$(echo "$ALL_DB" | jq -r '.[] | select(.postgres.name=="TelergamDB") | .postgres.id')

if [ -n "$DB_ID" ] && [ "$DB_ID" != "null" ]; then
    echo "Найден OWNER ID для базы TelergamDB: $DB_ID"
else
    echo "❌ Не удалось найти базу с именем TelergamDB или извлечь OWNER ID."
    exit 1
fi

echo "🛑 Stopping web service..."
curl -s -X POST "https://api.render.com/v1/services/$WEB_SERVICE_ID/suspend" \
  -H "Authorization: Bearer $RENDER_API_KEY"
#sleep 60

# Шаг 1: Получение данных текущей БД
echo "🔄 Получение данных БД..."
DB_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$DB_ID" \
  -H "accept: application/json" \
  -H "authorization: Bearer $RENDER_API_KEY")

# Шаг 2: Проверка успешности запроса
if [ -z "$DB_INFO" ]; then
  echo "❌ Ошибка: Не удалось получить данные БД."
  exit 1
fi

# Шаг 3: Получение конфиденциальных данных подключения к БД
echo "🔄 Получение данных подключения к БД..."
CONNECTION_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$DB_ID/connection-info" \
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
DB_HOST="$DB_ID.oregon-postgres.render.com"
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
export PGPASSWORD=$DB_PASSWORD
pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME --no-owner --no-acl -Fc -f $BACKUP_FILE_NAME

#pwd
#ls -lh backup.dump
#rm -i backup.dump

if [ $? -eq 0 ]; then
  echo "✅ Бекап успешно создан: $BACKUP_FILE_NAME"
else
  echo "❌ Ошибка: Не удалось создать бекап."
fi

echo "Try suspend DB"
curl --request POST \
     --url https://api.render.com/v1/postgres/$DB_ID/suspend \
     --header 'accept: application/json' \
     --header "authorization: Bearer $RENDER_API_KEY"
echo "Sleep 20 sec"
sleep 20

#echo "Try resume DB"
#curl --request POST \
#     --url https://api.render.com/v1/postgres/$DB_ID/resume \
#     --header 'accept: application/json' \
#     --header "authorization: Bearer $RENDER_API_KEY"

echo "Try Delete DB"
curl --request DELETE \
     --url https://api.render.com/v1/postgres/$DB_ID \
     --header 'accept: application/json' \
     --header "authorization: Bearer $RENDER_API_KEY"
     
echo "Sleep 20 sec"
sleep 20

echo "Try Create DB"
Response=$(curl --request POST \
     --url https://api.render.com/v1/postgres \
     --header "accept: application/json" \
     --header "authorization: Bearer $RENDER_API_KEY" \
     --header "content-type: application/json" \
     --data "{
  \"databaseName\": \"$NEW_DB_NAME\",
  \"databaseUser\": \"$NEW_DB_USER\",
  \"enableHighAvailability\": false,
  \"plan\": \"free\",
  \"version\": \"16\",
  \"name\": \"TelergamDB\",
  \"ownerId\": \"tea-ct84bie8ii6s73ccgf1g\",
  \"ipAllowList\": [
    {
      \"cidrBlock\": \"0.0.0.0/0\",
      \"description\": \"everywhere\"
    }
  ]
}")
echo "Sleep 1 min"
sleep 30

echo "⏳ Ожидание готовности БД..."
MAX_RETRIES=30
RETRY_INTERVAL=10

for i in $(seq 1 $MAX_RETRIES); do
    RESPONSE=$(curl -s --request GET \
             --url "https://api.render.com/v1/postgres/$NEW_DB_ID" \
             --header 'accept: application/json' \
             --header "authorization: Bearer $RENDER_API_KEY")
    
    echo "📝 Ответ API: $RESPONSE"  # Вывод ответа API для отладки

    # Получаем статус, учитывая массив и объект postgres
    STATUS=$(echo "$RESPONSE" | jq -r '.[0].postgres.status // empty')

    if [ "$STATUS" == "available" ]; then
        echo "✅ БД готова!"
        break
    fi
    
    echo "⏳ Статус: $STATUS. Повтор через $RETRY_INTERVAL секунд..."
    sleep $RETRY_INTERVAL
done

NEW_DB_ID=$(echo "$Response" | jq -r '.id')
NEW_DB_NAME=$(echo "$Response" | jq -r '.databaseName')
NEW_DB_USER=$(echo "$Response" | jq -r '.databaseUser')

echo "NEW_DB_ID:" $NEW_DB_ID

if [ -z "$NEW_DB_ID" ] || [ "$NEW_DB_ID" == "null" ]; then
  echo "❌ Ошибка: Не удалось создать БД!"
  echo "Ответ API: $Response"
  exit 1
fi

sleep 10

echo "🔄 Получение данных подключения к новой БД..."
CONNECTION_NEW_DB_INFO=$(curl -s -X GET "https://api.render.com/v1/postgres/$NEW_DB_ID/connection-info" \
  -H "accept: application/json" \
  -H "authorization: Bearer $RENDER_API_KEY")

if [ -z "$CONNECTION_NEW_DB_INFO" ]; then
  echo "❌ Ошибка: Не удалось получить данные подключения к новой БД."
  exit 1
fi

NEW_DB_PASSWORD=$(echo "$CONNECTION_NEW_DB_INFO" | jq -r '.password')

sleep 10
export PGPASSWORD=$NEW_DB_PASSWORD
pg_restore -h "$NEW_DB_ID.oregon-postgres.render.com" -p 5432 -U $NEW_DB_USER -d $NEW_DB_NAME --no-owner backup.dump

CONNECTION_STRING="Host=$NEW_DB_ID;Database=$NEW_DB_NAME;Username=$NEW_DB_USER;Password=$NEW_DB_PASSWORD;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
echo "CONNECTION_STRING=" $CONNECTION_STRING

curl --request PUT \
     --url https://api.render.com/v1/services/$WEB_SERVICE_ID/env-vars/DB_URL_POSTGRESQL \
     --header 'accept: application/json' \
     --header "Authorization: Bearer $RENDER_API_KEY" \
     --header 'content-type: application/json' \
     --data "{\"value\":\"$CONNECTION_STRING\"}"

echo "🚀 Resume web service..."
curl -X POST "https://api.render.com/v1/services/$WEB_SERVICE_ID/resume" \
    -H "Authorization: Bearer $RENDER_API_KEY"

echo "🚀 Deploy web service..."
curl --request POST \
     --url https://api.render.com/v1/services/srv-ctaoq5hu0jms73f1l3q0/deploys \
     --header 'accept: application/json' \
     --header 'authorization: Bearer rnd_sZLs5c8GIjjEmSc7EwblTKTvoTLZ' \
     --header 'content-type: application/json' \
     --data '
{
  "clearCache": "do_not_clear"
}
'

# Проверка доступности
echo "🔍 Checking site availability..."
MAX_RETRIES=10
RETRY_INTERVAL=30
SITE_URL="https://telegramantispambot.onrender.com/"

for i in $(seq 1 $MAX_RETRIES); do
  HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" $SITE_URL)
  if [ "$HTTP_STATUS" -eq 200 ]; then
    echo "✅ Site is up!"
    exit 0
  fi
  echo "⏳ Статус: $HTTP_STATUS. Повтор через $RETRY_INTERVAL секунд..."
  sleep $RETRY_INTERVAL
done

echo "❌ Site failed to start after $MAX_RETRIES attempts."
exit 1
