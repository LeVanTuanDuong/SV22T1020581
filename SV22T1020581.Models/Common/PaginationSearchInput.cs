namespace SV22T1020581.Models.Common
{
    /// <summary>
    /// L?p d�ng d? bi?u di?n th�ng tin d?u v�o c?a m?t truy v?n/t�m ki?m 
    /// d? li?u don gi?n du?i d?ng ph�n trang
    /// </summary>
    public class PaginationSearchInput
    {
        private const int MaxPageSize = 100; //Gi?i h?n t?i da 100 d�ng m?i trang
        private int _page = 1;
        private int _pageSize = 20;
        private string _searchValue = "";
        
        /// <summary>
        /// Trang c?n du?c hi?n th? (b?t d?u t? 1)
        /// </summary>
        public int Page 
        { 
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }
        /// <summary>
        /// S? d�ng du?c hi?n th? tr�n m?i trang
        /// (0 c� nghia l� hi?n th? t?t c? c�c d�ng tr�n m?t trang, t?c l� kh�ng ph�n trang)
        /// </summary>
        public int PageSize 
        { 
            get => _pageSize; 
            set
            {
                if (value < 0)
                    _pageSize = 0;
                else if (value > MaxPageSize)
                    _pageSize = MaxPageSize;
                else
                    _pageSize = value;
            }
        }
        /// <summary>
        /// Gi� tr? t�m ki?m (n?u c�) du?c s? d?ng d? l?c d? li?u 
        /// (N?u kh�ng c� gi� tr? t�m ki?m, th� d? r?ng)
        /// </summary>
        public string SearchValue
        { 
            get => _searchValue; 
            set => _searchValue = value?.Trim() ?? ""; 
        }        
        /// <summary>
        /// S? d�ng c?n b? qua (t�nh t? d�ng d?u ti�n c?a t?p d? li?u) 
        /// d? l?y d? li?u cho trang hi?n t?i
        /// </summary>
        public int Offset => PageSize > 0 ? (Page - 1) * PageSize : 0;
    }
}
