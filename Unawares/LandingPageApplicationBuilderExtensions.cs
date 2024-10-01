using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Unawares;

/// <summary>
/// Configuration extensions for configuring the landing page
/// </summary>
public static class LandingPageApplicationBuilderExtensions
{
    /// <summary>
    /// Captures requests to the route "/" and redirects (HTTP 302) to <paramref name="route"/>
    /// </summary>
    public static void UseLandingPage(this IApplicationBuilder app, string route)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        app.Use(async (context, next) =>
        {
            var request = context.Request;

            if (request.Path == new PathString("/"))
            {
                context.Response.Redirect(route);
                return;
            }

            await next();
        });
    }
}