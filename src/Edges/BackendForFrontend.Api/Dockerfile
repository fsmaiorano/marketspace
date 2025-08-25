FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/BuildingBlocks/"]
COPY ["src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj", "src/Edges/BackendForFrontend.Api/"]
RUN dotnet restore "src/Edges/BackendForFrontend.Api/BackendForFrontend.Api.csproj"
COPY . .
WORKDIR "/src/src/Edges/BackendForFrontend.Api"
RUN dotnet build "./BackendForFrontend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BackendForFrontend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackendForFrontend.Api.dll"]
