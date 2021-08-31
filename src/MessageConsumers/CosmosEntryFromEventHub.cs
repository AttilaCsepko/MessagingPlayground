using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MessageConsumers
{
    public static class CosmosEntryFromEventHub
    {
        private const string _eventHubConnectionStringVariable = "EventHubConnection";
        private const string _eventHubName = "messagewithsastoken";

        private const string _cosmosDbConnectionStringVariable = "CosmosDBConnection";
        private const string _cosmosDbName = "MessagesDB";
        private const string _cosmosDbCollectionName = "Messages";

        [FunctionName("CosmosEntryFromEventHub")]
        public static async Task Run([EventHubTrigger(_eventHubName, Connection = _eventHubConnectionStringVariable)] EventData[] events,
            [CosmosDB(databaseName: _cosmosDbName, collectionName: _cosmosDbCollectionName, ConnectionStringSetting = _cosmosDbConnectionStringVariable)] IAsyncCollector<object> items,
            ILogger log)
        {
             var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");

                    await items.AddAsync(messageBody);
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
    }
}
