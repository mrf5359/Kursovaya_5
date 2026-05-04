# 1. Используем официальный образ .NET SDK для сборки проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Копируем файлы проектов (Сервер и Shared) и восстанавливаем зависимости
COPY ["LogisticsApp.Server/LogisticsApp.Server.csproj", "LogisticsApp.Server/"]
COPY ["LogisticsApp.Shared/LogisticsApp.Shared.csproj", "LogisticsApp.Shared/"]
RUN dotnet restore "LogisticsApp.Server/LogisticsApp.Server.csproj"

# 3. Копируем весь остальной код и компилируем
COPY . .
WORKDIR "/src/LogisticsApp.Server"
RUN dotnet publish "LogisticsApp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Берем легкий образ ASP.NET только для запуска (чтобы контейнер весил мало)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 5. Открываем порт 8080 для нашего gRPC сервера
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 6. Команда запуска
ENTRYPOINT ["dotnet", "LogisticsApp.Server.dll"]