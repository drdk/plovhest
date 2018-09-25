using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using Plovhest.Shared;

namespace Plovhest.Web
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
           
            services.AddHangfire(config =>
                {
                    config.UseSqlServerStorage(Configuration.GetConnectionString("PlovhestDatabase"));
                });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwagger();
            services.AddDbContext<PlovhestDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("PlovhestDatabase")));
            services.AddLogging(loggingBuilder => loggingBuilder.AddConfiguration(Configuration.GetSection("Logging")));
            services.AddTransient<ProcessWrapper>();
            services.AddSingleton((ISettings) new Settings());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            PlovhestDbContext.Initialize(app.ApplicationServices);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHangfireDashboard();
            app.UseHttpsRedirection();
            app.UseSwaggerUi3WithApiExplorer(settings =>
            {
                settings.GeneratorSettings.SchemaType = 
                    SchemaType.OpenApi3;

                settings.GeneratorSettings.DefaultPropertyNameHandling = 
                    PropertyNameHandling.CamelCase;

                settings.GeneratorSettings.DefaultEnumHandling =
                    EnumHandling.String;
                
                settings.PostProcess = doc =>
                {
                    doc.Info.Title = "Plovhest";
                    doc.Info.Contact = new SwaggerContact
                    {
                        Name = "DR TU Streaming",
                        Email = "dl-DRTUStreamingteam@dr.dk",
                        Url = "https://github.com/drdk"
                    };
                    doc.Info.Description = $"" +
                                           $"* [Hangfire](/hangfire)";
                };
                
            });
            app.UseMvc();
        }
    }
}
