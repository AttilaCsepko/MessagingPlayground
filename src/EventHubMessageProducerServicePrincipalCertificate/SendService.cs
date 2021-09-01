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
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;

namespace EventHubMessageProducerServicePrincipalCertificate
{
    public class SendService : ISendService
    {
        private const string _source = "EventHubMessageProducreServicePrincipalCertificate";

        private readonly EventHubProducerClient _eventHubProducerClient;
        private readonly IConfiguration _config;
        private readonly ILogger<SendService> _logger;

        public SendService(ILogger<SendService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var certificateThumbprint = _config.GetValue<string>("CertificateThumbprint") ?? string.Empty;
            
            var tenantID = _config.GetValue<string>("TenantID") ?? string.Empty;
            var clientId = _config.GetValue<string>("ClientId") ?? string.Empty;

            var eventHubNamespace = _config.GetValue<string>("EventHubNamespace") ?? string.Empty;
            var eventHubName = _config.GetValue<string>("EventHubName") ?? string.Empty;

            // TODO: validate values

            var cert = GetCertificate(certificateThumbprint);
            _eventHubProducerClient = CreateEventHubProducerClient(tenantID, clientId, cert, eventHubNamespace, eventHubName);
        }

        private X509Certificate2 GetCertificate(string certificateThumbprint)
        {
            X509Certificate2 certificate = null;

            try
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                var certificates = collection.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);

                certificate = certificates?[0];

                store.Close();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return certificate;
        }

        private EventHubProducerClient CreateEventHubProducerClient(string tenantId, string clientId, X509Certificate2 certificate, string eventHubNamespace, string eventHubName)
        {
            EventHubProducerClient eventHubProducerClient = null;

            try
            {
                var managedCredential = new ManagedIdentityCredential(clientId);
                var certCredential = new ClientCertificateCredential(tenantId, clientId, certificate);

                // authenticate using managed identity if it is available otherwise use certificate auth
                var credential = new ChainedTokenCredential(managedCredential, certCredential);

                eventHubProducerClient = new EventHubProducerClient(eventHubNamespace, eventHubName, credential);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return eventHubProducerClient;
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
