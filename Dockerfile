# Этап 1: Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проектов и восстанавливаем зависимости
COPY ["GPQ/GPQ.csproj", "GPQ/"]
COPY ["GPQ.Client/GPQ.Client.csproj", "GPQ.Client/"]
RUN dotnet restore "GPQ/GPQ.csproj"

# Копируем исходный код и собираем релизную версию
COPY . .
WORKDIR "/src/GPQ"
RUN dotnet publish "GPQ.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 2: Запуск приложения
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 80

# Настраиваем ASP.NET Core на порт 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GPQ.dll"]
