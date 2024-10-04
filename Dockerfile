# Base image for the ASP.NET Core application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Switch to root user for installing dependencies
USER root

# Install Chrome dependencies
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    curl \
    gnupg \
    ca-certificates \
    fonts-liberation \
    libappindicator3-1 \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcups2 \
    libdbus-1-3 \
    libxcomposite1 \
    libxrandr2 \
    libxss1 \
    xdg-utils \
    libnss3 \
    libnspr4 \
    libxshmfence1 \
    libgbm-dev \
    libxdamage-dev \
    libxshmfence1 \
    libu2f-udev \
    libu2f-host \
    libgconf-2-4 \
    libpango1.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

# Install Chrome browser
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list' \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

# Install ChromeDriver
RUN wget -O /tmp/chromedriver.zip https://chromedriver.storage.googleapis.com/117.0.5938.62/chromedriver_linux64.zip \
    && unzip /tmp/chromedriver.zip -d /usr/local/bin/ \
    && rm /tmp/chromedriver.zip

# Switch back to non-root user
USER $APP_UID

# Build stage for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ScrapeTime.Presentation/ScrapeTime.Presentation.csproj", "ScrapeTime.Presentation/"]
RUN dotnet restore "ScrapeTime.Presentation/ScrapeTime.Presentation.csproj"
COPY . .
WORKDIR "/src/ScrapeTime.Presentation"
RUN dotnet build "ScrapeTime.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ScrapeTime.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - Running the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for ChromeDriver
ENV CHROME_BIN=/usr/bin/google-chrome \
    CHROME_DRIVER=/usr/local/bin/chromedriver

ENTRYPOINT ["dotnet", "ScrapeTime.Presentation.dll"]
