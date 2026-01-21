# STAGE 1: BUILD
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia tudo
COPY . .

# Restaura dependências
RUN dotnet restore "./fcg-NotificationsAPI/fcg-NotificationsAPI.csproj"

# Build em modo Release
RUN mkdir -p /app/build
RUN dotnet build "./fcg-NotificationsAPI/fcg-NotificationsAPI.csproj" -c Release -o /app/build

# STAGE 2: PUBLISH
FROM build AS publish
RUN dotnet publish "./fcg-NotificationsAPI/fcg-NotificationsAPI.csproj" -c Release -o /app/publish

# STAGE 3: RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app


COPY --from=publish /app/publish .
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081
ENTRYPOINT ["dotnet", "fcg-NotificationsAPI.dll"]
