FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["deployable/EmissionService/EmissionService.csproj", "deployable/EmissionService/"]
COPY ["shared/Domain/Domain.csproj", "shared/Domain/"]
COPY ["shared/Messages/Messages.csproj", "shared/Messages/"]
COPY ["library/Context/Context.csproj", "library/Context/"]
COPY ["library/Monitoring/Monitoring.csproj", "library/Monitoring/"]
RUN dotnet restore "deployable/EmissionService/EmissionService.csproj"
COPY . .
WORKDIR "/src/deployable/EmissionService"
RUN dotnet build "EmissionService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EmissionService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmissionService.dll"]
