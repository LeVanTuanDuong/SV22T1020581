namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// �?nh nghia c�c tr?ng th�i c?a don h�ng
    /// </summary>
    public enum OrderStatusEnum
    {
        /// <summary>
        /// �on h�ng b? t? ch?i
        /// </summary>
        Rejected = -2,
        /// <summary>
        /// �on h�ng b? h?y
        /// </summary>
        Cancelled = -1,
        /// <summary>
        /// �on h�ng v?a du?c t?o, chua du?c x? l�
        /// </summary>
        New = 1,
        /// <summary>
        /// �on h�ng d� du?c duy?t ch?p nh?n
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// �on h�ng dang du?c giao cho ngu?i giao h�ng d? v?n chuy?n d?n kh�ch h�ng
        /// </summary>
        Shipping = 3,
        /// <summary>
        /// �on h�ng d� ho�n t?t (th�nh c�ng)
        /// </summary>
        Completed = 4
    }
}
