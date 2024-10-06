# Base image for the ASP.NET Core application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Chrome dependencies and clean up APT cache after installation
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    ca-certificates \
    fonts-liberation \
    libappindicator3-1 \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcups2 \
    libdbus-1-3 \
    libnspr4 \
    libnss3 \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    xdg-utils \
    libcurl3-gnutls \
    libdrm2 \
    libgbm1 \
    libvulkan1 \
    --no-install-recommends && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install Chrome and clean up after installation
RUN wget -q -O /tmp/chromedriver.zip http://chromedriver.storage.googleapis.com/114.0.5735.90/chromedriver_linux64.zip && \
    unzip /tmp/chromedriver.zip -d /usr/local/bin/ && \
    rm /tmp/chromedriver.zip && \
    wget -q -O /tmp/chrome.deb https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb && \
    dpkg -i /tmp/chrome.deb || apt-get -f install -y && \
    rm /tmp/chrome.deb

# Verify installation of Chrome and ChromeDriver
RUN which google-chrome
RUN which chromedriver

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

# Explicitly set timezone to avoid locale issues in certain scraping scenarios
RUN ln -sf /usr/share/zoneinfo/Etc/UTC /etc/localtime

# Set the entry point for the application
ENTRYPOINT ["dotnet", "ScrapeTime.Presentation.dll"]
