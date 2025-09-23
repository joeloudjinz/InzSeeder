using InzSeeder.Core.Utilities;

namespace InzSeeder.Core.Tests.Utilities;

public class EnumerableExtensionsTests
{
    [Fact]
    public void Batch_WithEmptyCollection_ReturnsNoBatches()
    {
        // Arrange
        var source = new List<int>();

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Batch_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source!.Batch(3).ToList());
    }

    [Fact]
    public void Batch_WithZeroBatchSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.Batch(0).ToList());
    }

    [Fact]
    public void Batch_WithNegativeBatchSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.Batch(-1).ToList());
    }

    [Fact]
    public void Batch_WithSingleElement_ReturnsOneBatchWithOneElement()
    {
        // Arrange
        var source = new List<int> { 42 };

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        Assert.Single(result);
        Assert.Single(result[0]);
        Assert.Equal(42, result[0][0]);
    }

    [Fact]
    public void Batch_WithExactBatchSize_ReturnsCompleteBatches()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal([1, 2, 3], result[0]);
        Assert.Equal([4, 5, 6], result[1]);
    }

    [Fact]
    public void Batch_WithPartialBatch_ReturnsIncompleteFinalBatch()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal([1, 2, 3], result[0]);
        Assert.Equal([4, 5], result[1]);
    }

    [Fact]
    public void Batch_WithLargeBatchSize_ReturnsSingleBatch()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = source.Batch(10).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal([1, 2, 3, 4, 5], result[0]);
    }

    [Fact]
    public void Batch_WithLazyEvaluation_WorksCorrectly()
    {
        // Arrange
        var source = Enumerable.Range(1, 1000);

        // Act
        var batches = source.Batch(100);

        // Assert - Only enumerate a few batches to test lazy evaluation
        var firstBatch = batches.First();
        Assert.Equal(100, firstBatch.Count);
        Assert.Equal(Enumerable.Range(1, 100), firstBatch);
    }
}