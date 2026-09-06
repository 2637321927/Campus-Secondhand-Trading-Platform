namespace Backend.Utilities;

/// <summary>余弦相似度计算</summary>
public sealed class TermSimilarityCalculator : ITermSimilarityCalculator
{
    public IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> Calculate(
        TermGraphSnapshot snapshot,
        double minTermTotalWeight = 20.0,
        int topK = 3,
        CancellationToken cancellationToken = default)
    {
        if (topK <= 0 || snapshot.Adjacency.Count == 0)
            return new Dictionary<string, IReadOnlyList<WeightedTerm>>(StringComparer.OrdinalIgnoreCase);

        var eligible = snapshot.Adjacency
            .Where(pair => pair.Value.Values.All(IsValidWeight) &&
                           pair.Value.Values.Sum() >= minTermTotalWeight)
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (eligible.Count < 2)
            return new Dictionary<string, IReadOnlyList<WeightedTerm>>(StringComparer.OrdinalIgnoreCase);

        var norms = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var term in eligible)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sum = snapshot.Adjacency[term].Values
                .Where(IsValidWeight)
                .Sum(weight => weight * weight);
            if (sum > 0 && double.IsFinite(sum))
                norms[term] = Math.Sqrt(sum);
        }

        var dotProducts = new Dictionary<(string First, string Second), double>();
        foreach (var context in snapshot.Adjacency)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targets = context.Value
                .Where(pair => eligible.Contains(pair.Key) && IsValidWeight(pair.Value))
                .Select(pair => (pair.Key, pair.Value))
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var i = 0; i < targets.Count; i++)
            {
                for (var j = i + 1; j < targets.Count; j++)
                {
                    var first = targets[i].Key;
                    var second = targets[j].Key;
                    var key = (first, second);
                    dotProducts[key] = dotProducts.GetValueOrDefault(key) +
                                       targets[i].Value * targets[j].Value;
                }
            }
        }

        var best = new Dictionary<string, List<WeightedTerm>>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in dotProducts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!norms.TryGetValue(pair.Key.First, out var firstNorm) ||
                !norms.TryGetValue(pair.Key.Second, out var secondNorm) ||
                firstNorm <= 0 || secondNorm <= 0 || pair.Value <= 0)
                continue;

            var similarity = pair.Value / (firstNorm * secondNorm);
            if (!double.IsFinite(similarity) || similarity <= 0)
                continue;

            similarity = Math.Clamp(similarity, 0.0, 1.0);
            AddTop(best, pair.Key.First, new WeightedTerm(pair.Key.Second, similarity), topK);
            AddTop(best, pair.Key.Second, new WeightedTerm(pair.Key.First, similarity), topK);
        }

        return best.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<WeightedTerm>)pair.Value
                .OrderByDescending(x => x.Similarity)
                .ThenBy(x => x.Term, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static void AddTop(Dictionary<string, List<WeightedTerm>> result,
        string source, WeightedTerm candidate, int topK)
    {
        if (!result.TryGetValue(source, out var list))
            result[source] = list = new List<WeightedTerm>();

        var existing = list.FindIndex(x => string.Equals(x.Term, candidate.Term, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0)
        {
            if (list[existing].Similarity >= candidate.Similarity) return;
            list.RemoveAt(existing);
        }

        list.Add(candidate);
        list.Sort((left, right) =>
        {
            var similarity = right.Similarity.CompareTo(left.Similarity);
            return similarity != 0
                ? similarity
                : StringComparer.OrdinalIgnoreCase.Compare(left.Term, right.Term);
        });
        if (list.Count > topK)
            list.RemoveRange(topK, list.Count - topK);
    }

    private static bool IsValidWeight(double value) => double.IsFinite(value) && value > 0;
}
