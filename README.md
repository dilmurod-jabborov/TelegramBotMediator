# TelegramBotMediator

Production-ready Telegram mediator bot on `.NET 8` with `Telegram.Bot`, clean architecture, EF Core (SQLite/PostgreSQL), retry policies, and structured logging.

## Project structure

- `TelegramBotMediator.Domain` - entities and enums
- `TelegramBotMediator.Application` - service/repository abstractions and business logic
- `TelegramBotMediator.Infrastructure` - EF Core context, repositories, migrations
- `TelegramBotMediator.Presentation` - Telegram handlers, polling host, configuration

## Features

- Step-by-step user registration (`/start`)
  - First Name
  - Last Name
  - Phone Number (Telegram contact button)
  - Address
- User storage with SQLite or PostgreSQL
- DB-backed user session/state persistence (survives restarts)
- Per-user state tracking:
  - `None`
  - `WaitingForFirstName`
  - `WaitingForLastName`
  - `WaitingForPhone`
  - `WaitingForAddress`
  - `Registered`
- User to admin message relay with user info header
- Admin reply routing back to exact user using message mapping table
- Admin-only commands:
  - `/users` - list registered users
  - `/stats` - total/new/banned stats
  - `/user <id>` - show full user details
  - `/ban <telegramId>` - block user messaging
  - `/unban <telegramId>` - unblock user
  - `/broadcast <message>` - send message to all users
- Relay mapping cleanup background job (removes old rows)
- Rate limiting for user messages to reduce spam
- Supports both polling mode and webhook mode

## Configuration

Edit `TelegramBotMediator.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=telegram-bot.db"
  },
  "BotSettings": {
    "Token": "PUT_YOUR_BOT_TOKEN_HERE",
    "AdminTelegramId": 123456789,
    "UseWebhook": false,
    "WebhookUrl": "https://your-service.onrender.com",
    "WebhookPath": "/telegram/webhook",
    "UserMessageRateLimitMs": 800
  }
}
```

You can copy from template:

```bash
cp TelegramBotMediator.Presentation/appsettings.Example.json TelegramBotMediator.Presentation/appsettings.json
```

## Database and migrations

Initial migration is already added in:

- `TelegramBotMediator.Infrastructure/Data/Migrations`

To create a new migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project TelegramBotMediator.Infrastructure/TelegramBotMediator.Infrastructure.csproj \
  --startup-project TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj \
  --output-dir Data/Migrations
```

To apply migrations manually:

```bash
dotnet ef database update \
  --project TelegramBotMediator.Infrastructure/TelegramBotMediator.Infrastructure.csproj \
  --startup-project TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj
```

Note: app startup also runs `Database.Migrate()` automatically.

## Run

```bash
dotnet restore
dotnet build TelegramBotMediator.sln
dotnet run --project TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj
```

## Render deploy

This repository includes `Dockerfile` and `render.yaml` for quick Render deployment.

1. Push changes to GitHub.
2. In Render, create a new **Blueprint** service from this repo (it reads `render.yaml`).
3. Set secret env vars:
   - `BotSettings__Token` = your current bot token
   - `ConnectionStrings__DefaultConnection` = your PostgreSQL connection string
     (example: `Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true`)

Recommended: create a free PostgreSQL database (for example Supabase/Neon/Render Postgres) and paste its connection string into `ConnectionStrings__DefaultConnection`.

## How relay works

1. Registered user sends any message.
2. Bot sends admin message in format:

```
User: {FullName}
Phone: {Phone}
Address: {Address}

Message:
{UserMessage}
```

3. Bot stores `(forwarded_message_id, user_telegram_id)` in `MessageRelayMaps`.
4. Admin replies to that bot message.
5. Bot detects `ReplyToMessage.MessageId`, finds mapped user, and delivers admin text back.
