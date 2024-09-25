FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ScrapeTime.Presentation/ScrapeTime.Presentation.csproj", "ScrapeTime.Presentation/"]
COPY ["ScrapeTime.Domain/ScrapeTime.Domain.csproj", "ScrapeTime.Domain/"]
RUN dotnet restore "ScrapeTime.Presentation/ScrapeTime.Presentation.csproj"
COPY . .
WORKDIR "/src/ScrapeTime.Presentation"
RUN dotnet build "ScrapeTime.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ScrapeTime.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScrapeTime.Presentation.dll"]
