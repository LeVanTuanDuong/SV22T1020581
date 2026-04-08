namespace SV22T1020581.Models.Partner
{
    /// <summary>
    /// Nh� cung c?p
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// M� nh� cung c?p
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// T�n nh� cung c?p
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// T�n giao d?ch
        /// </summary>
        public string ContactName { get; set; } = string.Empty;
        /// <summary>
        /// T?nh th�nh
        /// </summary>
        public string? Province { get; set; }
        /// <summary>
        /// �?a ch?
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// �i?n tho?i
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }
    }
}
