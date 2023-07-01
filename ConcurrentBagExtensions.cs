using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FilenameInserter;

internal static class ConcurrentBagExtensions
{
    public static bool Contains<T>(this ConcurrentBag<T> bag, T obj)
    {
        bool contains = false;

        foreach (T item in bag)
        {
            if (Equals(item, obj))
            {
                contains = true;
                break;
            }
        }

        return contains;
    }

    public static ConcurrentBag<T> Remove<T>(
        this ConcurrentBag<T> bag, T obj)
    {
        if (bag.Contains(obj))
        {
            ConcurrentBag<T> updatedBag = new();

            foreach (T item in bag)
            {
                if (!Equals(item, obj))
                {
                    updatedBag.Add(item);
                }
            }

            return updatedBag;
        }

        return bag;
    }

    private static bool Equals<T>(T item1, T item2)
    {
        return EqualityComparer<T>.Default.Equals(item1, item2);
    }
}
