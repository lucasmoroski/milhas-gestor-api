# API Milhas (ASP.NET Core / .NET 10) - imagem pensada para hospedagem gratuita (Render, Koyeb, etc.)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MilhasApi.Api/MilhasApi.Api.csproj MilhasApi.Api/
RUN dotnet restore MilhasApi.Api/MilhasApi.Api.csproj

COPY . .
RUN dotnet publish MilhasApi.Api/MilhasApi.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production \
    Database__Provider=Postgres \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=0

EXPOSE 8080
# A plataforma injeta $PORT; localmente cai em 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet MilhasApi.Api.dll"]
