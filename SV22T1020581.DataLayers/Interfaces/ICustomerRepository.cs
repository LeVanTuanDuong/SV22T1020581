using SV22T1020581.Models.Partner;

namespace SV22T1020581.DataLayers.Interfaces
{
    /// <summary>
    /// �?nh nghia c�c ph�p x? l� d? li?u tr�n Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Ki?m tra xem m?t d?a ch? email c� h?p l? hay kh�ng?
        /// </summary>
        /// <param name="email">Email c?n ki?m tra</param>
        /// <param name="id">
        /// N?u id = 0: Ki?m tra email c?a kh�ch h�ng m?i.
        /// N?u id <> 0: Ki?m tra email d?i v?i kh�ch h�ng d� t?n t?i
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
    }
}
