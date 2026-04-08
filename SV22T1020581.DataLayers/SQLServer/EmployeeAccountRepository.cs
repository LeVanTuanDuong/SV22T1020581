using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Security;

namespace SV22T1020581.DataLayers.SQLServer;

/// <summary>
/// Lõi triển khai <see cref="IUserAccountRepository"/> (dùng chung cho nhân viên / khách hàng).
/// </summary>
internal sealed class UserAccountRepositoryCore : IUserAccountRepository
{
    private readonly string _connectionString;

    public UserAccountRepositoryCore(string connectionString) => _connectionString = connectionString;

    /// <inheritdoc />
    public async Task<UserAccount?> AuthenticateEmployeeAsync(string email, string password)
    {
        await using var conn = new SqlConnection(_connectionString);
        var row = await conn.QueryFirstOrDefaultAsync<EmployeeAuthRow>(@"
                SELECT EmployeeID,
                    CAST(FullName AS NVARCHAR(200)) AS FullName,
                    CAST(Email AS NVARCHAR(256)) AS Email,
                    CAST(Photo AS NVARCHAR(255)) AS Photo,
                    CAST(RoleNames AS NVARCHAR(500)) AS RoleNames
                FROM Employees
                WHERE Email = @email AND Password = @password AND ISNULL(IsWorking, 1) = 1",
            new { email, password });
        if (row == null) return null;
        return new UserAccount
        {
            UserId = row.EmployeeID.ToString(),
            UserName = row.Email,
            Email = row.Email,
            DisplayName = row.FullName,
            Photo = row.Photo ?? "",
            RoleNames = row.RoleNames ?? ""
        };
    }

    /// <inheritdoc />
    public async Task<UserAccount?> AuthenticateCustomerAsync(string email, string password)
    {
        await using var conn = new SqlConnection(_connectionString);
        var row = await conn.QueryFirstOrDefaultAsync<CustomerAuthRow>(@"
                SELECT CustomerID,
                    CAST(CustomerName AS NVARCHAR(200)) AS CustomerName,
                    CAST(Email AS NVARCHAR(256)) AS Email
                FROM Customers
                WHERE Email = @email AND Password = @password AND ISNULL(IsLocked, 0) = 0",
            new { email, password });
        if (row == null) return null;
        return new UserAccount
        {
            UserId = row.CustomerID.ToString(),
            UserName = row.Email,
            Email = row.Email,
            DisplayName = row.CustomerName,
            Photo = "",
            RoleNames = ""
        };
    }

    private sealed class EmployeeAuthRow
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Photo { get; set; }
        public string? RoleNames { get; set; }
    }

    private sealed class CustomerAuthRow
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordEmployeeAsync(int employeeId, string oldPassword, string newPassword)
    {
        await using var conn = new SqlConnection(_connectionString);
        var n = await conn.ExecuteAsync(@"
                UPDATE Employees SET Password=@newPassword WHERE EmployeeID=@employeeId AND Password=@oldPassword",
            new { employeeId, oldPassword, newPassword });
        return n > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordCustomerAsync(int customerId, string oldPassword, string newPassword)
    {
        await using var conn = new SqlConnection(_connectionString);
        var n = await conn.ExecuteAsync(@"
                UPDATE Customers SET Password=@newPassword WHERE CustomerID=@customerId AND Password=@oldPassword",
            new { customerId, oldPassword, newPassword });
        return n > 0;
    }

    /// <inheritdoc />
    public async Task<bool> SetEmployeePasswordByEmailAsync(string email, string newPassword)
    {
        await using var conn = new SqlConnection(_connectionString);
        var n = await conn.ExecuteAsync(@"
                UPDATE Employees SET Password=@newPassword WHERE Email=@email",
            new { email, newPassword });
        return n > 0;
    }

    /// <inheritdoc />
    public async Task<bool> SetEmployeePasswordByIdAsync(int employeeId, string newPassword)
    {
        await using var conn = new SqlConnection(_connectionString);
        var n = await conn.ExecuteAsync(@"
                UPDATE Employees SET Password=@newPassword WHERE EmployeeID=@employeeId",
            new { employeeId, newPassword });
        return n > 0;
    }
}

/// <summary>
/// Truy cập dữ liệu tài khoản nhân viên (đăng nhập, đổi mật khẩu) — triển khai <see cref="IUserAccountRepository"/>.
/// </summary>
public sealed class EmployeeAccountRepository : IUserAccountRepository
{
    private readonly UserAccountRepositoryCore _core;

    /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
    public EmployeeAccountRepository(string connectionString) =>
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
