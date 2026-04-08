using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Security;

namespace SV22T1020581.BusinessLayers
{
    /// <summary>Lo?i t�i kho?n d�ng cho x�c th?c / ??i m?t kh?u.</summary>
    public enum AccountTypes
    {
        /// <summary>Nh�n vi�n qu?n tr? / n?i b?.</summary>
        Employee,
        /// <summary>Kh�ch h�ng.</summary>
        Customer
    }

    /// <summary>D?ch v? t�i kho?n: ??ng nh?p, ??i m?t kh?u (wrap repository + c?u h�nh k?t n?i).</summary>
    public static class UserAccountService
    {
        private static IUserAccountRepository Repo => new EmployeeAccountRepository(Configuration.ConnectionString);

        /// <summary>X�c th?c theo lo?i t�i kho?n (email + m?t kh?u).</summary>
        public static Task<UserAccount?> Authorize(AccountTypes type, string username, string password)
        {
            return type switch
            {
                AccountTypes.Employee => Repo.AuthenticateEmployeeAsync(username, password),
                AccountTypes.Customer => Repo.AuthenticateCustomerAsync(username, password),
                _ => Task.FromResult<UserAccount?>(null)
            };
        }

        /// <summary>??t m?t kh?u m?i sau khi ?� ki?m tra m?t kh?u c? ? controller.</summary>
        public static async Task ChangePassword(AccountTypes type, string userName, string newPassword)
        {
            if (type == AccountTypes.Employee)
            {
                var ok = await Repo.SetEmployeePasswordByEmailAsync(userName, newPassword);
                if (!ok)
                    throw new InvalidOperationException("Kh�ng th? c?p nh?t m?t kh?u nh�n vi�n.");
            }
        }

        /// <summary>Qu?n tr? ??t l?i m?t kh?u nh�n vi�n theo m�.</summary>
        public static Task<bool> SetEmployeePassword(int employeeId, string newPassword) =>
            Repo.SetEmployeePasswordByIdAsync(employeeId, newPassword);
    }
}
