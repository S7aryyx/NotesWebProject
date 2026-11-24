# Notes

Небольшой проект заметок, который я делал для практики разработки на C#
и .NET. Идея приложения --- хранить заметки и папки пользователя,
работать с ними через API и постепенно добавлять дополнительные
возможности: AI-анализ заметок и платный доступ к нему.

Проект состоит из нескольких частей: основное API, WPF-клиент, отдельный
модуль локального AI, а также модуль для работы с платежами через
ЮKassa.

> Проект находится в разработке. Некоторые части решения являются
> отдельными демонстрационными или тестовыми модулями. Перед
> использованием в реальной эксплуатации нужно дополнительно проверить
> конфигурацию, безопасность и сценарии оплаты.

## Возможности

-   Регистрация и вход пользователя.
-   Работа с заметками и папками.
-   Создание, изменение и удаление заметок.
-   Избранное и архив заметок.
-   Хранение данных в PostgreSQL.
-   WPF-клиент для Windows.
-   Локальный AI-анализ заметок через Ollama.
-   Платный доступ к AI-функциям:
    -   месячная подписка --- 349 ₽ на 30 дней;
    -   единоразовый доступ --- 4 999 ₽.
-   Интеграция с ЮKassa: создание платежа, получение статуса и обработка
    HTTP-уведомлений.

AI-анализ в текущей задумке используется через Desktop-клиент.
Веб-интерфейс предназначен для работы с заметками и оплатой; AI-функции
в веб-версии не заявлены.

## Технологии

-   C# / .NET
-   WPF
-   ASP.NET Core Web API
-   PostgreSQL
-   Npgsql
-   Ollama
-   ЮKassa API
-   HTML, CSS и JavaScript для веб-интерфейса

## Структура решения

Основной файл решения --- `AIC-Notes.slnx`.

``` text
Notes/
├── AIC-Notes.slnx
├── database/
│   └── Database template.sql
├── src/
│   ├── Notes/
│   │   ├── Notes.Api/          # Основной Web API и веб-интерфейс
│   │   ├── Notes.Core/         # Основная логика, DTO, сервисы и доступ к данным
│   │   └── Notes.Desktop/      # WPF-клиент Notes
│   ├── LocalAI/
│   │   ├── LocalAI.Api/        # API для обращения к локальной модели
│   │   └── LocalAI.Desktop/    # WPF-клиент/тестовый интерфейс AI
│   └── Payments/
│       ├── Payments.Api/       # Отдельный API-модуль платежей
│       ├── Payments.Contracts/ # Общие модели и DTO платежей
│       └── Payments.Desktop/  # WPF-клиент тестового платежного модуля
├── .gitignore
└── Fix-GitTracking.ps1
```

### Кратко о проектах

  -----------------------------------------------------------------------
  Проект                              Для чего нужен
  ----------------------------------- -----------------------------------
  `Notes.Api`                         Регистрация, авторизация, API
                                      заметок и папок, встроенные
                                      endpoints оплаты, статусы доступа и
                                      webhook ЮKassa. Также содержит
                                      web-интерфейс в `wwwroot`.

  `Notes.Core`                        Сервисы, DTO, интерфейсы
                                      репозиториев и реализация доступа к
                                      PostgreSQL/JSON.

  `Notes.Desktop`                     Основное WPF-приложение для работы
                                      с заметками.

  `LocalAI.Api`                       Принимает запросы на AI-анализ и
                                      отправляет их в Ollama.

  `LocalAI.Desktop`                   Отдельный WPF-клиент для проверки
                                      AI API.

  `Payments.Api`                      Отдельный тестовый API для создания
                                      платежей и запроса их статуса в
                                      ЮKassa.

  `Payments.Contracts`                Общие модели запросов и ответов для
                                      платежного модуля.

  `Payments.Desktop`                  Отдельный WPF-клиент для
                                      тестирования платежного API.
  -----------------------------------------------------------------------

В основном сценарии приложения платежные endpoints находятся в
`Notes.Api` (`/api/billing/...`). `Payments.Api` оставлен в решении как
отдельный модуль для тестирования интеграции.

## Что понадобится для запуска

