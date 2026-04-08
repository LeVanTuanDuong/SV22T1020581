using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Security;

namespace SV22T1020581.DataLayers.SQLServer;

/// <summary>
/// Truy cập dữ liệu tài khoản khách hàng (đăng nhập, đổi mật khẩu) — triển khai <see cref="IUserAccountRepository"/>.
/// </summary>
public sealed class CustomerAccountRepository : IUserAccountRepository
{
    private readonly UserAccountRepositoryCore _core;

    /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
    public CustomerAccountRepository(string connectionString) =>
        _core = new UserAccountRepositoryCore(connectionString);

    /// <inheritdoc />
    public Task<UserAccount?> AuthenticateEmployeeAsync(string email, string password) =>
        _core.AuthenticateEmployeeAsync(email, password);

    /// <inheritdoc />
    public Task<UserAccount?> AuthenticateCustomerAsync(string email, string password) =>
        _core.AuthenticateCustomerAsync(email, password);

    /// <inheritdoc />
    public Task<bool> ChangePasswordEmployeeAsync(int employeeId, string oldPassword, string newPassword) =>
        _core.ChangePasswordEmployeeAsync(employeeId, oldPassword, newPassword);

    /// <inheritdoc />
    public Task<bool> ChangePasswordCustomerAsync(int customerId, string oldPassword, string newPassword) =>
        _core.ChangePasswordCustomerAsync(customerId, oldPassword, newPassword);

    /// <inheritdoc />
    public Task<bool> SetEmployeePasswordByEmailAsync(string email, string newPassword) =>
        _core.SetEmployeePasswordByEmailAsync(email, newPassword);

    /// <inheritdoc />
    public Task<bool> SetEmployeePasswordByIdAsync(int employeeId, string newPassword) =>
        _core.SetEmployeePasswordByIdAsync(employeeId, newPassword);
}
