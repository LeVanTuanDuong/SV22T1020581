namespace SV22T1020581.Models.Catalog
{
    /// <summary>
    /// ?nh c?a m?t h�ng
    /// </summary>
    public class ProductPhoto
    {
        /// <summary>
        /// M� ?nh
        /// </summary>
        public long PhotoID { get; set; }
        /// <summary>
        /// M� m?t h�ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// T�n file ?nh
        /// </summary>
        public string Photo { get; set; } = string.Empty;
        /// <summary>
        /// M� t? ?nh
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Th? t? hi?n th? (gi� tr? nh? s? hi?n th? tru?c)
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// C� ?n ?nh d?i v?i kh�ch h�ng hay kh�ng?
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
