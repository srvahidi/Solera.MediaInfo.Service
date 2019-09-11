using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly.Registry;
using Solera.MediaInfo.Service.Filters;
using Solera.MediaInfo.Service.Helpers;
using Solera.MediaInfo.Service.Middleware;
using Steeltoe.Management.CloudFoundry;

namespace Solera.MediaInfo.Service
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.TryAddSingleton<IEnvironmentConfiguration>(new EnvironmentConfiguration(Configuration));
            services.TryAddSingleton<IReadOnlyPolicyRegistry<string>, ResiliencePolicyRegistry>();
            services.AddCloudFoundryActuators(Configuration);
            services.AddScoped<ValidateModelAttribute>();
            var awsOptions = Configuration.GetAWSOptions("S3");
            awsOptions.Credentials = new EnvironmentVariablesS3Credentials();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.ConfigureCustomExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseCloudFoundryActuators();

        }
    }
}