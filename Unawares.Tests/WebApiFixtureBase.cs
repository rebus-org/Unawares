using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Testy;

namespace Unawares.Tests;

public abstract class WebApiFixtureBase : FixtureBase
{
    HttpClient _client;

    protected override void SetUp()
    {
        base.SetUp();

        var host = Host.CreateDefaultBuilder(Array.Empty<string>())
            .ConfigureWebHostDefaults(builder =>
            {
                ConfigureWebHostDefaults(builder);

                builder.UseTestServer();
            })
            .Build();

        Using(host);

        host.StartAsync();

        _client = Using(host.GetTestClient());
    }

    protected async Task Post(string route)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, route);

        using var response = await _client.SendAsync(request);

        if (response.IsSuccessStatusCode) return;

        throw new HttpRequestException(await GetResponseString(), null, response.StatusCode);

        async Task<string> GetResponseString() => await response.Content.ReadAsStringAsync();
    }

    protected abstract void ConfigureWebHostDefaults(IWebHostBuilder builder);
}