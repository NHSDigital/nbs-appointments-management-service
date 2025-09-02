# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src/data/BookingsDataExtracts

COPY . ./

RUN dotnet restore "./BookingsDataExtracts.csproj"

WORKDIR "/src/data/BookingsDataExtracts"
RUN dotnet build "./BookingsDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/BookingsDataExtracts


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

WORKDIR "/src/data/BookingsDataExtracts"
RUN dotnet publish "./BookingsDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/bookings /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/bookings ./BookingsDataExtracts

ENTRYPOINT ["dotnet", "BookingsDataExtracts.dll"]