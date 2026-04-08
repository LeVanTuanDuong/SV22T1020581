using SV22T1020581.Models.Security;

namespace SV22T1020581.DataLayers.Interfaces;

/// <summary>
/// ??nh ngh?a c�c thao t�c d? li?u li�n quan ??n t�i kho?n ??ng nh?p (nh�n vi�n / kh�ch h�ng).
/// </summary>
public interface IUserAccountRepository
{
    /// <summary>X�c th?c nh�n vi�n theo email v� m?t kh?u.</summary>
    Task<UserAccount?> AuthenticateEmployeeAsync(string email, string password);

    /// <summary>X�c th?c kh�ch h�ng theo email v� m?t kh?u.</summary>
    Task<UserAccount?> AuthenticateCustomerAsync(string email, string password);

    /// <summary>??i m?t kh?u nh�n vi�n khi bi?t m?t kh?u c?.</summary>
    Task<bool> ChangePasswordEmployeeAsync(int employeeId, string oldPassword, string newPassword);

    /// <summary>??i m?t kh?u kh�ch h�ng khi bi?t m?t kh?u c?.</summary>
    Task<bool> ChangePasswordCustomerAsync(int customerId, string oldPassword, string newPassword);

    /// <summary>C?p nh?t m?t kh?u nh�n vi�n theo email (sau khi ?� x�c th?c m?t kh?u c?).</summary>
    Task<bool> SetEmployeePasswordByEmailAsync(string email, string newPassword);

    /// <summary>??t m?t kh?u m?i cho nh�n vi�n theo m� (qu?n tr? ??i m?t kh?u).</summary>
    Task<bool> SetEmployeePasswordByIdAsync(int employeeId, string newPassword);
}
