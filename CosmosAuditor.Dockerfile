FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /App

COPY ./src/jobs/Nhs.Appointments.Jobs.BlobAuditor  ./src/jobs/Nhs.Appointments.Jobs.BlobAuditor
COPY ./src/api/Nhs.Appointments.Core/ ./src/api/Nhs.Appointments.Core/

RUN dotnet restore src/jobs/Nhs.Appointments.Jobs.BlobAuditor/Nhs.Appointments.Jobs.BlobAuditor.csproj
RUN dotnet publish src/jobs/Nhs.Appointments.Jobs.BlobAuditor/Nhs.Appointments.Jobs.BlobAuditor.csproj -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "Nhs.Appointments.Jobs.BlobAuditor.dll"]
