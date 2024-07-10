# nbs-appointments-management-service

## Introduction

An appointment and booking api designed to support national vaccination bookings.

[Product Backlog](https://nhsd-jira.digital.nhs.uk/secure/RapidBoard.jspa?rapidView=7622&projectKey=APPT&view=planning.nodetail&selectedIssue=APPT-26&issueLimit=100#)

[Slack chanel - #appointments-service](https://nhsdigitalcorporate.enterprise.slack.com/archives/C062RBW4NF4)

## Team

- Sponsor - James Spirit
- Product Owner - Lauren Caveney
- Development Team - Vincent Crowe, Joe Farina, Kim Crowe, Khurram Aziz, Saritha Lakkreddy

### Technologies

- dotnet (v8)
- C#
- Azure Functions
- Cosmos document database

### Project Goal

We will build an Appointments API using

- Will follow REST principles as much as possible
- Azure Devops (for code repo and pipelines)
- C# (development language)
- Azure Functions (as a platform for hosting)

We will build a web based application for appointment and site management

- We will use React/Next.js
- Github (for code repo)
- Azure Devops (pipelines)

## GitHub Repo and signed commits

The repo has signed commits enabled. This means that only commits that are verified can be merged into the main branch. More information about signed commits can be found  [here](https://docs.github.com/en/authentication/managing-commit-signature-verification/about-commit-signature-verification).

Your workstation will need to be set up to use signed commits. Please follow this guide for [instructions](https://github.com/NHSDigital/software-engineering-quality-framework/blob/main/practices/guides/commit-signing.md).

## Running Locally

### Setup

- Install Docker for Windows (use Linux
  Containers) [instructions](https://docs.docker.com/desktop/install/windows-install/)
- Install .NET 6.0 [instructions](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60)
- Install the Azure Functions Core
  Tools [instructions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)
- Install NPM [instructions](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)

### Running locally

Each of the following steps needs to be done in a separate terminal window

- Run the Cosmos Emulator and MockApi
  - From the root folder run `docker compose up`
- Run the API
  - From the folder `/src/api/Nhs.Appointments.Api` run the command `func start`
- Run the Web Application
  - From the folder `/src/client/` run `npm install` and then run `npm run dev`

### Setting a cert for the OIDC server

To run the oidc-server you will need to install a certificate. You will need to generate
and trust a dev cert with the following command (in order to ensure that the certificate
is placed in the correct location you should run the command from your user profile folder
or amend the command accordingly).

`dotnet dev-certs https -ep .aspnet/https/aspnetapp.pfx -p password -t -v`

### Notes about configuration

The API expects cosmos to be running at "https://localhost:8081"
The API expects the mockapi to be running at "http://localhost:4011"
The Web Application expects the API to be running at "http://localhost:7071"

If your configuration causes any of these to be running differently then you will need to change the application
configuration files accordingly. (Or use environment variables to override local configuration)

API configuration file is found at `/src/Nhs.Appointments.Api/localsettings.json`
Client configuration file is found at `/src/client/src/configuration.ts`

### Exploring the Cosmos Database

When running locally in docker the data in the cosmos database can be viewed and modified using the cosmos emulator
explorer. To access the explorer visit `https://localhost:8081/_explorer/index.html` (not you may get an unsafe page
warning unless you import the certificate from the cosmos container, this can safely be ignored for this url)

### Adding roles and permissions

Add a document with the following structure in to the index_data container. This sets up the permissions that each role
contains so that the UI can react to a user's assigned permissions. This will have to be done once the function has started
(so that the containers have been created) but before you can login. If you restart your cosmos database you will need to
insert the document again.

```json
{
    "id": "global_roles",
    "docType": "roles",
    "roles": [
        {
            "id": "canned:check-in",
            "name": "Check-in",
            "permissions": [
                "booking:query",
                "booking:set-status"
            ]
        },
        {
            "id": "canned:appointment-manager",
            "name": "Appointment manager",
            "permissions": [
                "booking:make",
                "booking:query",
                "booking:cancel"
            ]
        },
        {
            "id": "canned:availability-manager",
            "name": "Availability manager",
            "permissions": [
                "availability:get-setup",
                "availability:set-setup",
                "availability:query"
            ]
        },
        {
            "id": "canned:site-configuration-manager",
            "name": "Site configuration manager",
            "permissions": [
                "site:get-config",
                "site:set-config"
            ]
        },
        {
            "id": "canned:api-user",
            "name": "Api User",
            "permissions": [
                "site:get-meta-data",
                "availability:query",
                "booking:make",
                "booking:query",
                "booking:cancel"
            ]
        }
    ]
}
```

### Adding in User Site Assignments

Add another document to represent a user with the following structure in to the index_data container. Each user is represented by a single document which contains the id of the user and all the roles that are assigned to the user at each site. Role assignments are site or global scoped - site scoped means that the user is assigned that role at the given site; global scope means that the user has been assigned that role and is valid for all sites. 

This will have to be done once the function has started (so that the containers have been created) but before you can login. If you restart your cosmos database you will need to insert the document again.

```json
{
  "id": "cc.agent@nhs.uk",
  "docType": "user",
  "roleAssignments": [
    {
      "role": "canned:site-configuration-manager",
      "scope": "site:1000"
    },
    {
      "role": "canned:check-in",
      "scope": "site:1001"
    },
    {
      "role": "canned:availability-manager",
      "scope": "site:1001"
    }
  ]
}
```

Example of ApiUser with global scoped role:
```json
{
  "id": "ApiUser",
  "docType": "user",
  "roleAssignments": [
    {
      "role": "canned:api-user",
      "scope": "global"
    }
  ]
}
```