using System;
using System.Collections.Generic;
using System.Net;
using Unawares.Internals;

namespace Unawares;

/// <summary>
/// Mapper builder that helps with configuring which exceptions to map
/// </summary>
public class ExceptionMapper
{
    readonly List<ExceptionMapping> _mappings = new();

    /// <summary>
    /// Maps <typeparamref name="TException"/> to <paramref name="status"/>
    /// </summary>
    public ExceptionMapper Map<TException>(HttpStatusCode status, bool logExceptionDetails = true) where TException : Exception
    {
        _mappings.Add(new ExceptionMapping(typeof(TException), status, JustDoIt, logExceptionDetails));
        return this;
    }

    /// <summary>
    /// Maps <typeparamref name="TException"/> to <paramref name="status"/> if the exception satisties the given <paramref name="criteria"/>
    /// </summary>
    public ExceptionMapper Map<TException>(HttpStatusCode status, Func<TException, bool> criteria, bool logExceptionDetails = true) where TException : Exception
    {
        _mappings.Add(new ExceptionMapping(typeof(TException), status, exception => exception is TException specificException && criteria(specificException), logExceptionDetails));
        return this;
    }

    internal record ExceptionMapping(Type ExceptionType, HttpStatusCode StatusCode, Func<Exception, bool> Criteria, bool LogExceptionDetails)
    {
        public bool MustHandle(Exception exception) => ExceptionType.IsInstanceOfType(exception) && Criteria(exception);
    }

    internal ExceptionMappings GetMappings() => new(_mappings);

    static bool JustDoIt(Exception _) => true;
}