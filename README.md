# National Booking Service - Manage Your Appointments (MYA)

## Introduction

An appointment and booking api designed to support national vaccination bookings, and a frontend for managing this booking availability.

[Product Backlog](https://nhsd-jira.digital.nhs.uk/secure/RapidBoard.jspa?rapidView=7622&projectKey=APPT&view=planning.nodetail&selectedIssue=APPT-26&issueLimit=100#)

[Slack chanel - #appointments-service](https://nhsdigitalcorporate.enterprise.slack.com/archives/C062RBW4NF4)

## Team

- Sponsor - James Spirit
- Product Owner - Lauren Caveney
- Development Team - Sam Biram, Khurram Aziz, Saritha Lakkreddy, Ste Banks, Jonny Dyson, David Olsavsky, Paul Tallet, Glen Smith, David Callaghan, Peter Choi
- Past Development Team - Joe Farina, Vincent Crowe, Kim Crowe, Jonathan Rowlett

### Technologies

- Dotnet V8
- C# V12
- Azure Functions V4
- Javascript/Typescript (React V18)
- Next.js V15
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

## Contributing

This is an open source repo. First and foremost, please remember that every commit made to this repo is visible to the wider public and reflects on the reputation of the NHS.

Before contributing, please ensure that:

- You have configured GIT to sign your commits (instructions for how to do this above)
- Your machine is set up to run the solution locally
- You have ran the various code quality assurance tools (i.e. lint, prettier, dotnet format etc.)
- You have ran all the tests, which should all pass (tests include backend unit, gherkin integration, node unit, and node e2e)
- Your branch is named after the ticket it addresses, e.g. "APPT-1234/create-booking-management-endpoint"
- You are in the United Kingdom. The NHS has strict rules prohibiting overseas working.

## Running Locally

### Required technologies

- Install [Docker for Windows](https://docs.docker.com/desktop/install/windows-install/) (use Linux
  Containers)
- Install [.NET 8.0](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60)
- Install the [Azure Functions Core
  Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)
- Install [Node V20](https://nodejs.org/en/learn/getting-started/an-introduction-to-the-npm-package-manager)
- Install [NPM](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)
- (Recommended) Install [the Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator). It is unfortunately not recommended to use the Cosmos DB docker image, as it has some major flaws and has not been supported for many years. It will work, but you will likely find the integration tests failing sporadically. The emulator is much more reliable.

#### VS Code Setup (optional)

If you are using VS Code, you may find it beneficial to open the repo by opening the `nbs-ams.code-workspace` file itself. This will open a workspace configured with several productivity boons:

- The Explorer window is split into folders for convenience and to make navigating the repo quicker/easier
- VS Code will prompt you to install several extensions listed as recommended by the workspace. These will make certain tasks much easier.
- Terminals can be opened straight into any of these folders using the `ctrl-shift-'` command
- Default build tasks have been configured for Dotnet and Typescript. You can run these with the `ctrl-shift-b` command
- Non-build tasks have been configured for starting the docker, dotnet, and NextJS services. These are configured in `tasks.json` folders and can be ran through `Terminal` -> `Run Task...`

### Project overview

The solution vaguely comprises the following moving parts:

- A Cosmos DB document store. This is our main form of persistence.
- An Azure blob storage container. This is a secondary form of persistence, only used very lightly to support some business logic
- One or more .NET runtimes, hosting our various Azure Functions. This is the backend API and the core solution.
- An ID server for authorising requests. The live app uses NHS Mail and Okta; locally we use a docker image of Mock OIDC.
- Azure Service Buses to collect out of process tasks (such as sending notifications). The live service uses real ones in Azure itself, locally we stub this code out and do not run a local equivalent or mock.
- A node runtime hosting our frontend web app, written in React/NextJS. This is an optional frontend that customers can use to visually manage the availability created through the backend API.

The solution can be ran locally a number of ways, but however you choose to run them you will need at minimum:

- An ID server
- A Cosmos DB
- The backend API
- The frontend Web App

It is recommended to:

- use the Cosmos DB emulator for Cosmos
- create and run the mock oidc ID server in docker, through its docker-compose profile
- run the backend API through your IDE, the VS Code one-click task, or in docker through its docker-compose profile
- run the web app through its VS Code task, or in docker through its docker-compose profile

### Setting a cert for the OIDC server

To run the oidc-server you will need to install a certificate. You will need to generate
and trust a dev cert with the following command (in order to ensure that the certificate
is placed in the correct location you should run the command from your user profile folder
or amend the command accordingly).

`dotnet dev-certs https -ep .aspnet/https/aspnetapp.pfx -p password -t -v`

### Running locally

To run the DB, simply run the emulator on your machine. Its default configuration should be sufficient.

To quickly start the services in docker, open a terminal at the root of the repo and enter
`docker compose up --build -d oidc-server next-app api`. If you only wish to run the ID server in docker, simply run
`docker compose up --build -d oidc-server`.

To run the backend API:

- From the folder `/src/api/Nhs.Appointments.Api` run the command `func start`
- (OR if using VS Code) run the `Clean and Run API` task

To run the frontend Web App:

- Run the Web Application
  - From the folder `/src/client/` run `npm install` and then run `npm run dev`
  - (OR if using VS Code) run the `Run Client in Dev Mode` task

### Notes about configuration

The API expects cosmos to be running at "https://localhost:8081"
The Web Application expects the API to be running at "http://localhost:7071"

If your configuration causes any of these to be running differently then you will need to change the application
configuration files accordingly. (Or use environment variables to override local configuration)

API configuration file is found at `/src/Nhs.Appointments.Api/localsettings.json`
Client configuration file is found at `/src/client/.env`

### Exploring the Cosmos Database

When running locally in docker the data in the cosmos database can be viewed and modified using the cosmos emulator
explorer. To access the explorer visit `https://localhost:8081/_explorer/index.html` (note you may get an unsafe page
warning unless you import the certificate from the cosmos container, this can safely be ignored for this url)

### Seeding the Cosmos Database

To meaningfully explore the API/frontend you'll need a minimum set of data present in the Cosmos DB. There is a .NET Console App which uploads a set of cosmos-friendly documents in `/src/data/CosmosDbSeeder`.

You simply need to run this app to upload the default mock data. If using VS Code you can do this by running the `Seed Cosmos` task. You can also run it manually with the `dotnet run` terminal command in that folder.

The folder/file structure within the `items` folder matches the desired structure in Cosmos 1:1 (that is, there will be a Cosmos container called `index_data` which will contain each child `.json` file as a document). If you wish to create or modify a container simply alter these files, then re-run the seeder. Remember that if you run the app manually you will need to run `dotnet clean` first for the changes to be picked up (the VS Code task does this for you automatically).

Alternatively, you can upload these files one at a time yourself through the [emulator's browser interface](https://localhost:8081/_explorer/index.html).

## Authenticating calls to the API

REST calls using an API user must be signed (via HMAC) using the key associated with the api user.
The api user is identified using the ClientId header - so the in the above example a signed request with a ClientId header of `dev` would be needed to make api calls.

## Code Style & Formatting

.NET code style and formatting rules are imposed by [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) because it is IDE and platform agnostic, and natively included in the .NET SDK. Rule are dictated by an [.editorconfig file](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options).

The warnings and errors about violations of these rules could be surfaced at build time by enabling [Enforce Code Style In Build (<EnforceCodeStyleInBuild>)](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#enforcecodestyleinbuild) in each `.csproj` file.

#### How do I configure my IDE?

Because the `.editorconfig` file is attached to the solution, both Rider and Visual Studio should automatically use its rules in place of their own defaults.

- on Visual Studio, you may need to visit `Analyze -> Code Cleanup -> Configure Code Cleanup`, then change your default profile to include only the `Fix all warnings and errors set in EditorConfig` step.
- on Rider, you may need to visit `Settings -> Editor -> Inspection Settings` then ensure `Read settings from editorconfig, project settings and rule sets` is ticked.

#### How do I format only one file?

The easiest way is probably through an IDE, configured as per the step above. Most IDEs can format single files if you right-click it in the explorer.
Failing that, the harder way is to pass an `--include <PATH>` argument to `dotnet format` on the command line, providing it the files you want it to format.

#### How do I run the formatter manually?

You can invoke dotnet format on the command line like so:

```
dotnet format nbs-manage-your-appointments.sln --verify-no-changes --report dotnet-format-report.json
```

This will create a report named `dotnet-format-report.json` in the repository root. This has been added to the `.gitignore` file so should not cause a tracked change.

The `--verify-no-changes` argument tells `format` to make no changes. If you want it to automatically apply fixes, simply remove this argument:

```
dotnet format nbs-manage-your-appointments.sln
```

If you want to run it against one or more specific directories in the solution (or indeed exclude one or more), these can be specified through the `--include <PATH>` and `--exclude <PATH>` arguments.

If you want to see only errors, or include suggestions, pass a new value to the severity argument (accepted values are `error`, `warn`, and `info`):

```
dotnet format nbs-manage-your-appointments.sln --severity <SEVERITY>
```

See the [docs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) for more on this.

## Tests

### Frontend Unit Tests (using Jest)

From `~/src/client`:

- Ensure you have ran `npm i`
- Simply run `npm run test`

If running the workspace in VS Code, the Jest tests should be automatically discovered in the test explorer window. You should be able to run and debug them via the UI.

### Frontend E2E Tests (using Playwright)

From `~/src/client`:

- Ensure you have ran `npm i`
- The very first time you run the tests you will need to run `npx playwright install`. This instructs Playwright to download and instantiate the latest version of the browsers it requires.
- Ensure the Docker and .NET services are running following their respective setup commands (In the future we hope to be set these up automatically)
- Optionally run the frontend app (If you're not already running it, Playwright will start it up for you)
- Run `npm run test:e2e`. Optionally if you wish to view a step-by-step visual output of each test, run `npm run test:e2e:ui`.
