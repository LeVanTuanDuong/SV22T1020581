namespace SV22T1020581.Models.Common
{
    /// <summary>
    /// Ph?n t? tr�n thanh ph�n trang, c� th? l� m?t s? trang ho?c d?u "..." d? ph�n c�ch c�c nh�m trang
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pageNumber">0 n?u l� ph?n t? d�ng d? th? hi?n d?u "..." ph�n c�ch</param>
        /// <param name="isCurrent"></param>
        public PageItem(int pageNumber, bool isCurrent = false)
        {
            Page = pageNumber;
            IsCurrent = isCurrent;
        }
        /// <summary>
        /// S? trang (c� gi� tr? l� 0 n?u l� d?u "..." d? ph�n c�ch c�c nh�m trang)
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// C� ph?i l� trang hi?n t?i hay kh�ng?
        /// </summary>
        public bool IsCurrent { get; set; }
        /// <summary>
        /// C� ph?i l� v? tr� hi?n th? d?u "..." d? ph�n c�ch c�c nh�m trang hay kh�ng?
        /// </summary>
        public bool IsEllipsis => Page == 0;
    }
}
