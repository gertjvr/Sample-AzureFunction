using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.AzureFunction.Sagas;
using Sample.Components.Consumers;
using Sample.Contracts;

namespace Sample.AzureFunction
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<CreateOrderFunctions>();
                    services.AddScoped<SubmitOrderFunctions>();
                    services.AddScoped<AuditOrderFunctions>();
                    
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        
                        x.AddConsumer<AuditOrderConsumer>();
                        
                        x.AddSagaStateMachine<SubmitOrderStateMachine, SubmitOrderState, SubmitOrderStateMachineDefinition>()
                            .MessageSessionRepository();

                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration["ServiceBusConnection"]);
                            
                            cfg.Send<SubmitOrder>(x =>
                            {
                                x.UseSessionIdFormatter(context => context.Message.OrderId.ToString());
                            });
                        });

                        x.AddRider(r =>
                        {
                            r.UsingEventHub((context, cfg) =>
                            {
                                cfg.Host(hostContext.Configuration["EventHubConnection"]);
                            });
                        });
                    });

                    services.AddMassTransitHostedService(true);
                });
        }
    }
}