FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/BuildingBlocks/"]
COPY ["infra/MarketSpace.ServiceDefaults/MarketSpace.ServiceDefaults.csproj", "infra/MarketSpace.ServiceDefaults/"]
COPY ["src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj", "src/Edges/BackendForFrontend.Api/"]
RUN dotnet restore "src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj"
COPY . .
RUN dotnet build "src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackendForFrontend.Api.dll"]
