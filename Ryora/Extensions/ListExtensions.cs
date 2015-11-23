using System.Collections.Generic;

namespace Ryora.Client
{
    public static class ListExtensions
    {
        public static void AddIfNotNull<T>(this List<T> list, T? value) where T : struct
        {
            if (!value.HasValue) return;
            list.Add(value.Value);
        }
    }
}
