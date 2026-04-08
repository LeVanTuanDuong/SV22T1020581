using System.Text.Json;

namespace SV22T1020581.Models.Security;

/// <summary>
/// Cung cấp các phương thức serialize/deserialize đối tượng UserAccount
/// để lưu trữ vào Session.
/// </summary>
public static class UserAccountSessionJson
{
    /// <summary>
    /// Serialize đối tượng UserAccount thành chuỗi JSON.
    /// </summary>
    /// <param name="user">Đối tượng UserAccount.</param>
    /// <returns>Chuỗi JSON.</returns>
    public static string Serialize(UserAccount user)
    {
        return JsonSerializer.Serialize(user);
    }

    /// <summary>
    /// Deserialize chuỗi JSON thành đối tượng UserAccount.
    /// </summary>
    /// <param name="json">Chuỗi JSON.</param>
    /// <returns>Đối tượng UserAccount hoặc null nếu thất bại.</returns>
    public static UserAccount? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<UserAccount>(json);
        }
        catch
        {
            return null;
        }
    }
}
