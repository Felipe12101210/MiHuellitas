# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar y compilar solo la web
COPY ["MiHuellitas.web/MiHuellitas.web.csproj", "MiHuellitas.web/"]
RUN dotnet restore "MiHuellitas.web/MiHuellitas.web.csproj"

COPY . .
RUN dotnet build "MiHuellitas.web/MiHuellitas.web.csproj" -c Release -o /app/build

RUN dotnet publish "MiHuellitas.web/MiHuellitas.web.csproj" -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiHuellitas.web.dll"]
