using SV22T1020581.Models.Common;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các chức năng xử lý dữ liệu cho đơn hàng.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>Danh sách đơn hàng phân trang.</summary>
        Task<PagedResult<OrderViewInfo>> ListAsync(SV22T1020581.Models.Sales.OrderSearchInput input);

        /// <summary>Lấy thông tin một đơn hàng.</summary>
        Task<OrderViewInfo?> GetAsync(int orderID);

        /// <summary>Thêm đơn hàng mới.</summary>
        Task<int> AddAsync(Order data);

        /// <summary>Cập nhật đơn hàng.</summary>
        Task<bool> UpdateAsync(Order data);

        /// <summary>Xóa đơn hàng.</summary>
        Task<bool> DeleteAsync(int orderID);

        /// <summary>Cập nhật trạng thái đơn hàng.</summary>
        Task<bool> UpdateStatusAsync(int orderID, OrderStatusEnum status, DateTime? acceptTime, DateTime? finishedTime, int? employeeID);

        /// <summary>Danh sách mặt hàng trong đơn hàng.</summary>
        Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID);

        /// <summary>Lấy chi tiết một mặt hàng trong đơn hàng.</summary>
        Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID);

        /// <summary>Thêm mặt hàng vào đơn hàng.</summary>
        Task<bool> AddDetailAsync(OrderDetail data);

        /// <summary>Cập nhật số lượng và giá bán của mặt hàng.</summary>
        Task<bool> UpdateDetailAsync(int orderID, int productID, int quantity, decimal salePrice);

        /// <summary>Xóa mặt hàng khỏi đơn hàng.</summary>
        Task<bool> DeleteDetailAsync(int orderID, int productID);

        /// <summary>Danh sách đơn hàng theo khách hàng (dùng cho Shop).</summary>
        Task<PagedResult<OrderViewInfo>> GetByCustomerAsync(SV22T1020581.Models.Common.OrderSearchInput input);
    }
}
