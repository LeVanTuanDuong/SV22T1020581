using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.HR;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn bảng Employees.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
        public EmployeeRepository(string connectionString) => _connectionString = connectionString;

        /// <inheritdoc />
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            const string where = @"(@Search = '' OR FullName LIKE @Like OR Email LIKE @Like OR ISNULL(Phone,'') LIKE @Like OR ISNULL(Address,'') LIKE @Like)";
            var rowCount = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Employees WHERE {where}", new { Search = input.SearchValue ?? "", Like = like });
            var result = new PagedResult<Employee> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            const string cols = """
                EmployeeID,
                CAST(FullName AS NVARCHAR(200)) AS FullName,
                BirthDate,
                CAST(Address AS NVARCHAR(500)) AS Address,
                CAST(Phone AS NVARCHAR(50)) AS Phone,
                CAST(Email AS NVARCHAR(256)) AS Email,
                CAST(Photo AS NVARCHAR(255)) AS Photo,
                IsWorking,
                CAST(RoleNames AS NVARCHAR(500)) AS RoleNames
                """;
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Employee>($@"
                    SELECT {cols}, CAST(NULL AS nvarchar(50)) AS Password FROM Employees WHERE {where} ORDER BY FullName",
                    new { Search = input.SearchValue ?? "", Like = like });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<Employee>($@"
                SELECT {cols}, CAST(NULL AS nvarchar(50)) AS Password FROM Employees WHERE {where} ORDER BY FullName
                OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Employee?> GetAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Employee>(@"
                SELECT EmployeeID,
                    CAST(FullName AS NVARCHAR(200)) AS FullName,
                    BirthDate,
                    CAST(Address AS NVARCHAR(500)) AS Address,
                    CAST(Phone AS NVARCHAR(50)) AS Phone,
                    CAST(Email AS NVARCHAR(256)) AS Email,
                    Password,
                    CAST(Photo AS NVARCHAR(255)) AS Photo,
                    IsWorking,
                    CAST(RoleNames AS NVARCHAR(500)) AS RoleNames
                FROM Employees WHERE EmployeeID=@id", new { id });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Employee data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Employees (FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
                VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Employee data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Employees SET FullName=@FullName, BirthDate=@BirthDate, Address=@Address, Phone=@Phone,
                    Email=@Email, Password=@Password, Photo=@Photo, IsWorking=@IsWorking, RoleNames=@RoleNames
                WHERE EmployeeID=@EmployeeID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM Employees WHERE EmployeeID=@id", new { id }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE EmployeeID=@id) THEN 1 ELSE 0 END", new { id }) == 1;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Employees WHERE Email = @email AND (@id = 0 OR EmployeeID <> @id)",
                new { email = email.Trim(), id });
            return n == 0;
        }
    }
}
