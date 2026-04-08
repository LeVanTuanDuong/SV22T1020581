using SV22T1020581.Models.Partner;

namespace SV22T1020581.Admin.Models;

/// <summary>
/// Model cho dialog chuyển giao hàng (Shipping).
/// </summary>
public class OrderShippingDialogModel
{
    /// <summary>
    /// Mã đơn hàng cần chuyển giao.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Danh sách người giao hàng để chọn.
    /// </summary>
    public List<Shipper> Shippers { get; set; } = new();
}
