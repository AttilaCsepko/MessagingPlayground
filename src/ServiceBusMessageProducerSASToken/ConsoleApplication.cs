using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServiceBusMessageProducerSASToken
{
    class ConsoleApplication : IConsoleApplication
    {
        private readonly IConfiguration _config;
        private readonly ISendService _sendService;
        private readonly ILogger<ConsoleApplication> _logger;

        public ConsoleApplication(ILogger<ConsoleApplication> logger, IConfiguration config, ISendService sendService)
        {
            _logger = logger;
            _config = config;
            _sendService = sendService;
        }
        public void Run()
        {
        }

        public async Task RunAsync()
        {
            await _sendService.SendTenMessageAsync("This is the message!");
        }
    }
}
