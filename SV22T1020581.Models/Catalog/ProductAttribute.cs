namespace SV22T1020581.Models.Catalog
{
    /// <summary>
    /// Thu?c t�nh c?a m?t h�ng
    /// </summary>
    public class ProductAttribute
    {
        /// <summary>
        /// M� thu?c t�nh
        /// </summary>
        public long AttributeID { get; set; }
        /// <summary>
        /// M� m?t h�ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// T�n thu?c t�nh (v� d?: "M�u s?c", "K�ch thu?c", "Ch?t li?u", ...)
        /// </summary>
        public string AttributeName { get; set; } = string.Empty;
        /// <summary>
        /// Gi� tr? thu?c t�nh
        /// </summary>
        public string AttributeValue { get; set; } = string.Empty;
        /// <summary>
        /// Th? t? hi?n th? thu?c t�nh (gi� tr? nh? s? hi?n th? tru?c)
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
