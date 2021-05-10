using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.AzureFunction.Consumers;
using Sample.AzureFunction.Sagas;

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
                        x.AddConsumer<AuditOrderConsumer>();
                        
                        //x.SetMessageSessionSagaRepositoryProvider();
                        x.SetInMemorySagaRepositoryProvider();
                        
                        x.AddSagaStateMachine(typeof(SubmitOrderStateMachine), typeof(SubmitOrderStateMachineDefinition));
                        
                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration["ServiceBusConnection"]);
                            
                            cfg.ClearMessageDeserializers();
                            cfg.UseRawJsonSerializer();
                        });

                        x.AddRider(r =>
                        {
                            r.UsingEventHub((context, cfg) =>
                            {
                                cfg.Host(hostContext.Configuration["EventHubConnection"]);
                                cfg.UseRawJsonSerializer();
                            });
                        });

                        x.ConfigureReceiveEndpoint((_, cfg) =>
                        {
                            cfg.ClearMessageDeserializers();
                            cfg.UseRawJsonSerializer();
                        });
                        
                        
                    });

                    services.AddMassTransitHostedService(true);
                });
        }
    }
}