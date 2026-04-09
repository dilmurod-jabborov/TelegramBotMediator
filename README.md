# TelegramBotMediator

Production-ready Telegram mediator bot on `.NET 8` with `Telegram.Bot`, clean architecture, and SQLite + EF Core.

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
- SQLite user storage
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
  - `/broadcast <message>` - send message to all users

## Configuration

Edit `TelegramBotMediator.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=telegram-bot.db"
  },
  "BotSettings": {
    "Token": "PUT_YOUR_BOT_TOKEN_HERE",
    "AdminTelegramId": 123456789
  }
}
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
3. Set secret env var:
   - `BotSettings__Token` = your current bot token
4. Keep disk mount at `/var/data` (already defined in `render.yaml`).

The bot will use SQLite file at `/var/data/telegram-bot.db`.

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
