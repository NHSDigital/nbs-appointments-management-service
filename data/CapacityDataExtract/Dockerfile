# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src/data/CapacityDataExtract/

COPY . ./

RUN dotnet restore "./CapacityDataExtracts.csproj"

WORKDIR "/src/data/CapacityDataExtract"
RUN dotnet build "./CapacityDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/CapacityDataExtracts

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

WORKDIR "/src/data/CapacityDataExtract"
RUN dotnet publish "./CapacityDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/capacity /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/capacity ./CapacityDataExtracts

ENTRYPOINT ["dotnet", "CapacityDataExtracts.dll"]