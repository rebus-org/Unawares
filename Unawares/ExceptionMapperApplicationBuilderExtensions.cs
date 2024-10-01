using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Unawares.Internals;

namespace Unawares;

/// <summary>
/// Extensions for mapping exceptions to HTTP status codes
/// </summary>
public static class ExceptionMapperApplicationBuilderExtensions
{
    /// <summary>
    /// Installs the exception mapper middleware, using the configuration built by <paramref name="configure"/> to map caught exceptions to HTTP status codes.
    /// </summary>
    public static void UseExceptionMapper(this IApplicationBuilder app, Action<ExceptionMapper> configure)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var builder = new ExceptionMapper();

        configure(builder);

        var mappings = builder.GetMappings();

        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception exception) when (mappings.TryGetMapper(exception, out var mapper))
            {
                var response = context.Response;

                var statusCodeEnum = mapper.StatusCode;
                var statusCodeNumber = (int)statusCodeEnum;

                if (response.HasStarted)
                {
                    GetLogger()?.LogWarning("Could not map {method} {url} {exceptionType} to status {status} ({statusName}) (with text {text}), because the response headers were already sent!",
                        exception.GetType().Name, context.Request.Method, context.Request.GetDisplayUrl(),
                        statusCodeNumber, statusCodeEnum, exception.Message);

                    return;
                }

                response.StatusCode = statusCodeNumber;

                var responseFeature = context.Features.Get<IHttpResponseFeature>();

                if (responseFeature != null)
                {
                    responseFeature.ReasonPhrase = exception.Message.Replace(Environment.NewLine, " -- ");
                }

                await response.WriteAsJsonAsync(new ErrorResponse(statusCodeNumber, exception.Message));

                if (mapper.LogExceptionDetails)
                {
                    GetLogger()?.LogInformation(
                        exception,
                        "Mapped {method} {url} {exceptionType} to status {status} ({statusName})",
                        exception.GetType().Name, context.Request.Method, context.Request.GetDisplayUrl(),
                        statusCodeNumber, statusCodeEnum);
                }
                else
                {
                    GetLogger()?.LogInformation(
                        "Mapped {method} {url} {exceptionType} to status {status} ({statusName}): {text}",
                        exception.GetType().Name, context.Request.Method, context.Request.GetDisplayUrl(),
                        statusCodeNumber, statusCodeEnum, exception.Message);
                }

                ILogger GetLogger()
                {
                    var LoggerFactory = context.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                    var logger = LoggerFactory?.CreateLogger(typeof(ExceptionMapperApplicationBuilderExtensions));
                    return logger;
                }
            }
        });
    }
}