Перед началом нужно установить:

1.  **Visual Studio 2022** с workload **.NET desktop development** и
    **ASP.NET and web development**.
2.  **.NET 10 SDK**. В решении есть проекты на `net10.0` /
    `net10.0-windows`, а `Notes.Api` и `Notes.Core` пока на `net8.0`.
    Для сборки всего решения нужен SDK, поддерживающий .NET 10;
    отдельный .NET 8 Runtime может понадобиться при запуске
    соответствующих приложений вне Visual Studio.
3.  **PostgreSQL 14 или новее**.
4.  **pgAdmin 4** --- для удобного выполнения SQL-скрипта и просмотра
    базы.
5.  **Ollama** --- если планируется проверять AI-анализ заметок.
6.  Аккаунт и данные магазина в **ЮKassa** --- если нужно тестировать
    платежи.

## 1. Клонирование проекта

``` bash
git clone <URL-репозитория>
cd Notes
```

После клонирования откройте `AIC-Notes.slnx` в Visual Studio.

Если Visual Studio показывает большое количество изменений в `.vs`,
`bin` или `obj`, в корне проекта уже есть `.gitignore` и скрипт для
удаления ранее отслеживаемых генерируемых файлов из Git-индекса.

Запускать скрипт нужно из корневой папки репозитория в PowerShell:

``` powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Fix-GitTracking.ps1
```

Скрипт предназначен для очистки Git-индекса от генерируемых файлов. Он
не должен удалять локальные файлы сборки и настройки Visual Studio с
диска. После выполнения проверьте список изменений в Git Changes и не
коммитьте случайно пользовательские файлы IDE.

## 2. Создание базы данных PostgreSQL

1.  Запустите PostgreSQL.
2.  В pgAdmin 4 создайте пустую базу данных, например `notes_db`.
3.  Откройте Query Tool для этой базы.
4.  Откройте файл `database/Database template.sql`.
5.  Выполните весь скрипт.

Скрипт создаёт логические схемы:

  Схема       Содержимое
  ----------- --------------------------------------
  `auth`      Пользователи
  `content`   Папки и заметки
  `billing`   Платежные транзакции и права доступа

Если в базе остались таблицы старой схемы `"Ilgam"`, SQL-шаблон пытается
перенести их в новые схемы. Перед миграцией существующей базы
обязательно сделайте резервную копию.

После выполнения проверьте, что появились таблицы `auth.Users`,
`content.Folders`, `content.Notes`, `billing.UserAccess` и
`billing.PaymentTransactions`.

## 3. Настройка секретов Notes.Api

Секреты и строку подключения не нужно хранить в `appsettings.json` и
нельзя публиковать в GitHub. Для локальной разработки используйте **User
Secrets**.

Откройте терминал в корне репозитория и выполните команды (замените
значения на свои):

``` powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=notes_db;Username=postgres;Password=YOUR_DB_PASSWORD" --project src/Notes/Notes.Api/Notes.Api.csproj

dotnet user-secrets set "YooKassa:ShopId" "YOUR_SHOP_ID" --project src/Notes/Notes.Api/Notes.Api.csproj

dotnet user-secrets set "YooKassa:SecretKey" "YOUR_SECRET_KEY" --project src/Notes/Notes.Api/Notes.Api.csproj

dotnet user-secrets set "YooKassa:ReturnUrl" "https://YOUR_PUBLIC_HOST/payment.html?payment=return" --project src/Notes/Notes.Api/Notes.Api.csproj

dotnet user-secrets set "AiAccess:SigningKey" "YOUR_RANDOM_SECRET_KEY_AT_LEAST_32_BYTES" --project src/Notes/Notes.Api/Notes.Api.csproj
```

`AiAccess:SigningKey` должен быть случайной секретной строкой длиной не
менее 32 байт. Для генерации можно использовать PowerShell:

``` powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
$key
```

Скопируйте полученное значение в команду `dotnet user-secrets set`.

Посмотреть сохранённые имена и значения можно командой
`dotnet user-secrets list`, но не публикуйте её вывод: там находятся
секреты.

## 4. Настройка Ollama и LocalAI.Api

