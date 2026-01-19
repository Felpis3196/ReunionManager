FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SmartMeetingManager.sln ./
COPY src/SmartMeetingManager.API/SmartMeetingManager.API.csproj src/SmartMeetingManager.API/
COPY src/SmartMeetingManager.Application/SmartMeetingManager.Application.csproj src/SmartMeetingManager.Application/
COPY src/SmartMeetingManager.Domain/SmartMeetingManager.Domain.csproj src/SmartMeetingManager.Domain/
COPY src/SmartMeetingManager.Infrastructure/SmartMeetingManager.Infrastructure.csproj src/SmartMeetingManager.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY src/ ./src/
WORKDIR /src/src/SmartMeetingManager.API
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install PostgreSQL client for health checks
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=publish /app/publish .

# Create entrypoint script
RUN echo '#!/bin/sh' > /app/entrypoint.sh && \
    echo 'set -e' >> /app/entrypoint.sh && \
    echo 'echo "Waiting for PostgreSQL..."' >> /app/entrypoint.sh && \
    echo 'until pg_isready -h postgres -U postgres; do sleep 1; done' >> /app/entrypoint.sh && \
    echo 'echo "PostgreSQL is ready"' >> /app/entrypoint.sh && \
    echo 'echo "Starting application..."' >> /app/entrypoint.sh && \
    echo 'exec dotnet SmartMeetingManager.API.dll' >> /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
