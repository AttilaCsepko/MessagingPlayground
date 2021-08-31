using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MessageConsumers.AuthenticationModels;
using MessageConsumers.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MessageConsumers
{
    public static class CosmosEntryFromEventHubManagedIdentity
    {
        private const string _eventHubConnectionStringVariable = "EventHubConnection";
        private const string _eventHubName = "messagewithsastoken";

        private const string _cosmosDbName = "MessagesDB";
        private const string _cosmosDbCollectionName = "Messages";

        private static string _subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");

        private static string _resourceGroupName = Environment.GetEnvironmentVariable("ResourceGroupName");
        
        private static string _accountName = Environment.GetEnvironmentVariable("CosmosDbAccountName");
        
        private static string _cosmosDbEndpoint = Environment.GetEnvironmentVariable("CosmosDbEndpoint");


        // HttpClient is intended to be instantiated once, rather than per-use.
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("CosmosEntryFromEventHubManagedIdentity")]
        public static async Task Run([EventHubTrigger(_eventHubName, Connection = _eventHubConnectionStringVariable)] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            Container container = null;
            try
            {
                container = await CreateCosmosDBManagedIdentityConnection(log);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                log.LogError("Can't retrieve CosmosDB container");
            }
            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");

                    var input = JsonSerializer.Deserialize<ExtendedMessageBodyForCosmos>(messageBody);
                    if (container != null)
                    {
                        input.Processor = "CosmosEntryFromEventHubManagedIdentity";
                        await container.CreateItemAsync(input);
                        log.LogInformation("Data persisted to CosmosDB");
                    }
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static async Task<Container> CreateCosmosDBManagedIdentityConnection(ILogger log)
        {
            // AzureServiceTokenProvider will help us to get the Service Managed token.
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // Authenticate to the Azure Resource Manager to get the Service Managed token.
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/");

            // Setup the List Keys API to get the Azure Cosmos DB keys.
            string endpoint = $"https://management.azure.com/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_accountName}/listKeys?api-version=2019-12-12";

            // Add the access token to request headers.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Post to the endpoint to get the keys result.
            var result = await httpClient.PostAsync(endpoint, new StringContent(""));

            // Get the result back as a DatabaseAccountListKeysResult.
            var keys = await result.Content.ReadAsAsync<DatabaseAccountListKeysResult>();

            log.LogInformation("Starting to create the client");

            CosmosClient client = new CosmosClient(_cosmosDbEndpoint, keys.primaryMasterKey);

            log.LogInformation("Client created");

            var database = client.GetDatabase(_cosmosDbName);
            var container = database.GetContainer(_cosmosDbCollectionName);
            if (container != null)
                log.LogInformation($"CosmosDB container [{_cosmosDbCollectionName}] loaded");

            return container;
        }
    }
}
