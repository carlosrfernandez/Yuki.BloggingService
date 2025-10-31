FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Yuki.BloggingService.Domain/Yuki.BloggingService.Domain.csproj", "Yuki.BloggingService.Domain/"]
COPY ["Yuki.BloggingService.Domain.Common/Yuki.BloggingService.Domain.Common.csproj", "Yuki.BloggingService.Domain.Common/"]
COPY ["Yuki.BloggingService.Application/Yuki.BloggingService.Application.csproj", "Yuki.BloggingService.Application/"]
COPY ["Yuki.BloggingService.Infrastructure/Yuki.BloggingService.Infrastructure.csproj", "Yuki.BloggingService.Infrastructure/"] 
COPY ["Yuki.BloggingService.Queries/Yuki.BloggingService.Queries.csproj", "Yuki.BloggingService.Queries/"]
COPY ["Yuki.BloggingService.Api/Yuki.BloggingService.Api.csproj", "Yuki.BloggingService.Api/"]
RUN dotnet restore "Yuki.BloggingService.Api/Yuki.BloggingService.Api.csproj"
COPY . .
WORKDIR "/src/Yuki.BloggingService.Api"
RUN dotnet build "./Yuki.BloggingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yuki.BloggingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuki.BloggingService.Api.dll"]
