FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

WORKDIR /src/api
COPY src/api/Nhs.Appointments.Api/ ./Nhs.Appointments.Api/
COPY src/api/Nhs.Appointments.Core/ ./Nhs.Appointments.Core/
COPY src/api/Nhs.Appointments.Persistance/ ./Nhs.Appointments.Persistance/

RUN dotnet publish Nhs.Appointments.Api\
    --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]