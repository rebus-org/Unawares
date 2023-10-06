using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Unawares.Tests;

[TestFixture]
public class TestExceptionMapperApplicationBuilderExtensions : WebApiFixtureBase
{
    protected override void ConfigureWebHostDefaults(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                services.AddLogging(logging => logging.AddConsole());
            })

            .Configure(app =>
            {
                app.UseExceptionMapper(
                    mapper => mapper
                        .Map<Exception1>(status: HttpStatusCode.BadRequest)
                        .Map<Exception2>(status: HttpStatusCode.NotFound)
                        .Map<Exception3>(status: HttpStatusCode.BadGateway, criteria: e => e.Message.Contains("123"))
                        .Map<Exception>(status: HttpStatusCode.InternalServerError) //<fallback
                );

                app.Map("/throw", a => a.Run(async context =>
                {
                    var request = context.Request;
                    var response = context.Response;

                    if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                    {
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        return;
                    }

                    var what = request.Query["what"].FirstOrDefault();

                    switch (what)
                    {
                        case nameof(Exception1):
                            throw new Exception1();
                            break;
                        case nameof(Exception2):
                            throw new Exception2();
                            break;
                        case nameof(Exception3):
                            throw new Exception3(request.Query["message"].FirstOrDefault() ?? "???");
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                    }
                }));
            });
    }

    [Test]
    public async Task CanMapToDifferentHttpStatusCodes()
    {
        var ex1 = Assert.ThrowsAsync<HttpRequestException>(() => Post("throw?what=Exception1"));
        var ex2 = Assert.ThrowsAsync<HttpRequestException>(() => Post("throw?what=Exception2"));

        Assert.That(ex1.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(ex2.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CanFilterByCriteria_Match()
    {
        var ex1 = Assert.ThrowsAsync<HttpRequestException>(() => Post("throw?what=Exception3&message=hej123"));

        Assert.That(ex1.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
    }

    [Test]
    public async Task CanFilterByCriteria_Fallback()
    {
        var ex2 = Assert.ThrowsAsync<HttpRequestException>(() => Post("throw?what=Exception3&message=farvel"));

        Assert.That(ex2.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    class Exception1 : Exception { }

    class Exception2 : Exception { }

    class Exception3 : Exception
    {
        public Exception3(string message) : base(message)
        {
        }
    }

}