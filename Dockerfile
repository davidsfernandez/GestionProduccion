# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker cache
COPY ["GestionProduccion.sln", "./"]
COPY ["GestionProduccion.csproj", "./"]
COPY ["GestionProduccion.Shared/GestionProduccion.Shared.csproj", "GestionProduccion.Shared/"]
COPY ["Client/GestionProduccion.Client/GestionProduccion.Client.csproj", "Client/GestionProduccion.Client/"]
COPY ["GestionProduccion.Tests/GestionProduccion.Tests.csproj", "GestionProduccion.Tests/"]

# Restore all projects
RUN dotnet restore "GestionProduccion.sln"

# Copy the rest of the source code
COPY . .

# Build the main project (Server)
RUN dotnet build "GestionProduccion.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "GestionProduccion.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
EXPOSE 443

# Copy published files from build stage
COPY --from=publish /app/publish .

# Create directory for avatar uploads and set permissions
RUN mkdir -p /app/wwwroot/img/avatars && chmod -R 755 /app/wwwroot/img/avatars

# Install curl for healthchecks (optional but recommended)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Set entrypoint
ENTRYPOINT ["dotnet", "GestionProduccion.dll"]
