using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventHubMessageProducreSASToken
{
    public class SendService : ISendService
    {
        private const string _source = "EventHubMessageProducreSASToken";

        private readonly EventHubProducerClient _eventHubProducerClient;
        private readonly IConfiguration _config;
        private readonly ILogger<SendService> _logger;

        public SendService(ILogger<SendService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var eventHubConnectionString = _config.GetValue<string>("EventHubConnectionString") ?? string.Empty;
            var eventHubName = _config.GetValue<string>("EventHubName") ?? string.Empty;

            if (string.IsNullOrEmpty(eventHubConnectionString))
                _logger.LogError("EventHubConnectionString is not definied");

            if (string.IsNullOrEmpty(eventHubName))
                _logger.LogError("EventHubName is not definied");


            try
            {
                _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName);
            }
            catch (Exception)
            {
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_eventHubProducerClient == null)
                return;

            try
            {
                using var eventBatch = await _eventHubProducerClient.CreateBatchAsync();

                var messageContent = new
                {
                    Source = _source,
                    TimeStamp = DateTime.UtcNow,
                    Guid = Guid.NewGuid(),
                    Message = message
                };

                var messageJson = JsonSerializer.Serialize(messageContent);

                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(messageJson)));
                await _eventHubProducerClient.SendAsync(eventBatch);
                _logger.LogInformation("Event batch sent with 1 message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendTenMessageAsync(string message)
        {
            if (_eventHubProducerClient == null)
                return;

            try
            {
                using var eventBatch = await _eventHubProducerClient.CreateBatchAsync();
                for (int i = 0; i < 10; i++)
                {
                    var messageContent = new
                    {
                        Source = _source,
                        TimeStamp = DateTime.UtcNow,
                        Guid = Guid.NewGuid(),
                        Message = $"{message} #{i}"
                    };

                    var messageJson = JsonSerializer.Serialize(messageContent);

                    eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(messageJson)));
                }

                await _eventHubProducerClient.SendAsync(eventBatch);
                _logger.LogInformation("Event batch sent with 10 messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
