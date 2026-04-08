namespace SV22T1020581.Models.Catalog
{
    /// <summary>
    /// Lo?i h�ng
    /// </summary>
    public class Category
    {
        /// <summary>
        /// M� lo?i h�ng
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// T�n lo?i h�ng
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// M� t? lo?i h�ng
        /// </summary>
        public string? Description { get; set; }
    }
}
