using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.AzureFunction.Consumers;
using Sample.AzureFunction.Sagas;

namespace Deploy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            
            var services = new ServiceCollection();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AuditOrderConsumer>();

                x.AddSagaStateMachine<SubmitOrderStateMachine, SumbitOrderState, SubmitOrderStateMachineDefinition>()
                    .MessageSessionRepository();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration["ServiceBusConnection"]);
                    
                    cfg.DeployTopologyOnly = true;

                    cfg.ConfigureEndpoints(context);
                });
                
                // x.AddRider(r =>
                // {
                //     r.UsingEventHub((context, cfg) =>
                //     {
                //         cfg.Host(configuration["EventHubConnection"]);
                //     });
                // });
            });

            var provider = services.BuildServiceProvider();

            var busControl = provider.GetRequiredService<IBusControl>();

            try
            {
                await busControl.DeployAsync(new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token);

                Console.WriteLine("Topology Deployed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to deploy topology: {0}", ex);
            }
        }
    }
}