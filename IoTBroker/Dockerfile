# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files
COPY ["IoTBroker/IoTBroker.csproj", "IoTBroker/"]

# Restore of dependencies
RUN dotnet restore "IoTBroker/IoTBroker.csproj"

# Copy the rest of the code
COPY . .

# Build & Publish
WORKDIR "/src/IoTBroker"
RUN dotnet publish "IoTBroker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IoTBroker.dll"]