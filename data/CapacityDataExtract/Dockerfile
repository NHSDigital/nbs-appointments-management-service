# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM alpine

RUN addgroup -S nonroot \
    && adduser -S nonroot -G nonroot

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["data/CapacityDataExtract/CapacityDataExtracts.csproj", "data/CapacityDataExtract/"]
COPY ["data/DataExtract/DataExtract.csproj", "data/DataExtract/"]
COPY ["src/api/Nhs.Appointments.Api/Nhs.Appointments.Api.csproj", "src/api/Nhs.Appointments.Api/"]
COPY ["src/api/Nhs.Appointments.Audit/Nhs.Appointments.Audit.csproj", "src/api/Nhs.Appointments.Audit/"]
COPY ["src/api/Nhs.Appointments.Core/Nhs.Appointments.Core.csproj", "src/api/Nhs.Appointments.Core/"]
COPY ["src/api/Nhs.Appointments.Persistance/Nhs.Appointments.Persistance.csproj", "src/api/Nhs.Appointments.Persistance/"]
COPY ["src/libraries/Nbs.MeshClient/Nbs.MeshClient.csproj", "src/libraries/Nbs.MeshClient/"]

RUN dotnet restore "./data/CapacityDataExtract/CapacityDataExtracts.csproj"

COPY . .

WORKDIR "/src/data/CapacityDataExtract"
RUN dotnet build "./CapacityDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CapacityDataExtracts.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER nonroot

ENTRYPOINT ["dotnet", "CapacityDataExtracts.dll"]
