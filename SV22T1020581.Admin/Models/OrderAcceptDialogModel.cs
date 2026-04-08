using SV22T1020581.Models.HR;

namespace SV22T1020581.Admin.Models;

/// <summary>
/// Model cho dialog duyệt đơn hàng (Accept).
/// </summary>
public class OrderAcceptDialogModel
{
    /// <summary>
    /// Mã đơn hàng cần duyệt.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Danh sách nhân viên để chọn phụ trách.
    /// </summary>
    public List<Employee> Employees { get; set; } = new();
}
