namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// Th�ng tin chi ti?t c?a m?t h�ng du?c b�n trong don h�ng
    /// </summary>
    public class OrderDetail
    {
        /// <summary>
        /// M� don h�ng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// M� m?t h�ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// S? lu?ng
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Gi� b�n
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// T?ng s? ti?n
        /// </summary>
        public decimal TotalPrice => Quantity * SalePrice;        
    }
}
