FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ThisisczApi.csproj", "./"]
RUN dotnet restore "ThisisczApi.csproj"

COPY . .
RUN dotnet publish "ThisisczApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render will inject PORT at runtime.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet ThisisczApi.dll --urls http://0.0.0.0:${PORT:-10000}"]
