namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// Mở rộng các phương thức cho enum OrderStatusEnum
    /// </summary>
    public static class OrderStatusExtensions
    {
        /// <summary>
        /// Lấy chuỗi mô tả cho từng trạng thái của đơn hàng
        /// </summary>
        public static string GetDescription(this OrderStatusEnum status)
        {
            return status switch
            {
                OrderStatusEnum.Rejected => "Bị từ chối",
                OrderStatusEnum.Cancelled => "Bị hủy",
                OrderStatusEnum.New => "Chờ duyệt",
                OrderStatusEnum.Accepted => "Đã duyệt",
                OrderStatusEnum.Shipping => "Đang giao",
                OrderStatusEnum.Completed => "Đã hoàn tất",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Nhãn hiển thị theo mã trạng thái trong CSDL (int).
        /// </summary>
        public static string GetDisplayLabel(int status)
        {
            if (Enum.IsDefined(typeof(OrderStatusEnum), status))
                return GetDescription((OrderStatusEnum)status);
            return "Không xác định";
        }
    }
}
