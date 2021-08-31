using System.Threading.Tasks;

namespace EventHubMessageProducreSASToken
{
    public interface ISendService
    {
        Task SendMessageAsync(string message);
        Task SendTenMessageAsync(string message);
    }
}