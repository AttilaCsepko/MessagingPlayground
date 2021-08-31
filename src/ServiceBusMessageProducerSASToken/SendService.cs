using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServiceBusMessageProducerSASToken
{
    public class SendService : ISendService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly IConfiguration _config;
        private readonly ILogger<SendService> _logger;

        public SendService(ILogger<SendService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var serviceBusConnectionString = _config.GetValue<string>("ServiceBusConnectionString") ?? string.Empty;
            var serviceBusTopic = _config.GetValue<string>("ServiceBusTopic") ?? string.Empty;

            if (string.IsNullOrEmpty(serviceBusConnectionString))
                _logger.LogError("ServiceBusConnectionString is not definied");

            if (string.IsNullOrEmpty(serviceBusTopic))
                _logger.LogError("ServiceBusTopic is not definied");


            try
            {
                _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
                _serviceBusSender = _serviceBusClient.CreateSender(serviceBusTopic);
            }
            catch (Exception)
            {
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_serviceBusSender == null)
                return;

            try
            {
                using var messageBatch = await _serviceBusSender.CreateMessageBatchAsync();

                var messageContent = new
                {
                    TimeStamp = DateTime.UtcNow,
                    Guid = Guid.NewGuid(),
                    Message = message
                };

                var messageJson = JsonSerializer.Serialize(messageContent);

                messageBatch.TryAddMessage(new ServiceBusMessage(messageJson));

                await _serviceBusSender.SendMessagesAsync(messageBatch);
                _logger.LogInformation("Message batch sent with 1 message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendTenMessageAsync(string message)
        {
            if (_serviceBusSender == null)
                return;

            try
            {
                using var messageBatch = await _serviceBusSender.CreateMessageBatchAsync();
                for (int i = 0; i < 10; i++)
                {
                    var messageContent = new
                    {
                        TimeStamp = DateTime.UtcNow,
                        Guid = Guid.NewGuid(),
                        Message = $"{message} #{i}"
                    };

                    var messageJson = JsonSerializer.Serialize(messageContent);

                    messageBatch.TryAddMessage(new ServiceBusMessage(messageJson));
                }

                await _serviceBusSender.SendMessagesAsync(messageBatch);
                _logger.LogInformation("Message batch sent with 10 messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
