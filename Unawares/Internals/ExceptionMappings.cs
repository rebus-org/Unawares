using System;
using System.Collections.Generic;
// ReSharper disable ForCanBeConvertedToForeach

namespace Unawares.Internals;

class ExceptionMappings
{
    readonly IReadOnlyList<ExceptionMapper.ExceptionMapping> _mappings;

    public ExceptionMappings(List<ExceptionMapper.ExceptionMapping> mappings)
    {
        _mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
    }

    public bool TryGetMapper(Exception exception, out ExceptionMapper.ExceptionMapping result)
    {
        for (var index = 0; index < _mappings.Count; index++)
        {
            var mapping = _mappings[index];

            if (!mapping.MustHandle(exception)) continue;

            result = mapping;
            return true;
        }

        result = null;
        return false;
    }
}