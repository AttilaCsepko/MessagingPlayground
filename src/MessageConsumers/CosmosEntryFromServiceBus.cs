using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MessageConsumers
{
    public static class CosmosEntryFromServiceBus
    {
        private const string _serviceBusConnectionStringVariable = "ServiceBusConnection";
        private const string _serviceBusName = "messagewithsastoken";

        private const string _cosmosDbConnectionStringVariable = "CosmosDBConnection";
        private const string _cosmosDbName = "MessagesDB";
        private const string _cosmosDbCollectionName = "Messages";
        
        [FunctionName("CosmosEntryFromServiceBus")]
        public static async Task Run([ServiceBusTrigger(_serviceBusName, Connection = _serviceBusConnectionStringVariable)]string mySbMsg,
            [CosmosDB(databaseName: _cosmosDbName, collectionName: _cosmosDbCollectionName, ConnectionStringSetting = _cosmosDbConnectionStringVariable)] IAsyncCollector<object> items,
            ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
                await items.AddAsync(mySbMsg);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw ex;
            }
        }
    }
}
