using System.Threading.Tasks;

namespace ServiceBusMessageProducerSASToken
{
    public interface ISendService
    {
        Task SendMessageAsync(string message);
        Task SendTenMessageAsync(string message);
    }
}