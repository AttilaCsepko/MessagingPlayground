using System.Threading.Tasks;

namespace ServiceBusMessageProducerServicePrincipalCertificate
{
    public interface ISendService
    {
        Task SendMessageAsync(string message);
        Task SendTenMessageAsync(string message);
    }
}