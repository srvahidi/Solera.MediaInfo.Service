using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;

namespace MockWebHost
{
    public abstract class MockHost
    {
    public Mock<MockHost> Mock { get; set; }

    public abstract Task HandlePost(Uri uri, HttpContext context);

    }
}
