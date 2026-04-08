using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Partner;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn bảng Suppliers bằng Dapper.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            var where = @"(@Search = '' OR SupplierName LIKE @Like OR ContactName LIKE @Like OR ISNULL(Email,'') LIKE @Like OR ISNULL(Phone,'') LIKE @Like OR ISNULL(Address,'') LIKE @Like)";
            var countSql = $"SELECT COUNT(*) FROM Suppliers WHERE {where}";
            var rowCount = await conn.ExecuteScalarAsync<int>(countSql, new { Search = input.SearchValue ?? "", Like = like });

            var result = new PagedResult<Supplier> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Supplier>($@"
                    SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email FROM Suppliers WHERE {where}
                    ORDER BY SupplierName", new { Search = input.SearchValue ?? "", Like = like });
                result.DataItems = all.ToList();
                return result;
            }

            var data = await conn.QueryAsync<Supplier>($@"
                SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email FROM Suppliers WHERE {where}
                ORDER BY SupplierName
                OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Supplier?> GetAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Supplier>(@"
                SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email FROM Suppliers WHERE SupplierID = @id", new { id });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Supplier data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Suppliers (SupplierName, ContactName, Province, Address, Phone, Email)
                VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Supplier data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Suppliers SET SupplierName=@SupplierName, ContactName=@ContactName, Province=@Province,
                    Address=@Address, Phone=@Phone, Email=@Email WHERE SupplierID=@SupplierID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync("DELETE FROM Suppliers WHERE SupplierID=@id", new { id });
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM Products WHERE SupplierID=@id) THEN 1 ELSE 0 END", new { id }) == 1;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateEmailAsync(string email, int supplierID)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Suppliers
                WHERE Email = @email AND (@supplierID = 0 OR SupplierID <> @supplierID)",
                new { email = email.Trim(), supplierID });
            return n == 0;
        }
    }
}
