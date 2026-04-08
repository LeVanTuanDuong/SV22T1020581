using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Common;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn bảng Categories.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
        public CategoryRepository(string connectionString) => _connectionString = connectionString;

        /// <inheritdoc />
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            const string where = @"(@Search = '' OR CategoryName LIKE @Like OR ISNULL(Description,'') LIKE @Like)";
            var rowCount = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Categories WHERE {where}", new { Search = input.SearchValue ?? "", Like = like });
            var result = new PagedResult<Category> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Category>($"SELECT * FROM Categories WHERE {where} ORDER BY CategoryName", new { Search = input.SearchValue ?? "", Like = like });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<Category>($@"
                SELECT * FROM Categories WHERE {where} ORDER BY CategoryName OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Category?> GetAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Category>("SELECT * FROM Categories WHERE CategoryID=@id", new { id });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Category data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Categories (CategoryName, Description) VALUES (@CategoryName, @Description);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Category data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync("UPDATE Categories SET CategoryName=@CategoryName, Description=@Description WHERE CategoryID=@CategoryID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM Categories WHERE CategoryID=@id", new { id }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM Products WHERE CategoryID=@id) THEN 1 ELSE 0 END", new { id }) == 1;
        }
    }
}
