using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Logging;

namespace Solera.MediaInfo.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()

                .UseDefaultServiceProvider(options => options.ValidateScopes = false)
                .UseCloudFoundryHosting()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddCloudFoundry();
                })
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();  // avoid duplicated log entries
                    loggingBuilder.AddDynamicConsole();
                });
    }
}
