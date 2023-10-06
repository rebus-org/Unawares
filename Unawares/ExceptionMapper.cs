using System;
using System.Collections.Generic;
using System.Net;
using Unawares.Internals;

namespace Unawares;

public class ExceptionMapper
{
    readonly List<ExceptionMapping> _mappings = new();

    public ExceptionMapper Map<TException>(HttpStatusCode status) where TException : Exception
    {
        _mappings.Add(new ExceptionMapping(typeof(TException), status, JustDoIt));
        return this;
    }

    public ExceptionMapper Map<TException>(HttpStatusCode status, Func<TException, bool> criteria) where TException : Exception
    {
        _mappings.Add(new ExceptionMapping(typeof(TException), status, exception => exception is TException specificException && criteria(specificException)));
        return this;
    }

    internal record ExceptionMapping(Type ExceptionType, HttpStatusCode StatusCode, Func<Exception, bool> Criteria)
    {
        public bool MustHandle(Exception exception) => ExceptionType.IsInstanceOfType(exception) && Criteria(exception);
    }

    internal ExceptionMappings GetMappings() => new(_mappings);

    static bool JustDoIt(Exception _) => true;
}