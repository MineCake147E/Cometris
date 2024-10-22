using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Tests
{
    internal static class LinqUtils
    {
        public static IEnumerable<(T First, T Second)> ZipAdjacent<T>(this IEnumerable<T> values)
        {
            var en = values.GetEnumerator();
            if (!en.MoveNext()) yield break;
            var p0 = en.Current;
            while (en.MoveNext())
            {
                var current = en.Current;
                yield return (p0, current);
                p0 = current;
            }
        }

        public static ParallelQuery<(T First, T Second)> ZipAdjacent<T>(this OrderedParallelQuery<T> values)
        {
            var en = values.SkipWhile(a => false).Aggregate(default((ParallelQuery<(T First, T Second)>? zipped, List<(T First, T Second)>? chunk, (T first, T last)? edge)), (z, item) =>
            {
                if (z.edge.HasValue)
                {
                    var newItem = (First: z.edge.Value.last, Srcond: item);
                    //Console.WriteLine($"Appending {newItem}");
                    var list = z.chunk;
                    list?.Add(newItem);
                    list ??= new([newItem]);
                    return (z.zipped, list, (z.edge.Value.first, item));
                }
                else
                {
                    return (z.zipped, z.chunk, (item, item));
                }
            }, combineAccumulatorsFunc: (c0, c1) =>
            {
                var e0 = c0.edge;
                var e1 = c1.edge;
                ((T First, T Second)? newItem, (T first, T last)? newEdge) merged = (e0, e1) switch
                {
                    ({ } v0, { } v1) => ((v0.last, v1.first), (v0.first, v1.last)),
                    ({ } v0, null) => (null, v0),
                    _ => (null, e1),
                };
                var z0 = c0.zipped;
                var z1 = c1.zipped;
                var l0 = c0.chunk;
                var l1 = c1.chunk;
                l0 = l0.AddOrCreateIfNotNull(merged.newItem);
                var merging = (l0, z1) switch
                {
                    (null, null) => null,
                    ({ } lv0, null) => lv0.AsParallel(),
                    (null, { } zv1) => zv1,
                    _ => l0.AsParallel().Concat(z1)
                };
                z0 = merging is not null ? z0?.Concat(merging) ?? merging : z0;
                return (z0, l1, merged.newEdge);
            }, a => a.chunk is not null ? a.zipped?.Concat(a.chunk.AsParallel()) ?? a.chunk.AsParallel() : null);
            return en ?? ParallelEnumerable.Empty<(T First, T Second)>();
        }

        public static bool AdjacentElementAnyEquals<T>(this ParallelQuery<T> values, Func<T, T, bool> predicate)
        {
            var en = values.SkipWhile(a => false).Aggregate(default((bool result, (T first, T last)? edge)), (z, item) => z.edge.HasValue ? ((bool result, (T first, T last)? edge))(z.result || predicate(z.edge.Value.last, item), (z.edge.Value.first, item)) : ((bool result, (T first, T last)? edge))(z.result, (item, item))
            , combineAccumulatorsFunc: (c0, c1) =>
            {
                var e0 = c0.edge;
                var e1 = c1.edge;
                ((T First, T Second)? newItem, (T first, T last)? newEdge) merged = (e0, e1) switch
                {
                    ({ } v0, { } v1) => ((v0.last, v1.first), (v0.first, v1.last)),
                    ({ } v0, null) => (null, v0),
                    _ => (null, e1),
                };
                var z0 = c0.result;
                var z1 = c1.result;
                return (z0 || z1 || merged.newItem.HasValue && predicate(merged.newItem.Value.First, merged.newItem.Value.Second), merged.newEdge);
            }, a => a.result || a.edge.HasValue && predicate(a.edge.Value.first, a.edge.Value.last));
            return en;
        }

        private static List<T>? AddOrCreateIfNotNull<T>(this List<T>? list, T? elementToAdd) where T : class
        {
            if (elementToAdd is null) return list;
            list?.Add(elementToAdd);
            list ??= new([elementToAdd]);
            return list;
        }

        private static List<T>? AddOrCreateIfNotNull<T>(this List<T>? list, T? elementToAdd) where T : struct
        {
            if (!elementToAdd.HasValue) return list;
            list?.Add(elementToAdd.Value);
            list ??= new([elementToAdd.Value]);
            return list;
        }

        private static List<T> AddOrCreate<T>(this List<T>? list, T elementToAdd)
        {
            list?.Add(elementToAdd);
            list ??= new([elementToAdd]);
            return list;
        }

        internal static IEnumerable<(T, T)> GenerateAllTwoCombinationsOf<T>(T[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                for (int j = i + 1; j < values.Length; j++)
                {
                    yield return (values[i], values[j]);
                }
            }
        }
    }
}
