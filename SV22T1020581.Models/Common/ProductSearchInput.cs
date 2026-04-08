namespace SV22T1020581.Models.Common
{
    /// <summary>
    /// Đầu vào tìm kiếm mặt hàng (kèm phân trang).
    /// </summary>
    public class ProductSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Lọc theo mã loại hàng (null = không lọc).
        /// </summary>
        public int? CategoryID { get; set; }
        /// <summary>
        /// Lọc theo mã nhà cung cấp (null = không lọc).
        /// </summary>
        public int? SupplierID { get; set; }

        /// <summary>Giá tối thiểu (null = không lọc).</summary>
        public decimal? MinPrice { get; set; }

        /// <summary>Giá tối đa (null = không lọc).</summary>
        public decimal? MaxPrice { get; set; }
    }
}
