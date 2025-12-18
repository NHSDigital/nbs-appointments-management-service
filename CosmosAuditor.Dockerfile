FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /App

COPY ./src/jobs/Nhs.Appointments.Jobs.BlobAuditor  ./src/jobs/Nhs.Appointments.Jobs.BlobAuditor
COPY ./src/api/Nhs.Appointments.Core/ ./src/api/Nhs.Appointments.Core/
COPY ./src/api/Nhs.Appointments.Api/ ./src/api/Nhs.Appointments.Api/
COPY ./src/api/Nhs.Appointments.Audit ./src/api/Nhs.Appointments.Audit
COPY ./src/api/Nhs.Appointments.Persistance ./src/api/Nhs.Appointments.Persistance

ARG DOTNET_CONFIGURATION=Release

RUN dotnet restore src/jobs/Nhs.Appointments.Jobs.BlobAuditor/Nhs.Appointments.Jobs.BlobAuditor.csproj
RUN dotnet publish src/jobs/Nhs.Appointments.Jobs.BlobAuditor/Nhs.Appointments.Jobs.BlobAuditor.csproj \
    -c $DOTNET_CONFIGURATION \
    -o out \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "Nhs.Appointments.Jobs.BlobAuditor.dll"]
