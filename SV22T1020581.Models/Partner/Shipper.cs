namespace SV22T1020581.Models.Partner
{
    /// <summary>
    /// Ngu?i giao h�ng
    /// </summary>
    public class Shipper
    {
        /// <summary>
        /// M� ngu?i giao h�ng
        /// </summary>
        public int ShipperID { get; set; }
        /// <summary>
        /// T�n ngu?i giao h�ng
        /// </summary>
        public string ShipperName { get; set; } = string.Empty;
        /// <summary>
        /// �i?n tho?i
        /// </summary>
        public string? Phone { get; set; }
    }
}
