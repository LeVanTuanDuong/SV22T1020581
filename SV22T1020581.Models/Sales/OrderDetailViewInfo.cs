namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// DTO hien thi thong tin chi tiet cua mat hang trong don hang.
    /// </summary>
    public class OrderDetailViewInfo : OrderDetail
    {
        /// <summary>Ten mat hang.</summary>
        public string ProductName { get; set; } = "";
        /// <summary>Don vi tinh.</summary>
        public string Unit { get; set; } = "";
        /// <summary>Ten file anh.</summary>
        public string Photo { get; set; } = "";
        /// <summary>Tong thanh tien (dung boi View).</summary>
        public decimal TotalAmount => Quantity * SalePrice;
    }
}
