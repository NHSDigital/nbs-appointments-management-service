# nbs-appointments-management-service

## Introduction

An appointment and booking api designed to support national vaccination bookings.

[Product Backlog](https://nhsd-jira.digital.nhs.uk/secure/RapidBoard.jspa?rapidView=7622&projectKey=APPT&view=planning.nodetail&selectedIssue=APPT-26&issueLimit=100#)

[Slack chanel - #appointments-service](https://nhsdigitalcorporate.enterprise.slack.com/archives/C062RBW4NF4)

## Team

- Sponsor - James Spirit
- Product Owner - Lauren Caveney
- Development Team - Vincent Crowe, Sam Biram, Kim Crowe, Khurram Aziz, Saritha Lakkreddy
- Past Development Team - Joe Farine

### Technologies

- Dotnet V8
- C# V12
- Azure Functions V4
- Javascript/Typescript (React V18)
- Next.js V14
- Cosmos document database

### Project Goal

We will build an Appointments API using

- Will follow REST principles as much as possible
- Github (for code repo)
- Azure Devops (for CI pipelines)
- C# (development language)
- Azure Functions (as a platform for hosting)

We will build a web based application for appointment and site management

- We will use React/Next.js
- Github (for code repo)
- Azure Devops (pipelines)
- Azure Web Apps (to host the NextJS server)

## GitHub Repo and signed commits

The repo has signed commits enabled. This means that only commits that are verified can be merged into the main branch. More information about signed commits can be found [here](https://docs.github.com/en/authentication/managing-commit-signature-verification/about-commit-signature-verification).

Your workstation will need to be set up to use signed commits. Please follow this guide for [instructions](https://github.com/NHSDigital/software-engineering-quality-framework/blob/main/practices/guides/commit-signing.md).\*

\*At the time of writing this guide is missing a step in the Windows section. After running `git config --global commit.gpgsign true` you must then run `git config --global user.signingkey <key>` to actually tell git about the key you just created.

## Running Locally

### Setup

- Install [Docker for Windows](https://docs.docker.com/desktop/install/windows-install/) (use Linux
  Containers)
- Install [.NET 8.0](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60)
- Install the [Azure Functions Core
  Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)
- Install [Node V20](https://nodejs.org/en/learn/getting-started/an-introduction-to-the-npm-package-manager)
- Install [NPM](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)

#### VS Code Setup (optional)

If you are using VS Code, you may find it beneficial to open the repo by opening the `nbs-ams.code-workspace` file itself. This will open a workspace configured with several productivity boons:

- The Explorer window is split into folders for convenience and to make navigating the repo quicker/easier
- Terminals can be opened straight into any of these folders using the `ctrl-shift-'` command
- Default build tasks have been configured for Dotnet and Typescript. You can run these with the `ctrl-shift-b` command
- Non-build tasks have been configured for starting the docker, dotnet, and NextJS services. These are configured in `tasks.json` folders and can be ran through `Terminal` -> `Run Task...`

### Running locally

Each of the following steps needs to be done in a separate terminal window

- Run the Cosmos Emulator and MockApi
  - From the root folder run `docker compose up`
  - (OR if using VS Code) run the `Start local development containers` task
- Run the API
  - From the folder `/src/api/Nhs.Appointments.Api` run the command `func start`
  - (OR if using VS Code) run the `Clean and Run API` task
- Run the Web Application
  - From the folder `/src/client/` run `npm install` and then run `npm run dev`
  - (OR if using VS Code) run the `Run Client in Dev Mode` task

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
Client configuration file is found at `/src/new-client/.env`

### Exploring the Cosmos Database

When running locally in docker the data in the cosmos database can be viewed and modified using the cosmos emulator
explorer. To access the explorer visit `https://localhost:8081/_explorer/index.html` (note you may get an unsafe page
warning unless you import the certificate from the cosmos container, this can safely be ignored for this url)

### Seeding the Cosmos Database

To meaningfully explore the API/frontend you'll need a minimum set of data present in the Cosmos DB. There is a .NET Console App which uploads a set of cosmos-friendly documents in `/src/mock-data/CosmosDbSeeder`.

You simply need to run this app to upload the default mock data. If using VS Code you can do this by running the `Seed Cosmos` task. You can also run it manually with the `dotnet run` terminal command in that folder.

The folder/file structure within the `items` folder matches the desired structure in Cosmos 1:1 (that is, there will be a Cosmos container called `index_data` which will contain each child `.json` file as a document). If you wish to create or modify a container simply alter these files, then re-run the seeder. Remember that if you run the app manually you will need to run `dotnet clean` first for the changes to be picked up (the VS Code task does this for you automatically).

Alternatively, you can upload these files one at a time yourself through the [emulator's browser interface](https://localhost:8081/_explorer/index.html).

## Authenticating calls to the API

REST calls using an API user must be signed (via HMAC) using the key associated with the api user.
The api user is identified using the ClientId header - so the in the above example a signed request with a ClientId header of `dev` would be needed to make api calls
