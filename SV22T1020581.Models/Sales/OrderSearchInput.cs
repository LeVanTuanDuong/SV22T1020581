using SV22T1020581.Models.Common;

namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// Ð?u vào tìm ki?m, phân trang don hàng
    /// </summary>
    public class OrderSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Tr?ng thái don hàng
        /// </summary>
        public OrderStatusEnum Status { get; set; }
        /// <summary>
        /// T? ngày (ngày l?p don hàng)
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// Ð?n ngày (ngày l?p don hàng)
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
