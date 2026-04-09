FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TelegramBotMediator.sln", "."]
COPY ["TelegramBotMediator.Domain/TelegramBotMediator.Domain.csproj", "TelegramBotMediator.Domain/"]
COPY ["TelegramBotMediator.Application/TelegramBotMediator.Application.csproj", "TelegramBotMediator.Application/"]
COPY ["TelegramBotMediator.Infrastructure/TelegramBotMediator.Infrastructure.csproj", "TelegramBotMediator.Infrastructure/"]
COPY ["TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj", "TelegramBotMediator.Presentation/"]
RUN dotnet restore "TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj"

COPY . .
RUN dotnet publish "TelegramBotMediator.Presentation/TelegramBotMediator.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TelegramBotMediator.Presentation.dll"]
