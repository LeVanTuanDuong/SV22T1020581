using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Partner;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn bảng Shippers.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
        public ShipperRepository(string connectionString) => _connectionString = connectionString;

        /// <inheritdoc />
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            const string where = @"(@Search = '' OR ShipperName LIKE @Like OR ISNULL(Phone,'') LIKE @Like)";
            var rowCount = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Shippers WHERE {where}", new { Search = input.SearchValue ?? "", Like = like });
            var result = new PagedResult<Shipper> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Shipper>($"SELECT * FROM Shippers WHERE {where} ORDER BY ShipperName", new { Search = input.SearchValue ?? "", Like = like });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<Shipper>($@"
                SELECT * FROM Shippers WHERE {where} ORDER BY ShipperName OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Shipper?> GetAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Shipper>("SELECT * FROM Shippers WHERE ShipperID=@id", new { id });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Shipper data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Shippers (ShipperName, Phone) VALUES (@ShipperName, @Phone);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Shipper data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync("UPDATE Shippers SET ShipperName=@ShipperName, Phone=@Phone WHERE ShipperID=@ShipperID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM Shippers WHERE ShipperID=@id", new { id }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE ShipperID=@id) THEN 1 ELSE 0 END", new { id }) == 1;
        }
    }
}
