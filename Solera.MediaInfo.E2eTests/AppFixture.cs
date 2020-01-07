using Solera.MediaInfo.E2eTests.Utilities;
using Solera.MediaInfo.Service.Constants;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solera.MediaInfo.E2eTests
{
    public class AppFixture : IDisposable
    {
        public readonly string MisUrl;

        private readonly IDisposable _misApp;

        public HttpClient Client { get; }

        public AppFixture()
        {
            MisUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5005";
            var envVars = new EnvironmentVariables();

            var skipAppStart = Environment.GetEnvironmentVariable("SKIP_APP_START") == bool.TrueString;
            if (!skipAppStart)
            {
                var solutionDirectory = new DirectoryInfo(Path.GetFullPath("."));
                while (solutionDirectory.Name != "Solera.MediaInfo.E2eTests")
                {
                    solutionDirectory = solutionDirectory.Parent;
                }

                solutionDirectory = solutionDirectory.Parent;
                PublishProject(solutionDirectory);
                _misApp = RunMis(solutionDirectory, envVars);
            }

            WaitForHealthcheckOK("Solera.MediaInfo.Service", $"{MisUrl}/api/health");
            Client = new HttpClient();
        }

        public void Dispose()
        {
            _misApp?.Dispose();
        }

        private static void PublishProject(DirectoryInfo rootDirectory)
        {
            Console.WriteLine("Publishing Solera.MediaInfo.Service project");
            var csProjFilePath = Path.Combine(rootDirectory.FullName, "Solera.MediaInfo.Service", "Solera.MediaInfo.Service.csproj");
            var mode = "Release";
#if DEBUG
            mode = "Debug";
#endif
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", $"publish -c {mode} \"{csProjFilePath}\"")
            };
            process.Start();
            using (new LiveProcess(process))
            {
                process.WaitForExit();
            }

            if (process.ExitCode != 0)
            {
                throw new Exception($"Publish Solera.MediaInfo.Service exited with non zero exit code {process.ExitCode}");
            }
        }

        private IDisposable RunMis(DirectoryInfo rootDirectory, EnvironmentVariables environmentVariables)
        {
            Console.WriteLine("Running Solera.MediaInfo.Service");
            var mode = "Release";
#if DEBUG
            mode = "Debug";
#endif
            var binDllPath = Path.Combine(rootDirectory.FullName, "Solera.MediaInfo.Service", "bin", mode, "netcoreapp2.1", "publish", "Solera.MediaInfo.Service.dll");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", $"\"{binDllPath}\"")
                {
                    Environment = {
                        { "ASPNETCORE_URLS", MisUrl },
                        { EnvironmentVariable.ASPNETCORE_ENVIRONMENT, environmentVariables.AspNetCoreEnvironment },
                        { EnvironmentVariable.S3_URL, environmentVariables.S3ApiUrl }, // overwrite appsettings S3:ServiceUrl 
                        { EnvironmentVariable.S3_BUCKET, environmentVariables.S3Bucket },
                        { EnvironmentVariable.S3_ACCESS_KEY, environmentVariables.S3AccessKey },
                        { EnvironmentVariable.S3_SECRET_KEY, environmentVariables.S3SecretKey },
                        { EnvironmentVariable.RESILIENCE_POLICY_MIN_WAIT_TIME_MSECS, environmentVariables.MinWait },
                        { EnvironmentVariable.RESILIENCE_POLICY_MAX_WAIT_TIME_MSECS, environmentVariables.MaxWait },
                        { EnvironmentVariable.RESILIENCE_POLICY_MAX_RETRY_COUNT, environmentVariables.RetryCount }
                    }
                }
            };
            process.Start();
            return new LiveProcess(process);
        }

        private static void WaitForHealthcheckOK(string appName, string healthUri)
        {
            var httpClient = new HttpClient();
            const int maxIterations = 30;
            for (var i = 0; i < maxIterations; i++)
            {
                Console.WriteLine($"Wait for {appName} {i}/{maxIterations}");
                try
                {
                    var response = httpClient.GetAsync(healthUri).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch (Exception)
                {
                }

                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
        }

        private class EnvironmentVariables
        {
            public EnvironmentVariables()
            {
                AspNetCoreEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable.ASPNETCORE_ENVIRONMENT) ?? "local";
                S3ApiUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_URL) ?? "https://s3.gp2.axadmin.net";
                S3AccessKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_ACCESS_KEY) ?? "BNDW1H4EWBFDJ36H61F3";
                S3SecretKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_SECRET_KEY) ?? "WClsE8kfoP8g29yCyF6BtZeukbnEwMzVWPVDO03g";
                S3Bucket = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_BUCKET) ?? "rms-e2etest";
                MinWait = Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MIN_WAIT_TIME_MSECS) ?? "1000";
                MaxWait = Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MAX_WAIT_TIME_MSECS) ?? "3000";
                RetryCount = Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MAX_RETRY_COUNT) ?? "3";
            }
            public string S3ApiUrl { get; private set; }

            public string S3AccessKey { get; private set; }

            public string S3Bucket { get; private set; }

            public string S3SecretKey { get; private set; }

            public string MinWait { get; private set; }

            public string MaxWait { get; private set; }

            public string RetryCount { get; private set; }

            public string AspNetCoreEnvironment { get; private set; }
        }
    }
}
