namespace SV22T1020581.BusinessLayers;

/// <summary>
/// Cấu hình truy cập dữ liệu cho BusinessLayer.
/// Connection string được đọc từ appsettings.json qua IConfiguration.
/// </summary>
public static class Configuration
{
    private static string? _connectionString;
    public static string ConnectionString =>
        _connectionString
        ?? throw new InvalidOperationException("Connection string chưa được khởi tạo. Gọi Initialize() trước.");

    public static void Initialize(string connectionString)
    {
        _connectionString = connectionString;
    }
}
