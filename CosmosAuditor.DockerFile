FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /App

COPY ./data/CosmosAuditor ./data/CosmosAuditor
COPY ./src/api/Nhs.Appointments.Core/ ./src/api/Nhs.Appointments.Core/

RUN dotnet restore data/CosmosAuditor/CosmosAuditor.csproj

RUN dotnet publish data/CosmosAuditor/CosmosAuditor.csproj -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "CosmosAuditor.dll"]