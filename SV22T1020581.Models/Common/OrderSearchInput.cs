namespace SV22T1020581.Models.Common
{
    /// <summary>
    /// Đầu vào tìm kiếm đơn hàng (kèm phân trang).
    /// </summary>
    public class OrderSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Lọc theo trạng thái đơn hàng (null = không lọc).
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Lọc theo ngày lập đơn — từ ngày (định dạng dd/MM/yyyy hoặc d/M/yyyy, từ flatpickr).
        /// </summary>
        public string? DateFrom { get; set; }

        /// <summary>
        /// Lọc theo ngày lập đơn — đến ngày (cùng định dạng).
        /// </summary>
        public string? DateTo { get; set; }

        /// <summary>
        /// Lọc theo mã khách hàng (chỉ dùng cho Shop, Admin để null).
        /// </summary>
        public int? CustomerID { get; set; }
    }
}
