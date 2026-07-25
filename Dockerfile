FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ThisisczApi.csproj", "./"]
RUN dotnet restore "ThisisczApi.csproj"

COPY . .
RUN dotnet publish "ThisisczApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Fly.io / 多数 PaaS 默认 8080；若平台注入 PORT（如 Render）则优先用 PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet ThisisczApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
