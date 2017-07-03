using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vanderstack.Api.Core;

namespace Vanderstack.Api.Endpoints
{
    public class EndpointsService : IMicroService
    {
        public void Start()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseApplicationInsights()
                .ConfigureServices((services) =>
                    services.AddMvc()
                )
                //.ConfigureLogging((loggerFactory) => {
                //    loggerFactory.AddConsole(configuration.GetSection("Logging"));
                //    loggerFactory.AddDebug();
                //})
                .Configure((applicationBuilder) =>
                    applicationBuilder.UseMvc()
                )
                .Build();

            var environment = host.Services.GetService<IHostingEnvironment>();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            IConfiguration configuration = configurationBuilder.Build();


            host.Run();
        }
    }
}
