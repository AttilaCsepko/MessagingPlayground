using System;
using System.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using MessageConsumers.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MessageConsumers
{
    public static class SQLEntryFromServiceBus
    {
        private const string _serviceBusConnectionStringVariable = "ServiceBusConnection";
        private const string _serviceBusName = "messagewithsastoken";

        private const string _sqlDbConnectionStringVariable = "SqlConnection";

        [FunctionName("SQLEntryFromServiceBus")]
        public static async Task Run([ServiceBusTrigger(_serviceBusName, Connection = _serviceBusConnectionStringVariable)] string mySbMsg,
            ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");

                var input = JsonSerializer.Deserialize<ExpectedMessageBody>(mySbMsg);
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable(_sqlDbConnectionStringVariable)))
                {
                    connection.Open();
                    if (true)
                    {
                        var query = $"INSERT INTO [Messages] (UUID, Source, MsgJson) VALUES('{input.Guid}', '{input.Source}' , '{mySbMsg}')";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw ex;
            }
        }
    }
}
