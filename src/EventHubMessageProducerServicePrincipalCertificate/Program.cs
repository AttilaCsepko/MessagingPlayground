using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventHubMessageProducerServicePrincipalCertificate
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
