using System;
using System.Collections.Generic;

namespace Lueben.Microservice.Api.Idempotency.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int size)
        {
            for (var i = 0; i < list.Count; i += size)
            {
                yield return list.GetRange(i, Math.Min(size, list.Count - i));
            }
        }
    }
}