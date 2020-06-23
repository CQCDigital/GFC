
# Give Feedback on Care  · [![Build Status](https://dev.azure.com/CQCDigital/SYE-Project/_apis/build/status/GFC-MASTER-Build?branchName=master)](https://dev.azure.com/CQCDigital/SYE-Project/_build/latest?definitionId=13&branchName=master)

## Table of Contents
 - [Introduction](#introduction)
 - [Project Overview](#project-overview)
 - [Application Components](#application-components)
 - [Search](#search)
 - [Form Schema](#form-schema)
 - [Key Vault](#key-vault)
 - [Technology](#technology)
 - [Licence](#licence)

## Introduction
The purpose of the Give feedback on care service: a way for people to tell the Care Quality Commission (CQC) about their experience at health or social care providers. CQC inspectors and analysts use the information from this service to help us decide when, where and what to inspect. To see the live site in action, visit [https://www.cqc.org.uk/give-feedback-on-care](https://www.cqc.org.uk/give-feedback-on-care).

[Back to Top](#table-of-contents)

## Project Overview
The GFC application is built using Microsoft .Net Core 3.1 and has been designed to run on an Azure WebApp, although the application will happily run on IIS as well. The application utilises Azures Cosmos Db for data storage, which is a Multi-Model unstructured Database. We also use a combination of the appsettings.json file and Azure Key Vault for storage of application configuration. For local development, the local equivalents of the [Key Vault](#key-vault) values are stored within a [local secrets](#key-vault) file.

[Back to Top](#table-of-contents)

## Application Components
The application/service works by utilising the following features:

[Back to Top](#table-of-contents)

## Search
This allows users to search through our list of regulated care providers. The search is powered by [Azure Cognitive Search](https://docs.microsoft.com/en-us/azure/search/search-what-is-azure-search), which in turn indexes a copy of the data from [CQC's public location API](https://www.cqc.org.uk/about-us/transparency/using-cqc-data) hosted on Azure Cosmos. An Azure Logic App is used to query this API and populate this database every few minutes, this allows us to make sure that the search feature within the service is almost in sync with any changes to CQC's CRM system.

The schema for the search takes the following format:
```
{
	"providerId": "RP4",
	"id": "RP401",
	"locationName": " Great Ormond Street Hospital",
	"alsoKnownAs": "",
	"fullAddress": "Great Ormond Street London ",
	"postalAddressLine1": "Great Ormond Street",
	"postalAddressLine2": "",
	"postalAddressTownCity": "London",
	"postalAddressCounty": "",
	"postalCode": "WC1N 3JH",
	"syeInspectionCategories": [
		"Hospital - NHS",
		"Ambulance"
	],
	"regulatedPeople": []
}
```
[Back to Top](#table-of-contents)

## Form Schema
Once a user has chosen their care provider from the search results, they are then taken through a set of questions, the flow of which is based upon previous answers. The questions asked, the content, validation, ordering and flow are configured using our Form Schema, a json file *(SYE/Content/form-schema.json)* which again is hosted in our Azure Comos Db. This schema file allows us to change the configuration, content, validation and number of the questions very easily with minimal change to the service.

To help you understand how this file is structured and how each part relates to another, please see the following file included in this repo - [GFC Form Schema Documentation.pdf](GFC%20Form%20Schema%20Documentation.pdf).


[Back to Top](#table-of-contents)

## Key Vault
The following json schema contains a copy of our local secrets file (redacted of course), all values in this are duplicated within each hosted environment's Key Vault: DEV, TEST, UAT, PROD etc.

* **ApplicationSettings** - This section is used to store application specific configuration. The GFC application has its main start page hosted on the cqc.org.uk site, and this section helps to configure this start page url. In addition because some data is sent from the cqc.org.uk site, we lock down the ability for websites to post data to the GFC app: this is done using the AllowedCorsDomainselement.
* **CosmosDBConnectionPolicy** - This is used to help configure the default connection to our Cosmos Db.
* **ConnectionStrings** - This section stores all of our connectivity information.
	* **CQCRedirection** - This section works with the AllowedCorsDomains, the allowed domains are given a unique name/value pair of encrypted values. These are then validated against the referring domain when users are sent to the GFC application.
	* **DefaultCosmosDB** - This is the connection string and api key to the Cosmos DB, the values below are the ones for our local enviroment which are the same for any developers using the Azure Cosmos Db Emulator.
	* **LocationSearchCosmosDB** - In addition to allowing users to search for provider locations within the GFC application, we allow users to search and select provider locations within cqc.org.uk before arriving on the GFC application. We use this section to store the connection to our location database to validate this data.
	* **EsbConfig** - GFC uses a connection to Mulesoft ESB in order to push the data within the application database to other CQC systems. This section stores the configuration for this interaction.
	* **SearchDb** - This section stores the url, index name and api key for our Azure Search Instance.
	* **GovUkNotify** - The GFC application uses the Gov Notify system to send emails to end users. This section stores the configuration for this connectivity.
* **CosmosDBCollections** - This section stores the Cosmos database and collection names used by the GFC App.

```
{
  "ApplicationSettings": {
    "GFCUrls": {
      "StartPage": "https://www.mystartpage.com",
      "RedirectUrl": ""
    },
    "AllowedCorsDomains": "localhost:44339,www.cqc.org.uk"
  },
  "CosmosDBConnectionPolicy": {
    "ConnectionMode": "Direct",
    "ConnectionProtocol": "Tcp"
  },
  "ConnectionStrings": {
    "CQCRedirection": {
      "GFCKey": "CORS-KEY-NAME",
      "GFCPassword": "CORS-KEY-VALUE"
    },
    "DefaultCosmosDB": {
      "Endpoint": "https://localhost:8081/",
      "Key": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
    },
    "LocationSearchCosmosDB": {
      "Endpoint": "URL-FOR-SEARCH-DATABASE",
      "Key": "API-KEY-FOR-SEARCH-DATABASE"
    },
    "EsbConfig": {
      "esbAuthenticationEndpoint": "ESB-AUTHENTICATION-ENDPOINT",
      "esbAuthenticationSubmitKey": "SOAPAction",
      "esbAuthenticationSubmitValue": "ESB-AUTHENTICATION-VALUE",
      "esbGenericAttachmentEndpoint": "ESB-SEND-PAYLOAD-ENDPOINT",
      "esbGenericAttachmentSubmitKey": "SOAPAction",
      "esbGenericAttachmentSubmitValue": "document/ESB-SEND-PAYLOAD-VALUE:CreateEnquiry",
      "apiPublicKey": "ESB-PUBLIC-API-KEY"
    },
    "SearchDb": {
      "SearchServiceName": "AZURE-SEARCH-SERVICE-NAME",
      "SearchApiKey": "AZURE-SEARCH-SERVICE-API-KEY",
      "IndexName": "AZURE-SEARCH-INDEX-NAME"
    },
    "GovUkNotify": {
      "ApiKey": "GOV-NOTIFY-API-KEY"
    }
  },
  "CosmosDBCollections": {
    "LocationSchemaDb": {
      "DatabaseId": "CQCData",
      "CollectionId": "ProviderLocations"
    },
    "FormSchemaDb": {
      "DatabaseId": "gfc_db",
      "CollectionId": "form_schema"
    },
    "SubmissionsDb": {
      "DatabaseId": "gfc_db",
      "CollectionId": "submissions"
    },
    "ConfigDb": {
      "DatabaseId": "gfc_db",
      "CollectionId": "config",
      "ConfigRecordId": "CONFIR-DOCUMEN-ID"
    }
  }
}
```
[Back to Top](#table-of-contents)

## Technology
The GFC project utilises the following technology:

* **.Net Core 3.1** - .Net Core 3.1 is, at the time of writing, the latest version of Microsoft's open source .Net Framework. The GFC application has been written to be open source from the ground up.
* **GovUK Design System** - We use this design system to make our service consistent with GOV.UK. The [GovUK Design System](https://design-system.service.gov.uk/) has been designed from the research and experience of other service teams and helps us to avoid repeating work that’s already been done.
* **Azure WebApp** - We deploy the GFC application on an Azure WebApp, as this allows us easy customatisation and replication of our enviroment.
* **Azure Key Vault** - We use [Azure Key Vault](https://azure.microsoft.com/en-gb/services/key-vault/) to store application "secrets" in a secure manner. This enables us to securely store things like database connection strings away from our code and release pipelines, and also gives us permission-based access to each environment's Key Vault.
* **Azure Cognitive Search** - [Azure Cognitive Search](https://azure.microsoft.com/en-gb/services/search/) is a cloud search service with built-in AI capabilities that helps to enrich all types of information to easily identify and explore relevant content at scale. We use Azure Cognitive Search to help provide a rich search experience to the end users.
* **Azure Cosmos DB** - 

[Back to Top](#table-of-contents)

## Licence

Unless stated otherwise, the codebase is released under the MIT License. This
covers both the codebase and any sample code in the documentation. The
documentation is &copy; Crown copyright and available under the terms of the
[Open Government 3.0 licence](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/).