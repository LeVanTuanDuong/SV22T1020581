using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.BusinessLayers;

/// <summary>
/// Cung cấp các tính năng xử lý đơn hàng cho khách hàng:
/// tạo đơn hàng từ giỏ hàng, xem đơn hàng, theo dõi trạng thái, lịch sử mua hàng.
/// </summary>
public static class OrderService
{
    private static readonly OrderRepository _orderDB;

    static OrderService()
    {
        _orderDB = new OrderRepository(Configuration.ConnectionString);
    }

    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng của khách hàng.
    /// Sau khi tạo thành công, trả về mã đơn hàng.
    /// </summary>
    /// <param name="customerId">Mã khách hàng.</param>
    /// <param name="deliveryProvince">Tỉnh/thành giao hàng.</param>
    /// <param name="deliveryAddress">Địa chỉ giao hàng.</param>
    /// <param name="cartLines">Các dòng trong giỏ hàng.</param>
    /// <returns>Mã đơn hàng được tạo, hoặc 0 nếu thất bại.</returns>
    public static async Task<int> CreateOrderAsync(
        int customerId,
        string? deliveryProvince,
        string? deliveryAddress,
        IEnumerable<OrderCartLine> cartLines)
    {
        var order = new Order
        {
            CustomerID = customerId,
            OrderTime = DateTime.Now,
            DeliveryProvince = deliveryProvince,
            DeliveryAddress = deliveryAddress,
            Status = (int)OrderStatusEnum.New
        };

        var orderId = await _orderDB.AddAsync(order);
        if (orderId == 0) return 0;

        foreach (var line in cartLines)
        {
            var detail = new OrderDetail
            {
                OrderID = orderId,
                ProductID = line.ProductID,
                Quantity = line.Quantity,
                SalePrice = line.SalePrice
            };
            await _orderDB.AddDetailAsync(detail);
        }

        return orderId;
    }

    /// <summary>
    /// Lấy thông tin chi tiết một đơn hàng của khách hàng.
    /// </summary>
    /// <param name="orderId">Mã đơn hàng.</param>
    /// <returns>Thông tin đơn hàng hoặc null.</returns>
    public static async Task<OrderViewInfo?> GetOrderAsync(int orderId)
    {
        return await _orderDB.GetAsync(orderId);
    }

    /// <summary>
    /// Lấy danh sách chi tiết các mặt hàng trong đơn hàng.
    /// </summary>
    /// <param name="orderId">Mã đơn hàng.</param>
    /// <returns>Danh sách chi tiết đơn hàng.</returns>
    public static async Task<IReadOnlyList<OrderDetailViewInfo>> GetOrderDetailsAsync(int orderId)
    {
        return await _orderDB.ListDetailsAsync(orderId);
    }

        /// <summary>Lấy danh sách đơn hàng của một khách hàng (lịch sử mua hàng).</summary>
        /// <param name="customerId">Mã khách hàng.</param>
        /// <param name="page">Trang cần hiển thị.</param>
        /// <param name="pageSize">Số dòng mỗi trang (0 = tất cả).</param>
        /// <returns>Kết quả phân trang đơn hàng.</returns>
        public static async Task<PagedResult<OrderViewInfo>> GetOrdersByCustomerAsync(int customerId, int page = 1, int pageSize = 10)
        {
            var input = new SV22T1020581.Models.Common.OrderSearchInput
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = "",
                CustomerID = customerId
            };

            return await OrderService.GetOrdersByCustomerAsync(input);
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của một khách hàng theo đầu vào tìm kiếm.
        /// </summary>
        /// <param name="input">Điều kiện tìm kiếm (chứa CustomerID).</param>
        /// <returns>Kết quả phân trang đơn hàng.</returns>
        internal static async Task<PagedResult<OrderViewInfo>> GetOrdersByCustomerAsync(SV22T1020581.Models.Common.OrderSearchInput input)
        {
            return await _orderDB.GetByCustomerAsync(input);
        }
}
