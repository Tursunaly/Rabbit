Проект producer и consumer. Состоит из двух сервисов, которые взаимодействуют между собой через брокера сообщений RabbitMq.
Swagger UI доступен по адресу, для ручной отправки сообщения:

http://localhost:5036/swagger/index.html

используется для тестирования и просмотра API

Роут producer api: 
1. POST /api/rabbit/send

роут /api/rabbit/send отправляет сообщение в очередь RabbitMq
тело(json) 
"текcтовое сообщение"
поведение, принимает строковое сообщение из тела запроса
публикует сообщение рэббит 
ошибки подключения приводят к http 500
ответ:
 
{
  "status": "Сообщение отправлено",
  "message": "текстовое сообщение"
}

Роут consumer api: 

1. GET  /api/consumer/status

2. GET  /api/consumer/health

consumer запускается как backgroundservice в фоновом режиме
/api/consumer/status возвращает текущее состояние статуса о полученного сообщения
/api/consumer/health эндпоит для мониторинга сервиса
проверка состояния consumer 
поведение consumer подключается к RabbitMq
подписка на очередь queue-test 
принимает сообщения
