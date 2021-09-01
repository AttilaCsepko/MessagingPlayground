using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ServiceBusMessageProducerServicePrincipalCertificate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ISendService, SendService>();
                    services.AddTransient<IConsoleApplication, ConsoleApplication>();

                })
                .Build();

            var svc = ActivatorUtilities.CreateInstance<ConsoleApplication>(host.Services);
            await svc.RunAsync();
        }
    }
}
