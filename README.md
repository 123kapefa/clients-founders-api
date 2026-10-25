#  API клиентов и учредителей

Тестовое задание: REST API на ASP.NET Core для учёта клиентов (юридических лиц и индивидуальных предпринимателей) и учредителей юридических лиц.

**Стек:** .NET 10, ASP.NET Core (контроллеры), EF Core 10 (Code First) + Npgsql, PostgreSQL 17 в Docker, FluentValidation, CSharpFunctionalExtensions, OpenAPI + Scalar. Тесты: xUnit v3, Testcontainers, Respawn.

## Требования

- [.NET SDK 10.0.203](https://dotnet.microsoft.com/download) или новее в линейке 10.0 (`global.json`)
- Docker (для базы данных и для интеграционных тестов)

## Запуск

Все команды выполняются из корня репозитория.

```bash
# 1. База данных (PostgreSQL 17 на порту 5434)
docker compose up -d

# 2. Локальные инструменты (dotnet-ef фиксированной версии из dotnet-tools.json)
dotnet tool restore

# 3. Применить миграции
dotnet ef database update --project src/Teledok.Infrastructure.Postgres

# 4. Запустить API
dotnet run --project src/Teledok.Api
```

После запуска:

- документация API (Scalar): http://localhost:5244/scalar/v1
- спецификация OpenAPI: http://localhost:5244/openapi/v1.json

Документация доступна только в окружении `Development`. Профиль запуска задаёт его автоматически.

### Настройки

| Что | Где | По умолчанию |
|---|---|---|
| Строка подключения для API | `src/Teledok.Api/appsettings.Development.json`, ключ `ConnectionStrings:Postgres` | `Host=localhost;Port=5434;Database=teledok;Username=teledok;Password=teledok` |
| Строка подключения для `dotnet ef` | переменная окружения `ConnectionStrings__Postgres` | то же значение |
| Параметры контейнера | переменные `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_HOST_PORT` | `teledok` / `teledok` / `teledok` / `5434` |

Порт 5434 выбран, чтобы не конфликтовать с локально установленным PostgreSQL на 5432.

## Тесты

```bash
dotnet test --solution Teledok.slnx
```

- `Teledok.Domain.Tests` — модульные тесты домена (value objects, сущности, инварианты).
- `Teledok.Api.IntegrationTests` — HTTP-тесты всех эндпоинтов через `WebApplicationFactory`. Testcontainers поднимает **отдельный** контейнер PostgreSQL, поэтому нужен запущенный Docker. База из `docker compose` тестами не используется. Между тестами данные очищаются Respawn.

## API

Все ответы обёрнуты в единый конверт:

```json
{ "result": { ... }, "error": null, "isError": false, "timeGenerated": "2026-10-25T18:00:00Z" }
```

Ошибка возвращает соответствующий HTTP-статус и список сообщений с кодом и полем:

```json
{
  "result": null,
  "isError": true,
  "error": {
    "type": "VALIDATION",
    "messages": [
      { 
        "code": "inn.invalid.checksum", 
        "message": "Неверная контрольная сумма ИНН", 
        "invalidField": "inn" 
      }
    ]
  }
}
```

### Клиенты — `/api/clients`

| Метод | Путь | Описание |
|---|---|---|
| `POST` | `/api/clients` | Создать клиента. `type`: `LEGAL_ENTITY` или `INDIVIDUAL_ENTREPRENEUR`. Для ЮЛ можно сразу передать список `founders` |
| `GET` | `/api/clients` | Список: `page`, `pageSize` (1–100, по умолчанию 20), `type`, `search` (начало ИНН или часть наименования без учёта регистра) |
| `GET` | `/api/clients/{id}` | Клиент; для ЮЛ — вместе с учредителями |
| `PUT` | `/api/clients/{id}` | Изменить наименование |
| `DELETE` | `/api/clients/{id}` | Мягкое удаление |
| `POST` | `/api/clients/{id}/founders` | Добавить учредителя к ЮЛ |
| `DELETE` | `/api/clients/{id}/founders/{founderId}` | Отвязать учредителя от ЮЛ |

Учредителя можно указать двумя способами (и при создании ЮЛ, и при добавлении):

```json
{ "founderId": "0199..." }
```
```json
{ "inn": "500100732259", "lastName": "Иванов", "firstName": "Иван", "middleName": "Иванович" }
```

Во втором случае учредитель ищется по ИНН. Если найден и ФИО совпадает, привязывается существующий. Если ФИО другое, возвращается `409`: ФИО меняется через `PUT /api/founders/{id}`. Если не найден, создаётся новый.

### Учредители — `/api/founders`

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/founders` | Список: `page`, `pageSize`, `search` (начало ИНН или часть ФИО без учёта регистра) |
| `GET` | `/api/founders/{id}` | Учредитель и юрлица, в которых он состоит |
| `PUT` | `/api/founders/{id}` | Изменить ФИО |
| `DELETE` | `/api/founders/{id}` | Мягкое удаление; `409`, пока учредитель привязан хотя бы к одному ЮЛ |

## Бизнес-правила

- **ИНН** проверяется по длине и контрольной сумме ФНС. У ЮЛ 10 цифр, у ИП и учредителя 12. ИНН уникален среди неудалённых записей и не меняется после создания.
- **Учредитель** — только физическое лицо. Один учредитель может состоять в нескольких ЮЛ (связь многие-ко-многим). У ЮЛ может не быть учредителей, у ИП учредителей нет.
- **Удаление мягкое**: записи помечаются `is_deleted` и скрываются глобальным фильтром. После удаления ИНН можно использовать снова. При удалении ЮЛ его связи с учредителями тоже помечаются удалёнными, а сами учредители остаются.
- Даты создания и изменения проставляются автоматически, в UTC.

## Архитектура

Clean Architecture + DDD, срезы по операциям (команда или запрос + handler + валидатор в одной папке).

```
src/
├── Teledok.Api                      — контроллеры, Program.cs
├── Teledok.Application              — сценарии: Clients/, Founders/ → Commands/, Queries/
├── Teledok.Contracts                — DTO запросов и ответов
├── Teledok.Domain                   — сущности, value objects, доменные ошибки
├── Teledok.Infrastructure.Postgres  — EF Core: DbContext, конфигурации, миграции, репозитории
└── Shared/
    ├── Teledok.Shared.SharedKernel  — Error, Envelope, PageResult
    ├── Teledok.Shared.Core          — абстракции команд, запросов и транзакций
    └── Teledok.Shared.Framework     — Result → HTTP, обработка исключений, OpenAPI
tests/
├── Teledok.Domain.Tests
└── Teledok.Api.IntegrationTests
```

- **Ошибки без исключений.** Бизнес-ошибки возвращаются как `Result<T, Error>` и превращаются в HTTP-статус в одном месте (`Shared.Framework`). Исключения перехватывает глобальный обработчик и тоже отдаёт конверт.
- **Запись и чтение разделены.** Команды работают через репозитории и `ITransactionManager`. Запросы читают через `IReadDbContext` (`AsNoTracking`) с проекцией сразу в DTO.
- **Ошибки базы данных.** Нарушение уникального индекса (PostgreSQL `23505`) превращается в `409` с именем поля, нарушение внешнего ключа (`23503`) и конфликт конкурентности тоже дают понятную ошибку.
- **Строгая сборка.** `TreatWarningsAsErrors`, анализаторы .NET в режиме `Recommended`, единые версии пакетов (`Directory.Packages.props`), правила стиля в `.editorconfig`.

## Решения и отступления

- **Contracts ссылается на SharedKernel** — ради общего `PageResult<T>` в ответах списков. Обычно контракты ни от чего не зависят; здесь это осознанный компромисс вместо дублирования типа.
- **`citext` для наименования и ФИО** — поиск без учёта регистра обычным `Contains`, без `ToLower()` на стороне запроса и без функциональных индексов.
- **ИНН хранится через value converter**, а не как complex type: EF Core 10 не умеет строить индекс по свойству complex type, а уникальный индекс ИНН нужен. Из-за этого поиск по началу ИНН записан через `(string)(object)x.Inn` — EF не переводит в SQL обращение к членам сконвертированного свойства ([dotnet/efcore#10434](https://github.com/dotnet/efcore/issues/10434)). Каст изолирован в `*QueryExtensions.WhereMatches`.
- **Отключённые правила анализаторов** (`.globalconfig`, у каждого указана причина): CA1707 — константы и значения enum в `UPPER_CASE`; CA1716 — пространство имён `Shared`; CA1000 — статические фабрики `Envelope<T>`.
- **Успешный `DELETE` возвращает `200` с конвертом**, а не `204`, чтобы формат всех ответов был единым.
- **Контроллеры**, а не Minimal API — по условию задания (без scaffold).

## Известные ограничения

- **Конкурентные изменения** не отслеживаются: токена версии (например, `xmin`) нет, при одновременном `PUT` побеждает последний.
- **Удаление учредителя** проверяет активные связи отдельным запросом. Если в тот же момент другой запрос привязывает этого учредителя к ЮЛ, проверка может не увидеть новую связь. Полностью закрывается блокировкой строки или проверкой на уровне базы.
- Аутентификации и авторизации нет.

## Возможное развитие

- **Учредитель — юридическое лицо.** Сейчас учредитель только физлицо. Для ЮЛ-учредителей связь должна ссылаться на `Client` (полиморфный участник), плюс понадобится защита от циклов и владения самим собой.
- Токен конкурентности и `ETag` / `If-Match` для `PUT`.
- Контейнер API в `docker-compose.yml` для запуска одной командой.
