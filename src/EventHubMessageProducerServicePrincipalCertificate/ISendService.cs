using System.Threading.Tasks;

namespace EventHubMessageProducerServicePrincipalCertificate
{
    public interface ISendService
    {
        Task SendMessageAsync(string message);
        Task SendTenMessageAsync(string message);
    }
}