namespace Mimir.Application.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0");

        var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return ChunkByIterator(enumerator, chunkSize - 1);
        }
    }

    private static IEnumerable<T> ChunkByIterator<T>(IEnumerator<T> source, int chunkSize)
    {
        yield return source.Current;

        for (var i = 0; i < chunkSize && source.MoveNext(); i++)
        {
            yield return source.Current;
        }
    }
}