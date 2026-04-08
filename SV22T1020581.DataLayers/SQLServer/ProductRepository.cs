using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Common;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy vấn Products, ProductAttributes, ProductPhotos.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với chuỗi kết nối SQL Server.</summary>
        public ProductRepository(string connectionString) => _connectionString = connectionString;

        private const string ProductColumns = @"
            ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo,
            CAST(ISNULL(IsSelling, 0) AS bit) AS IsSelling";

        /// <inheritdoc />
        public async Task<PagedResult<Product>> ListAsync(Models.Catalog.ProductSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            // ProductSearchInput dùng 0 = "bỏ qua" bộ lọc; SQL trước đây coi 0 là giá trị thật nên không có dòng nào khớp.
            int? categoryId = input.CategoryID == 0 ? null : input.CategoryID;
            int? supplierId = input.SupplierID == 0 ? null : input.SupplierID;
            decimal? minPrice = input.MinPrice == 0 ? null : input.MinPrice;
            decimal? maxPrice = input.MaxPrice == 0 ? null : input.MaxPrice;

            var where = @"(@Search = '' OR ProductName LIKE @Like OR ISNULL(ProductDescription,'') LIKE @Like)
                AND (@CategoryID IS NULL OR CategoryID = @CategoryID)
                AND (@SupplierID IS NULL OR SupplierID = @SupplierID)
                AND (@MinPrice IS NULL OR Price >= @MinPrice)
                AND (@MaxPrice IS NULL OR Price <= @MaxPrice)";
            var rowCount = await conn.ExecuteScalarAsync<int>($@"
                SELECT COUNT(*) FROM Products WHERE {where}",
                new { Search = input.SearchValue ?? "", Like = like, CategoryID = categoryId, SupplierID = supplierId, MinPrice = minPrice, MaxPrice = maxPrice });
            var result = new PagedResult<Product> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<Product>($@"
                    SELECT {ProductColumns} FROM Products WHERE {where} ORDER BY ProductName",
                    new { Search = input.SearchValue ?? "", Like = like, CategoryID = categoryId, SupplierID = supplierId, MinPrice = minPrice, MaxPrice = maxPrice });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<Product>($@"
                SELECT {ProductColumns} FROM Products WHERE {where} ORDER BY ProductName
                OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Search = input.SearchValue ?? "", Like = like, CategoryID = categoryId, SupplierID = supplierId, MinPrice = minPrice, MaxPrice = maxPrice, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<Product?> GetAsync(int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Product>($@"
                SELECT {ProductColumns} FROM Products WHERE ProductID=@productId", new { productId });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Product data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                SELECT CAST(SCOPE_IDENTITY() AS int);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Product data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Products SET ProductName=@ProductName, ProductDescription=@ProductDescription,
                    SupplierID=@SupplierID, CategoryID=@CategoryID, Unit=@Unit, Price=@Price, Photo=@Photo, IsSelling=@IsSelling
                WHERE ProductID=@ProductID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM ProductAttributes WHERE ProductID=@productId", new { productId });
            await conn.ExecuteAsync("DELETE FROM ProductPhotos WHERE ProductID=@productId", new { productId });
            return await conn.ExecuteAsync("DELETE FROM Products WHERE ProductID=@productId", new { productId }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsUsedAsync(int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            var sql = @"
                SELECT CASE WHEN EXISTS(SELECT 1 FROM OrderDetails WHERE ProductID=@id)
                    OR EXISTS(SELECT 1 FROM ProductAttributes WHERE ProductID=@id)
                    OR EXISTS(SELECT 1 FROM ProductPhotos WHERE ProductID=@id)
                    THEN 1 ELSE 0 END";
            return await conn.ExecuteScalarAsync<int>(sql, new { id = productId }) == 1;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductAttribute>> ListAttributesAsync(int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<ProductAttribute>(@"
                SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
                FROM ProductAttributes WHERE ProductID=@productId ORDER BY DisplayOrder, AttributeID",
                new { productId });
            return rows.ToList();
        }

        /// <inheritdoc />
        public async Task<ProductAttribute?> GetAttributeAsync(int productId, long attributeId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<ProductAttribute>(@"
                SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
                FROM ProductAttributes WHERE ProductID=@productId AND AttributeID=@attributeId",
                new { productId, attributeId });
        }

        /// <inheritdoc />
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<long>(@"
                INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
                VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE ProductAttributes SET AttributeName=@AttributeName, AttributeValue=@AttributeValue, DisplayOrder=@DisplayOrder
                WHERE AttributeID=@AttributeID AND ProductID=@ProductID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAttributeAsync(long attributeId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM ProductAttributes WHERE AttributeID=@attributeId", new { attributeId }) > 0;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductPhoto>> ListPhotosAsync(int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<ProductPhoto>(@"
                SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden
                FROM ProductPhotos WHERE ProductID=@productId ORDER BY DisplayOrder, PhotoID", new { productId });
            return rows.ToList();
        }

        /// <inheritdoc />
        public async Task<ProductPhoto?> GetPhotoAsync(int productId, long photoId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<ProductPhoto>(@"
                SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden
                FROM ProductPhotos WHERE ProductID=@productId AND PhotoID=@photoId", new { productId, photoId });
        }

        /// <inheritdoc />
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<long>(@"
                INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
                VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);", data);
        }

        /// <inheritdoc />
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE ProductPhotos SET Photo=@Photo, Description=@Description, DisplayOrder=@DisplayOrder, IsHidden=@IsHidden
                WHERE PhotoID=@PhotoID AND ProductID=@ProductID", data);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeletePhotoAsync(long photoId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync("DELETE FROM ProductPhotos WHERE PhotoID=@photoId", new { photoId }) > 0;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int topPerCategory = 1)
        {
            await using var conn = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT * FROM (
                    SELECT {ProductColumns},
                           ROW_NUMBER() OVER (PARTITION BY CategoryID ORDER BY Price DESC) AS rn
                    FROM Products
                    WHERE IsSelling = 1 AND CategoryID IS NOT NULL
                ) t
                WHERE rn <= @Top
                ORDER BY Price DESC";
            var rows = await conn.QueryAsync<Product>(sql, new { Top = topPerCategory });
            return rows.ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int categoryID, int currentProductID, int take = 4)
        {
            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<Product>($@"
                SELECT TOP(@Take) {ProductColumns}
                FROM Products
                WHERE CategoryID = @CategoryID AND ProductID <> @CurrentProductID AND IsSelling = 1
                ORDER BY NEWID()",
                new { CategoryID = categoryID, CurrentProductID = currentProductID, Take = take });
            return rows.ToList();
        }
    }
}
