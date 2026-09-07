namespace Backend.Utilities;

/// <summary>一个近似词及其余弦相似度。</summary>
public sealed record WeightedTerm(string Term, double Similarity);

