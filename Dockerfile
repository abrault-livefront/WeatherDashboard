# syntax=docker/dockerfile:1

# Build stage: .NET SDK with Azure Linux 3.0
FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0@sha256:f54a6d48372083f14142bab3182c89a306884ee7c6908e4b798f6337aa2e0d57 AS build

# Node.js version (override with --build-arg NODE_VERSION=x.x.x)
ARG NODE_VERSION=24.11.1

ENV NVM_DIR="/root/.nvm"
ENV NODE_VERSION=${NODE_VERSION}

# Install Node.js via NVM and enable corepack for pnpm
RUN set -x \
    && tdnf -y update \
    && tdnf -y install awk bash curl ca-certificates tar \
    && curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash \
    && . "$NVM_DIR/nvm.sh" \
    && nvm install $NODE_VERSION \
    && nvm use $NODE_VERSION \
    && corepack enable \
    && tdnf clean all

WORKDIR /src

# Copy solution configuration (build props, packages, NuGet config)
COPY Directory.Build.props Directory.Build.props
COPY Directory.Packages.props Directory.Packages.props
COPY nuget.config .
COPY global.json .
COPY *.slnx .

# Copy project files for layer caching
# Note: packages.lock.json excluded due to .NET 10 SDK issue with Microsoft.AspNetCore.App.Internal.Assets
COPY src/WeatherDashboard.Application/*.csproj \
     src/WeatherDashboard.Application/

COPY src/WeatherDashboard.Domain/*.csproj \
     src/WeatherDashboard.Domain/

COPY src/WeatherDashboard.Infrastructure/*.csproj \
     src/WeatherDashboard.Infrastructure/

COPY src/WeatherDashboard.Web/*.csproj \
     src/WeatherDashboard.Web/

# Restore NuGet packages (Web project only, avoids needing test projects)
RUN dotnet restore src/WeatherDashboard.Web/WeatherDashboard.Web.csproj

# Copy package files for layer caching
COPY src/WeatherDashboard.Web/package.json \
     src/WeatherDashboard.Web/pnpm-lock.yaml \
     src/WeatherDashboard.Web/pnpm-workspace.yaml \
     src/WeatherDashboard.Web/

# Install pnpm dependencies with frozen lockfile
RUN . "$NVM_DIR/nvm.sh" && CI=true pnpm -C src/WeatherDashboard.Web install --frozen-lockfile

# Copy all remaining source files
COPY . .

# Build CSS and publish application
RUN . "$NVM_DIR/nvm.sh" && CI=true pnpm -C src/WeatherDashboard.Web run css:build \
    && dotnet publish src/WeatherDashboard.Web/WeatherDashboard.Web.csproj -c Release -o /src/artifacts --self-contained false

# Final stage: Minimal distroless runtime for production
FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0-distroless-composite-extra@sha256:d4c7ee56f9e780f9a35d16946de921b5f310a570e38ee64ae73f74b42da60d79 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Copy artifacts with non-root ownership
COPY --chown=$APP_UID:$APP_UID --from=build /src/artifacts/ .

USER $APP_UID

# Note: Distroless image lacks shell utilities for HEALTHCHECK
# Configure health checks at orchestration layer (Docker Compose, Kubernetes, etc.)

ENTRYPOINT ["dotnet", "WeatherDashboard.Web.dll"]
