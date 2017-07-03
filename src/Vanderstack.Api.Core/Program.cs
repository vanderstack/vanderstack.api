using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vanderstack.Api.Core
{
    public class Program
    {
        public static void Main(string[] args)
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
