namespace SV22T1020581.Models.Security
{
    /// <summary>
    /// Th�ng tin t�i kho?n ngu?i d�ng
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// M� t�i kho?n
        /// </summary>
        public string UserId { get; set; } = "";
        /// <summary>
        /// T�n dang nh?p
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// T�n hi?n th? (thu?ng l� h? t�n c?a ngu?i d�ng, ho?c c� th? l� t�n dang nh?p n?u kh�ng c� h? t�n)
        /// </summary>
        public string DisplayName { get; set; } = "";
        /// <summary>
        /// �?a ch? email (n?u c�)
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// T�n fie ?nh d?i di?n c?a ngu?i d�ng (n?u c�)
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Danh s�ch t�n c�c vai tr�/quy?n c?a ngu?i d�ng, du?c ph�n c�ch b?i d?u ch?m ph?y (n?u c�)
        /// </summary>
        public string RoleNames { get; set; } = "";
    }
}
