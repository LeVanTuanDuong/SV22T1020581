using SV22T1020581.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020581.DataLayers.Interfaces
{
    /// <summary>
    /// �?nh nghia c�c ph�p x? l� d? li?u tr�n Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Ki?m tra xem email c?a nh�n vi�n c� h?p l? kh�ng
        /// </summary>
        /// <param name="email">Email c?n ki?m tra</param>
        /// <param name="id">
        /// N?u id = 0: Ki?m tra email c?a nh�n vi�n m?i
        /// N?u id <> 0: Ki?m tra email c?a nh�n vi�n c� m� l� id
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
    }
}
