namespace Backend.Utilities;

/// <summary>词图快照，供相似度计算使用</summary>
public sealed record TermGraphSnapshot(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> Adjacency);

