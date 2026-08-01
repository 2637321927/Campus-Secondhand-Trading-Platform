using System.Text;

namespace Backend.Utilities;

public static class ShareCodeHelper
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string Prefix = "campus_";   //前端检测这个前缀才跳转

    /// <summary>
    /// 生成当天有效的分享码，带 campus_ 前缀（例：campus_aB3x）
    /// </summary>
    public static string Encode(long productId)
    {
        long num = productId ^ GetTodayKey();
        return Prefix + ToBase62(num);
    }

    /// <summary>
    /// 解码分享码，必须带前缀，过期自动失效
    /// </summary>
    public static long Decode(string code)
    {
        if (!code.StartsWith(Prefix))
            throw new ArgumentException("Invalid share code");
        long num = FromBase62(code.Substring(Prefix.Length));
        return num ^ GetTodayKey();
    }

    private static long GetTodayKey() => long.Parse(DateTime.Now.ToString("yyyyMMdd"));

    private static string ToBase62(long num)
    {
        if (num == 0) return Chars[0].ToString();
        var list = new List<char>();
        while (num > 0)
        {
            list.Add(Chars[(int)(num % 62)]);
            num /= 62;
        }
        list.Reverse();
        return new string(list.ToArray());
    }

    private static long FromBase62(string code)
    {
        long num = 0;
        foreach (char c in code)
        {
            int idx = Chars.IndexOf(c);
            if (idx < 0) throw new ArgumentException("Invalid share code");
            num = num * 62 + idx;
        }
        return num;
    }
}
