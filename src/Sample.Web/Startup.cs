using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Sample.Components.Consumers;
using Sample.Components.Sagas;
using Sample.Contracts;

namespace Sample.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Sample.Web", Version = "v1"});
            });
            
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                        
                x.AddConsumer<AuditOrderConsumer>();
                        
                x.AddSagaStateMachine<SubmitOrderStateMachine, SubmitOrderState, SubmitOrderStateMachineDefinition>()
                    .MessageSessionRepository();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(Configuration["ServiceBusConnection"]);
                            
                    cfg.Send<SubmitOrder>(x =>
                    {
                        x.UseSessionIdFormatter(context => context.Message.OrderId.ToString());
                    });
                    
                    cfg.ConfigureEndpoints(context);
                });

                x.AddRider(r =>
                {
                    r.UsingEventHub((context, cfg) =>
                    {
                        cfg.Host(Configuration["EventHubConnection"]);
                        
                        cfg.Storage("UseDevelopmentStorage=true;");
                    });
                });
            });

            services.AddMassTransitHostedService(true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.Web v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}