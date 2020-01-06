using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MockWebHost
{
    public class MockWebHostMiddleware
	{
		public MockWebHostMiddleware(RequestDelegate next) { }

		public async Task InvokeAsync(HttpContext context)
		{
			var request = context.Request;
			var mockHost = context.RequestServices.GetService<MockHost>();
			switch (request.Method)
			{
				case "POST":
				{
                    await mockHost.HandlePost(new Uri(request.GetDisplayUrl()), context);
					break;
				}
            }
		}
    }
}
