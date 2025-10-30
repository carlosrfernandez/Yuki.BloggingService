FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Yuki.BloggingService/Yuki.BloggingService.csproj", "Yuki.BloggingService/"]
RUN dotnet restore "Yuki.BloggingService/Yuki.BloggingService.csproj"
COPY . .
WORKDIR "/src/Yuki.BloggingService"
RUN dotnet build "./Yuki.BloggingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yuki.BloggingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuki.BloggingService.dll"]