AI API обращается к локальному Ollama по адресу:

``` text
http://localhost:11434/
```

В текущем коде используется модель `qwen3:8b`.

Установите Ollama, затем загрузите модель:

``` bash
ollama pull qwen3:8b
```

Убедитесь, что Ollama запущен и API доступен на порту `11434`. Обычно
Ollama запускает локальный сервер автоматически; если он не запущен,
выполните:

``` bash
ollama serve
```

Запустите `LocalAI.Api`:

``` powershell
dotnet run --project src/LocalAI/LocalAI.Api/LocalAI.Api.csproj
```

В текущем профиле запуска используются адреса: - HTTP:
`http://localhost:5156` - HTTPS: `https://localhost:7110`

Если сертификат HTTPS для локальной разработки не доверенный, настройте
dev-сертификат .NET:

``` powershell
dotnet dev-certs https --trust
```

## 5. Восстановление и сборка решения

Из корневой папки проекта выполните:

``` powershell
dotnet restore AIC-Notes.slnx
dotnet build AIC-Notes.slnx
```

Либо откройте решение в Visual Studio и выполните **Build → Build
Solution**.

Если сборка завершается ошибкой, проверьте установку .NET 10 SDK и
восстановление NuGet-пакетов.

## 6. Запуск основного API и WPF-клиента

Запустите `Notes.Api`:

``` powershell
dotnet run --project src/Notes/Notes.Api/Notes.Api.csproj
```

В профиле разработки используются: - HTTP: `http://localhost:5080` -
HTTPS: `https://localhost:7080` - Swagger: `/swagger`

После запуска Swagger доступен по адресу:

``` text
https://localhost:7080/swagger
```

Для запуска WPF-клиента выберите `Notes.Desktop` как Startup Project в
Visual Studio и нажмите **F5**.

Если запускаете API и Desktop одновременно, проверьте, что локальные
адреса в настройках клиента соответствуют запущенным сервисам.

## 7. Настройка и запуск платежей ЮKassa

В основном приложении создание платежа находится в `Notes.Api`:

``` text
POST /api/billing/create
GET  /api/billing/status
GET  /api/billing/ai-token
POST /api/billing/notifications/yookassa
```

Для создания платежа пользователь должен быть авторизован. Доступные
тарифы: - `monthly` --- 349 ₽, доступ на 30 дней; - `lifetime` --- 4 999
₽, единоразовый доступ.

В запросе создания платежа передаётся план, например:

``` json
{
  "plan": "monthly"
}
```

После создания платежа API возвращает ссылку `confirmationUrl`.
Пользователь переходит по ней на страницу оплаты ЮKassa.

### Важное условие для получения статусов оплаты

Чтобы ЮKassa могла отправлять уведомления об изменении статуса платежа,
у приложения должен быть **доступный из интернета HTTPS-адрес**. Адрес
`localhost` для webhook не подходит: сервер ЮKassa не может обратиться к
localhost на компьютере разработчика.

