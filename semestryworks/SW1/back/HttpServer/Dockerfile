# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем csproj и восстанавливаем зависимости
COPY HttpServer/HttpServer.csproj HttpServer/
RUN dotnet restore HttpServer/HttpServer.csproj

# копируем остальной код и собираем
COPY . .
RUN dotnet publish HttpServer/HttpServer.csproj -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENV APP_ENV=Docker

COPY HttpServer/settings.docker.json ./settings.docker.json
COPY HttpServer/static ./app/static
# если сервер слушает 1234
EXPOSE 1234

ENTRYPOINT ["dotnet", "HttpServer.dll"]
