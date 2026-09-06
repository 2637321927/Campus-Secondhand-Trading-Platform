namespace Backend.Utilities;

public interface ITermSimilarityCalculator
{
    IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> Calculate(
        TermGraphSnapshot snapshot,
        double minTermTotalWeight = 20.0,
        int topK = 3,
        CancellationToken cancellationToken = default);
}
