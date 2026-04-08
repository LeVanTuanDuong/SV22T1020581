namespace SV22T1020581.DataLayers;

/// <summary>
/// Tiện ích mã hóa / bảo mật dữ liệu (mở rộng theo nghiệp vụ nếu cần).
/// </summary>
public static class CryptHelper
{
}

/// <summary>
/// Pattern LIKE an toàn cho truy vấn SQL (escape %, _, [).
/// </summary>
internal static class SqlLike
{
    /// <summary>
    /// Tạo pattern LIKE an toàn (escape %, _, [).
    /// </summary>
    public static string Pattern(string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return "";
        var s = search.Trim();
        s = s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        return "%" + s + "%";
    }
}
