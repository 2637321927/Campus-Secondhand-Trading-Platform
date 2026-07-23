using System.Text.RegularExpressions;
using JiebaNet.Segmenter;

namespace Backend.Utilities;

/// <summary>
/// 词条提取
/// </summary>
public partial class TermExtractionService : ITermExtractionService
{
    private readonly JiebaSegmenter _segmenter;
    private readonly HashSet<string> _stopwords;
    private readonly HashSet<string> _singleCharWhitelist;
    private readonly Dictionary<string, string> _synonymMap;

    private static readonly HashSet<string> DropPosTags = new()
    {
        "u",  // 助词
        "p",  // 介词
        "c",  // 连词
        "e",  // 叹词
        "o",  // 拟声词
        "w",  // 标点
        "y",  // 语气词
        "r",  // 代词
        "d",  // 副词
    };

    public TermExtractionService()
    {
        _segmenter = new JiebaSegmenter();

        var dictDir = Path.Combine(AppContext.BaseDirectory, "Dictionaries");

        TryLoadUserDict(Path.Combine(dictDir, "dict.txt"));

        _stopwords = LoadLines("Dictionaries/stopwords.txt");
        _singleCharWhitelist = LoadLines("Dictionaries/single_char_whitelist.txt");
        _synonymMap = LoadSynonymMap("Dictionaries/synonym_map.txt");
    }

    public List<string> Extract(string text)
    {

        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // 预处理
        text = Preprocess(text);

        // jieba 分词
        var terms = Segment(text);

        // 停用词过滤
        terms = FilterStopwords(terms);

        // 丢弃 posTag
        var words = FilterByPosTag(terms);

        // 长度过滤 去重
        words = FilterByLengthAndDedup(words);

        // 同义词归并
        words = NormalizeSynonyms(words);

        return words;

    }

    /// <summary>
    /// 文本预处理
    /// </summary>
    private static string Preprocess(string text)
    {
        // 全角字符转半角
        text = FullToHalfRegex().Replace(text, m =>
        {
            var c = m.Value[0];
            if (c >= '！' && c <= '～')
                return ((char)(c - 0xFEE0)).ToString();
            if (c == '　')
                return " ";
            return m.Value;
        });

        // 连续标点压缩为单个
        text = RepeatedPunctRegex().Replace(text, "$1");

        return text.Trim();
    }

    [GeneratedRegex(@"[！-～　]")]
    private static partial Regex FullToHalfRegex();

    [GeneratedRegex(@"([!！?？。，,;；:：""'']>】\]\}\)\)])\1+")]
    private static partial Regex RepeatedPunctRegex();

    private void TryLoadUserDict(string path)
    {
        if (File.Exists(path))
            _segmenter.LoadUserDict(path);
    }

    /// <summary>
    /// 调用jieba PosSegmenter分词
    /// </summary>
    private static List<(string word, string? posTag)> Segment(string text)
    {
        var segmenter = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var segments = segmenter.Cut(text);

        var result = new List<(string word, string? posTag)>();

        foreach (var t in segments)
        {

            var raw = t.ToString()!;
            var idx = raw.LastIndexOf('/');
            if (idx > 0 && idx < raw.Length - 1)
            {
                var word = raw[..idx].Trim();
                var tag  = raw[(idx + 1)..].Trim();
                if (!string.IsNullOrEmpty(word))
                    result.Add((word, tag));
            }
            else
            {
                var word = raw.Trim();
                if (!string.IsNullOrEmpty(word))
                    result.Add((word, null));
            }
            
        }

        return result;
    }

    /// <summary>过滤停用词</summary>
    private List<(string word, string? posTag)> FilterStopwords(List<(string word, string? posTag)> terms)
    {
        return terms
            .Where(t => !_stopwords.Contains(t.word))
            .ToList();
    }

    /// <summary>
    /// 按词性标签过滤
    /// </summary>
    private static List<string> FilterByPosTag(List<(string word, string? posTag)> terms)
    {
        var result = new List<string>();

        foreach (var (word, tag) in terms)
        {
            if (tag == null)
            {
                result.Add(word);
                continue;
            }

            if (DropPosTags.Contains(tag))
                continue;

            result.Add(word);
        }

        return result;
    }

    private List<string> FilterByLengthAndDedup(List<string> terms)
    {
        var seen = new HashSet<string>();
        var result = new List<string>();

        foreach (var t in terms)
        {
            if (string.IsNullOrWhiteSpace(t))
                continue;

            if (t.Length == 1 && !_singleCharWhitelist.Contains(t))
                continue;

            if (seen.Add(t))
                result.Add(t);
        }

        return result;
    }

    private List<string> NormalizeSynonyms(List<string> terms)
    {
        return terms
            .Select(t => _synonymMap.TryGetValue(t, out var canonical) ? canonical : t)
            .Distinct()
            .ToList();
    }

    private static HashSet<string> LoadLines(string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(path))
            return new HashSet<string>();

        return File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith('#'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> LoadSynonymMap(string relativePath)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);

        if (!File.Exists(path))
            return map;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split("→");
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    map[key] = value;
            }
        }

        return map;
    }
}
