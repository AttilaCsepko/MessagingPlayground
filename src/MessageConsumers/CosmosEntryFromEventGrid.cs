// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace MessageConsumers
{
    public static class CosmosEntryFromEventGrid
    {
        private const string _cosmosDbConnectionStringVariable = "CosmosDBConnection";
        private const string _cosmosDbName = "MessagesDB";
        private const string _cosmosDbCollectionName = "Messages";

        [FunctionName("CosmosEntryFromEventGrid")]
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, 
            [CosmosDB(databaseName: _cosmosDbName, collectionName: _cosmosDbCollectionName, ConnectionStringSetting = _cosmosDbConnectionStringVariable)] IAsyncCollector<object> items,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
