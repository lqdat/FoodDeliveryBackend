# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["FoodDeliveryBackend.sln", "./"]
COPY ["FoodDeliveryBackend.API/FoodDeliveryBackend.API.csproj", "FoodDeliveryBackend.API/"]
COPY ["FoodDeliveryBackend.Core/FoodDeliveryBackend.Core.csproj", "FoodDeliveryBackend.Core/"]
COPY ["FoodDeliveryBackend.Infrastructure/FoodDeliveryBackend.Infrastructure.csproj", "FoodDeliveryBackend.Infrastructure/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the API project
WORKDIR "/src/FoodDeliveryBackend.API"
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Railway or standard)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FoodDeliveryBackend.API.dll"]
