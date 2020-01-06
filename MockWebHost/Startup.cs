using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MockWebHost
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var mock = new Mock<MockHost>();
			mock.Object.Mock = mock;
			services.AddSingleton(mock.Object);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.Use(async (context, next) =>
			{
				if (context.Request.Path != "/api/health") await next();
			});

			app.UseMiddleware<MockWebHostMiddleware>();
		}
	}
}