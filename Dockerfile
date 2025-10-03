# 使用官方 .NET 8.0 runtime 作為基礎映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 使用 SDK 映像來建置應用程式
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyFirstSKApp.csproj", "."]
RUN dotnet restore "./MyFirstSKApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MyFirstSKApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyFirstSKApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 最終階段/映像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 設定環境變數
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "MyFirstSKApp.dll"]