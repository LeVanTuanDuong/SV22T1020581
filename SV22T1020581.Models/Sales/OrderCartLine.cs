namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// DTO đại diện một dòng trong giỏ hàng (không có OrderID).
    /// Dùng khi tạo đơn hàng từ giỏ hàng.
    /// </summary>
    public class OrderCartLine
    {
        /// <summary>Mã mặt hàng.</summary>
        public int ProductID { get; set; }
        /// <summary>T�n m?t h�ng.</summary>
        public string ProductName { get; set; } = "";
        /// <summary>??n v? t�nh.</summary>
        public string Unit { get; set; } = "";
        /// <summary>S? l??ng mua.</summary>
        public int Quantity { get; set; }
        /// <summary>Gi� b�n t?i th?i ?i?m ??t.</summary>
        public decimal SalePrice { get; set; }
        /// <summary>T?ng th�nh ti?n.</summary>
        public decimal TotalPrice => Quantity * SalePrice;
    }
}
