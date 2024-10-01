using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Unawares.Tests;

[TestFixture]
public class TestLandingPageConfigurationExrtensions : WebApiFixtureBase
{
    protected override void ConfigureWebHostDefaults(IWebHostBuilder builder)
    {
        builder.Configure(app =>
        {
            app.UseLandingPage("bimse");

            app.Map("/bimse", a => a.Run(async context =>
            {
                var response = context.Response;

                response.StatusCode = 200;
                await response.WriteAsync("ok :)");
            }));
        });
    }

    [Test]
    public async Task ItWorks()
    {
        var httpRequestException = Assert.ThrowsAsync<HttpRequestException>(() => Get("/"))!;

        Console.WriteLine(httpRequestException);

        Assert.That(httpRequestException.StatusCode, Is.EqualTo((HttpStatusCode)302));
    }
}