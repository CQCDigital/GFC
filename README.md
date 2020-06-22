# Share Your Experience


## Description



## Build Status


#### develop


[![Build status](https://dev.azure.com/CQCDigital/SYE-Project/_apis/build/status/SYE-NetCore-Build-Pipeline?branchName=develop)](https://dev.azure.com/CQCDigital/SYE-Project/_build/latest?definitionId=2)

#### master


[![Build status](https://dev.azure.com/CQCDigital/SYE-Project/_apis/build/status/SYE-NetCore-Build-Pipeline?branchName=master)](https://dev.azure.com/CQCDigital/SYE-Project/_build/latest?definitionId=2)

## Table of contents



## Development and Test Environment

### Framework
  * .Net Core 2.2

### NuGet Package Sources

For GDSHelpers add [this](https://pkgs.dev.azure.com/CQCDigital/_packaging/GdsHelpers-Nuget-Feed/nuget/v3/index.json) link to list of NuGet package sources.
For The GOV.UK Notify client add [this](https://api.bintray.com/nuget/gov-uk-notify/nuget) link to list of NuGet package sources.


### Tools
  * Visual Studio 2017 or Visual Studio Code

  * Azure Cosmos DB Emulator download from [here](https://aka.ms/cosmosdb-emulator).

    Once you install and start the emulator please create databse, collection and populate it with data.
    You can find officual documentation (including limitations) for Cosmos DB emulator [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).
 
    Alternatively you can use Docker image of Azure Cosmos DB Emulator**.

** This is not tested by SYE development team yet.

### Caching


### Session



## Building


## Developing and Testing locally (Debugging locally)

 * If you are in the middle of piece of work that requires access to existing Cosmos Db in cloud, please change "ASPNETCORE_ENVIRONMENT" in launchSettings.json from "Local" to "Development".

## Testing



## Deploying



## Key Contacts

## AI Logging
    // Following is only required if capturing logs from Program and Startup to Application Insights.
    // Otherwise you can just use loggingBuilder.AddApplicationInsights() without any arguments and iKey will be picked up from application settings
    var iKey = builderContext?.Configuration?.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");
    if (!string.IsNullOrWhiteSpace(iKey))
    {
        loggingBuilder.AddApplicationInsights(iKey);
        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Error);
        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Error);
    }