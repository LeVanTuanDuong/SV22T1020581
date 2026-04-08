using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.HR;

namespace SV22T1020581.BusinessLayers
{
    /// <summary>
    /// Cung c?p các ch?c nang x? lý d? li?u liên quan d?n nhân s? c?a h? th?ng    
    /// </summary>
    public static class HRDataService
    {
        private static readonly IEmployeeRepository employeeDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static HRDataService()
        {
            employeeDB = new EmployeeRepository(Configuration.ConnectionString);
        }

        #region Employee

        /// <summary>
        /// Tìm ki?m và l?y danh sách nhân viên du?i d?ng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm ki?m và phân trang (t? khóa tìm ki?m, trang c?n hi?n th?, s? dòng m?i trang).
        /// </param>
        /// <returns>
        /// K?t qu? tìm ki?m du?i d?ng danh sách nhân viên có phân trang.
        /// </returns>
        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            return await employeeDB.ListAsync(input);
        }

        /// <summary>
        /// L?y thông tin chi ti?t c?a m?t nhân viên d?a vào mã nhân viên.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên c?n tìm.</param>
        /// <returns>
        /// Ð?i tu?ng Employee n?u tìm th?y, ngu?c l?i tr? v? null.
        /// </returns>
        public static async Task<Employee?> GetEmployeeAsync(int employeeID)
        {
            return await employeeDB.GetAsync(employeeID);
        }

        /// <summary>
        /// B? sung m?t nhân viên m?i vào h? th?ng.
        /// </summary>
        /// <param name="data">Thông tin nhân viên c?n b? sung.</param>
        /// <returns>Mã nhân viên du?c t?o m?i.</returns>
        public static async Task<int> AddEmployeeAsync(Employee data)
        {
            //TODO: Ki?m tra d? li?u h?p l?
            return await employeeDB.AddAsync(data);
        }

        /// <summary>True n?u email ch?a t?n t?i trong b?ng Employees (dùng cho ??ng ký).</summary>
        public static async Task<bool> IsEmployeeEmailAvailableAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await employeeDB.ValidateEmailAsync(email.Trim(), 0);
        }

        /// <summary>
        /// C?p nh?t thông tin c?a m?t nhân viên.
        /// </summary>
        /// <param name="data">Thông tin nhân viên c?n c?p nh?t.</param>
        /// <returns>
        /// True n?u c?p nh?t thành công, ngu?c l?i False.
        /// </returns>
        public static async Task<bool> UpdateEmployeeAsync(Employee data)
        {
            //TODO: Ki?m tra d? li?u h?p l?
            return await employeeDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa m?t nhân viên d?a vào mã nhân viên.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên c?n xóa.</param>
        /// <returns>
        /// True n?u xóa thành công, False n?u nhân viên dang du?c s? d?ng
        /// ho?c vi?c xóa không th?c hi?n du?c.
        /// </returns>
        public static async Task<bool> DeleteEmployeeAsync(int employeeID)
        {
            if (await employeeDB.IsUsedAsync(employeeID))
                return false;

            return await employeeDB.DeleteAsync(employeeID);
        }

        /// <summary>
        /// Ki?m tra xem m?t nhân viên có dang du?c s? d?ng trong d? li?u hay không.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên c?n ki?m tra.</param>
        /// <returns>
        /// True n?u nhân viên dang du?c s? d?ng, ngu?c l?i False.
        /// </returns>
        public static async Task<bool> IsUsedEmployeeAsync(int employeeID)
        {
            return await employeeDB.IsUsedAsync(employeeID);
        }

        /// <summary>
        /// Ki?m tra xem email c?a nhân viên có h?p l? không
        /// (không b? trùng v?i email c?a nhân viên khác).
        /// </summary>
        /// <param name="email">Ð?a ch? email c?n ki?m tra.</param>
        /// <param name="employeeID">
        /// N?u employeeID = 0: ki?m tra email d?i v?i nhân viên m?i.
        /// N?u employeeID khác 0: ki?m tra email c?a nhân viên có mã là employeeID.
        /// </param>
        /// <returns>
        /// True n?u email h?p l? (không trùng), ngu?c l?i False.
        /// </returns>
        public static async Task<bool> ValidateEmployeeEmailAsync(string email, int employeeID = 0)
        {
            return await employeeDB.ValidateEmailAsync(email, employeeID);
        }

        #endregion
    }
}