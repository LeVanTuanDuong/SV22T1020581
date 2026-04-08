namespace SV22T1020581.Models.Catalog
{
    /// <summary>
    /// M?t h�ng
    /// </summary>
    public class Product
    {
        /// <summary>
        /// M� m?t h�ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// T�n m?t h�ng
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>
        /// M� t? m?t h�ng
        /// </summary>
        public string? ProductDescription { get; set; }
        /// <summary>
        /// M� nh� cung c?p
        /// </summary>
        public int? SupplierID { get; set; }
        /// <summary>
        /// M� lo?i h�ng
        /// </summary>
        public int? CategoryID { get; set; }
        /// <summary>
        /// �on vi t�nh
        /// </summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>
        /// Gi�
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// T�n file ?nh d?i di?n c?a m?t h�ng (n?u c�)
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// M?t h�ng hi?n c� dang du?c b�n hay kh�ng?
        /// </summary>
        public bool IsSelling { get; set; }
    }
}
