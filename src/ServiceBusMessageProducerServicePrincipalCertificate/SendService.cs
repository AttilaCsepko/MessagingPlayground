using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBusMessageProducerServicePrincipalCertificate
{
    public class SendService : ISendService
    {
        private const string _source = "ServiceBusMessageProducerServicePrincipalCertificate";

        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;

        private readonly IConfiguration _config;
        private readonly ILogger<SendService> _logger;

        public SendService(ILogger<SendService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var certificateThumbprint = _config.GetValue<string>("CertificateThumbprint") ?? string.Empty;
            
            var tenantID = _config.GetValue<string>("TenantID") ?? string.Empty;
            var clientId = _config.GetValue<string>("ClientId") ?? string.Empty;

            var serviceBusNamespace = _config.GetValue<string>("ServiceBusNamespace") ?? string.Empty;
            var serviceBusTopic = _config.GetValue<string>("ServiceBusTopic") ?? string.Empty;

            // TODO: validate values

            var cert = GetCertificate(certificateThumbprint);
            _serviceBusClient = CreateServiceBusClient(tenantID, clientId, cert, serviceBusNamespace);
            _serviceBusSender = _serviceBusClient?.CreateSender(serviceBusTopic);

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

        private ServiceBusClient CreateServiceBusClient(string tenantId, string clientId, X509Certificate2 certificate, string serviceBusNamespace)
        {
            ServiceBusClient serviceBusClient = null;

            try
            {
                var managedCredential = new ManagedIdentityCredential(clientId);
                var certCredential = new ClientCertificateCredential(tenantId, clientId, certificate);

                // authenticate using managed identity if it is available otherwise use certificate auth
                var credential = new ChainedTokenCredential(managedCredential, certCredential);

                serviceBusClient = new ServiceBusClient(serviceBusNamespace, credential);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return serviceBusClient;
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
                    Source = _source,
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
                        Source = _source,
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
