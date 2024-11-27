# Base stage for fast mode (Linux-based)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create the subfolder structure and set permissions
RUN mkdir -p /app/wwwroot/uploads/backlog \
    && chmod -R 755 /app/wwwroot

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["worklog-api.csproj", "."]
RUN dotnet restore "./worklog-api.csproj"
COPY . . 
WORKDIR "/src/."
RUN dotnet build "./worklog-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./worklog-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish ./

# Create and copy .env file dynamically
ARG DB_CONNECTION_STRING
RUN echo "DB_CONNECTION_STRING=${DB_CONNECTION_STRING}" > /app/.env

# Ensure static files and subfolder exist
RUN mkdir -p /app/wwwroot/uploads/backlog \
    && chmod -R 755 /app/wwwroot

# Set the entry point
ENTRYPOINT ["dotnet", "worklog-api.dll"]

# Expose application ports (matching launchSettings.json)
EXPOSE 8080
EXPOSE 8081
