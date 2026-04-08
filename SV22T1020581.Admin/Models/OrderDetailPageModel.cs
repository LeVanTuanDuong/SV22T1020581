using SV22T1020581.Models.HR;
using SV22T1020581.Models.Partner;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.Admin.Models;

/// <summary>
/// Model cho trang chi tiết đơn hàng.
/// </summary>
public class OrderDetailPageModel
{
    /// <summary>
    /// Thông tin đơn hàng.
    /// </summary>
    public OrderViewInfo Order { get; set; } = new();

    /// <summary>
    /// Thông tin khách hàng (nullable).
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Thông tin nhân viên phụ trách đơn hàng (nullable).
    /// </summary>
    public Employee? Employee { get; set; }

    /// <summary>
    /// Danh sách chi tiết mặt hàng trong đơn hàng.
    /// </summary>
    public List<OrderDetailViewInfo> Details { get; set; } = new();
}
