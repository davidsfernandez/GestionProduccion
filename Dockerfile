# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all projects and restore
COPY ["GestionProduccion.sln", "./"]
COPY ["GestionProduccion.csproj", "./"]
COPY ["Client/GestionProduccion.Client/GestionProduccion.Client.csproj", "Client/GestionProduccion.Client/"]
COPY ["GestionProduccion.Tests/GestionProduccion.Tests.csproj", "GestionProduccion.Tests/"]

RUN dotnet restore

# Copy everything else
COPY . .

# Run tests (optional but recommended during build)
# RUN dotnet test "GestionProduccion.Tests/GestionProduccion.Tests.csproj" -c Release

# Build the main project
RUN dotnet build "GestionProduccion.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "GestionProduccion.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables for Docker
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "GestionProduccion.dll"]
