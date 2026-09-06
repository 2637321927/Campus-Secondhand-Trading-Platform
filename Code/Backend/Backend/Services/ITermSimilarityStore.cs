using Backend.Utilities;

namespace Backend.Services;

public interface ITermSimilarityStore
{
    SimilaritySnapshot Current { get; }
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task ReplaceAsync(IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> values,
        CancellationToken cancellationToken = default);
}

public sealed record SimilaritySnapshot(
    IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> Terms)
{
    public static SimilaritySnapshot Empty { get; } =
        new(new Dictionary<string, IReadOnlyList<WeightedTerm>>(StringComparer.OrdinalIgnoreCase));
}
