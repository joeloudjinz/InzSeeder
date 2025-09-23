namespace InzSeeder.Core.Utilities;

/// <summary>
/// Provides extension methods for working with enumerables.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Batches the elements of a sequence into lists of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to batch.</param>
    /// <param name="batchSize">The maximum size of each batch.</param>
    /// <returns>An enumerable of lists, where each list contains up to batchSize elements.</returns>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");

        var batch = new List<T>(batchSize);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count != batchSize) continue;
            yield return batch;
            batch = new List<T>(batchSize);
        }

        if (batch.Count > 0) yield return batch;
    }
}