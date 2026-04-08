using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Partner;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn bảng Customers.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
        public CustomerRepository(string connectionString) => _connectionString = connectionString;

        /// <inheritdoc />
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            const string where = @"(@Search = '' OR CustomerName LIKE @Like OR ContactName LIKE @Like OR Email LIKE @Like OR ISNULL(Phone,'') LIKE @Like OR ISNULL(Address,'') LIKE @Like)";
            var rowCount = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Customers WHERE {where}", new { Search = input.SearchValue ?? "", Like = like });
            var result = new PagedResult<Customer> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            const string cols = "CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked";
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Customer>($@"
                    SELECT {cols}, CAST(NULL AS nvarchar(50)) AS Password FROM Customers WHERE {where} ORDER BY CustomerName",
                    new { Search = input.SearchValue ?? "", Like = like });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<Customer>($@"
                SELECT {cols}, CAST(NULL AS nvarchar(50)) AS Password FROM Customers WHERE {where} ORDER BY CustomerName
                OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Customer?> GetAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Customer>(@"
                SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked FROM Customers WHERE CustomerID=@id", new { id });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Customer data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Customer data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Customers SET CustomerName=@CustomerName, ContactName=@ContactName, Province=@Province,
                    Address=@Address, Phone=@Phone, Email=@Email, Password=@Password, IsLocked=@IsLocked
                WHERE CustomerID=@CustomerID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM Customers WHERE CustomerID=@id", new { id }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE CustomerID=@id) THEN 1 ELSE 0 END", new { id }) == 1;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Customers WHERE Email = @email AND (@id = 0 OR CustomerID <> @id)",
                new { email = email.Trim(), id });
            return n == 0;
        }
    }
}
