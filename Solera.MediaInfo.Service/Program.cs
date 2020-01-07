using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Solera.MediaInfo.Service.Constants;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Logging;
using System;
using System.IO;

namespace Solera.MediaInfo.Service
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(EnvironmentVariable.ASPNETCORE_ENVIRONMENT) ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            // Add VCAP_* configuration data
            .AddCloudFoundry()
            .Build();

        public static int Main(string[] args)
        {
            Log.Logger = new Serilog.LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting Media Info Service");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .UseKestrel(k => k.AddServerHeader = false)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseDefaultServiceProvider(options => options.ValidateScopes = false)
                    .UseCloudFoundryHosting()
                    // Add VCAP_* configuration data
                    .AddCloudFoundry()
                    .ConfigureLogging((builderContext, loggingBuilder) =>
                    {
                        loggingBuilder.ClearProviders();  // avoid duplicated log entries
                        loggingBuilder.AddDynamicConsole();
                    })
                    .UseStartup<Startup>();
    }
}