Для тестирования я использовал [Pinggy
Dashboard](https://dashboard.pinggy.io/). С его помощью можно поднять
временный HTTPS-туннель к локальному API. Официальная документация:
[Pinggy Quickstart](https://pinggy.io/docs/).

Пример команды для текущего локального HTTP-порта `Notes.Api`:

``` powershell
ssh -p 443 -R0:127.0.0.1:5080 -o ServerAliveInterval=30 free.pinggy.io
```

После запуска Pinggy выдаёт публичный HTTPS-адрес. Если, например, он
выдал `https://example.free.pinggy.net`, адрес webhook будет:

``` text
https://example.free.pinggy.net/api/billing/notifications/yookassa
```

Этот URL нужно указать в личном кабинете ЮKassa в разделе настройки
HTTP-уведомлений. Для интеграции используются события: -
`payment.succeeded` --- платёж успешно завершён; - `payment.canceled`
--- платёж отменён.

Также нужно задать `YooKassa:ReturnUrl` на публичную HTTPS-страницу
приложения, например:

``` text
https://example.free.pinggy.net/payment.html?payment=return
```

**Не путайте `ReturnUrl` и webhook URL.** `ReturnUrl` --- адрес, куда
возвращается пользователь после оплаты. Webhook --- адрес, куда ЮKassa
отправляет серверное POST-уведомление об изменении платежа. Сам факт
возврата пользователя на сайт ещё не подтверждает оплату: статус нужно
получать и проверять через API ЮKassa.

У временного туннеля адрес может измениться после перезапуска, а сам
туннель может быть ограничен по времени. При смене адреса нужно обновить
и `YooKassa:ReturnUrl`, и URL HTTP-уведомлений в кабинете ЮKassa. Для
постоянного использования лучше настроить собственный домен и сервер с
HTTPS.

Официальная документация: - [ЮKassa ---
HTTP-уведомления](https://yookassa.ru/developers/using-api/webhooks) -
[ЮKassa --- настройка
HTTP-уведомлений](https://yookassa.ru/docs/support/merchant/payments/http-notifications) -
[Pinggy --- HTTP/HTTPS tunnels](https://pinggy.io/docs/http_tunnels/)

### Отдельный Payments.Api

В решении также есть отдельный `Payments.Api`. Это самостоятельный
тестовый модуль. Для его запуска задайте собственные User Secrets:

``` powershell
dotnet user-secrets set "YooKassa:ShopId" "YOUR_SHOP_ID" --project src/Payments/Payments.Api/Payments.Api.csproj

dotnet user-secrets set "YooKassa:SecretKey" "YOUR_SECRET_KEY" --project src/Payments/Payments.Api/Payments.Api.csproj

dotnet user-secrets set "YooKassa:ReturnUrl" "https://YOUR_PUBLIC_HOST/payments/return" --project src/Payments/Payments.Api/Payments.Api.csproj
```

Затем запустите:

``` powershell
dotnet run --project src/Payments/Payments.Api/Payments.Api.csproj
```

Порты этого модуля задаются в
`src/Payments/Payments.Api/Properties/launchSettings.json` (HTTP `5221`,
HTTPS `443`).

## 8. Запуск через Visual Studio

Можно не запускать каждый проект вручную:

1.  Откройте `AIC-Notes.slnx`.
2.  В Solution Explorer нажмите правой кнопкой по Solution.
3.  Выберите **Configure Startup Projects**.
4.  Установите **Multiple startup projects**.
5.  Для нужных API выберите `Start`.
6.  Для ненужных проектов оставьте `None`.
7.  Нажмите **F5**.

Для обычной работы с заметками нужен `Notes.Api` и `Notes.Desktop`. Для
AI-функций дополнительно нужен Ollama и `LocalAI.Api`. Для тестирования
отдельного платежного модуля можно запустить `Payments.Api` и
`Payments.Desktop`.

## 9. Полезные замечания

-   Не добавляйте в Git `appsettings.*` с реальными паролями, ключами и
    токенами.
-   Не публикуйте `ShopId` вместе с `SecretKey`, строку подключения
    PostgreSQL, `AiAccess:SigningKey`, cookies и содержимое User
    Secrets.
-   Для реального webhook используйте публичный HTTPS URL и проверяйте
    фактический статус платежа через API ЮKassa.
-   Временный Pinggy-туннель подходит для разработки и тестов, но не
    заменяет постоянный хостинг.
-   Перед обновлением существующей базы сделайте резервную копию.
-   `Notes.Api` сейчас использует .NET 8, остальные API и WPF-проекты
    --- .NET 10. Поэтому для сборки всего решения устанавливайте .NET 10
    SDK.
-   Локальные адреса сервисов сейчас заданы в коде некоторых клиентов.
    Если меняете порты или размещаете API на другом компьютере,
    проверьте `BaseAddress` в соответствующих клиентах.

## Статус проекта

Проект развивается и дорабатывается. README описывает текущую структуру
и основные шаги локального запуска. Перед публикацией релиза желательно
отдельно прогнать сборку всех проектов, проверить миграцию базы на копии
данных и протестировать полный сценарий оплаты от создания платежа до
получения webhook и выдачи доступа.

------------------------------------------------------------------------

Спасибо, что заглянули в проект!
