using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Security;

namespace SV22T1020581.BusinessLayers;

/// <summary>
/// Cung cấp các tính năng xử lý tài khoản khách hàng:
/// đăng nhập, đăng ký, đổi mật khẩu, quản lý thông tin cá nhân.
/// </summary>
public static class CustomerAccountService
{
    private static readonly CustomerAccountRepository _userAccountDB;
    private static readonly CustomerRepository _customerDB;

    static CustomerAccountService()
    {
        var conn = Configuration.ConnectionString;
        _userAccountDB = new CustomerAccountRepository(conn);
        _customerDB = new CustomerRepository(conn);
    }

    /// <summary>
    /// Xác thực thông tin đăng nhập của khách hàng.
    /// </summary>
    /// <param name="email">Email (tên đăng nhập).</param>
    /// <param name="password">Mật khẩu.</param>
    /// <returns>Tài khoản khách hàng hoặc null nếu không hợp lệ.</returns>
    public static async Task<UserAccount?> AuthenticateAsync(string email, string password)
    {
        return await _userAccountDB.AuthenticateCustomerAsync(email, password);
    }

    /// <summary>
    /// Đăng ký tài khoản khách hàng mới.
    /// </summary>
    /// <param name="customerName">Tên khách hàng.</param>
    /// <param name="contactName">Tên giao dịch.</param>
    /// <param name="email">Email.</param>
    /// <param name="password">Mật khẩu.</param>
    /// <param name="phone">Điện thoại.</param>
    /// <param name="province">Tỉnh/thành.</param>
    /// <param name="address">Địa chỉ.</param>
    /// <returns>Mã khách hàng được tạo, hoặc 0 nếu thất bại.</returns>
    public static async Task<int> RegisterAsync(
        string customerName, string contactName, string email, string password,
        string? phone, string? province, string? address)
    {
        if (await _customerDB.ValidateEmailAsync(email))
        {
            var customer = new Models.Partner.Customer
            {
                CustomerName = customerName,
                ContactName = contactName,
                Email = email,
                Password = password,
                Phone = phone,
                Province = province,
                Address = address,
                IsLocked = false
            };
            return await _customerDB.AddAsync(customer);
        }
        return 0;
    }

    /// <summary>
    /// Lấy thông tin khách hàng theo mã.
    /// </summary>
    /// <param name="customerId">Mã khách hàng.</param>
    /// <returns>Khách hàng hoặc null.</returns>
    public static async Task<Models.Partner.Customer?> GetCustomerAsync(int customerId)
    {
        return await _customerDB.GetAsync(customerId);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân khách hàng.
    /// </summary>
    /// <param name="customer">Dữ liệu khách hàng cần cập nhật.</param>
    /// <returns>true nếu thành công.</returns>
    public static async Task<bool> UpdateCustomerAsync(Models.Partner.Customer customer)
    {
        return await _customerDB.UpdateAsync(customer);
    }

    /// <summary>
    /// Đổi mật khẩu khách hàng.
    /// </summary>
    /// <param name="customerId">Mã khách hàng.</param>
    /// <param name="oldPassword">Mật khẩu cũ.</param>
    /// <param name="newPassword">Mật khẩu mới.</param>
    /// <returns>true nếu thành công.</returns>
    public static async Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword)
    {
        return await _userAccountDB.ChangePasswordCustomerAsync(customerId, oldPassword, newPassword);
    }

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa.
    /// </summary>
    /// <param name="email">Email cần kiểm tra.</param>
    /// <param name="excludeId">Mã khách hàng cần loại trừ (khi cập nhật).</param>
    /// <returns>true nếu email có thể sử dụng.</returns>
    public static async Task<bool> ValidateEmailAsync(string email, int excludeId = 0)
    {
        return await _customerDB.ValidateEmailAsync(email, excludeId);
    }
}
