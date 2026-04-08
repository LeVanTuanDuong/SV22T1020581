using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.DataDictionary;
using SV22T1020581.Models.Partner;

namespace SV22T1020581.BusinessLayers;

/// <summary>
/// Cung cấp các tính năng xử lý dữ liệu nghiệp vụ liên quan đến danh mục sản phẩm.
/// Bao gồm: Product (Mặt hàng), Category (Loại hàng), Supplier (Nhà cung cấp), Province (Tỉnh/thành).
/// </summary>
public static class CatalogDataService
{
    private static readonly IProductRepository _productDB;
    private static readonly IGenericRepository<Category> _categoryDB;
    private static readonly IGenericRepository<Supplier> _supplierDB;
    private static readonly IDataDictionaryRepository<Province> _provinceDB;

    static CatalogDataService()
    {
        var conn = Configuration.ConnectionString;
        _productDB = new ProductRepository(conn);
        _categoryDB = new CategoryRepository(conn);
        _supplierDB = new SupplierRepository(conn);
        _provinceDB = new ProvinceRepository(conn);
    }

    // ── Products ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Tìm kiếm và phân trang danh sách mặt hàng.
    /// </summary>
    /// <param name="input">Điều kiện tìm kiếm và phân trang.</param>
    /// <returns>Kết quả phân trang.</returns>
    public static async Task<PagedResult<Product>> ListProductsAsync(Models.Catalog.ProductSearchInput input)
    {
        return await _productDB.ListAsync(input);
    }

    /// <summary>
    /// Lấy thông tin một mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng.</param>
    /// <returns>Mặt hàng hoặc null nếu không tìm thấy.</returns>
    public static async Task<Product?> GetProductAsync(int productId)
    {
        return await _productDB.GetAsync(productId);
    }

    /// <summary>Lấy sản phẩm nổi bật (tối đa N sản phẩm giá cao nhất mỗi danh mục).</summary>
    public static async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int topPerCategory = 1)
    {
        return await _productDB.GetFeaturedProductsAsync(topPerCategory);
    }

    /// <summary>Lấy sản phẩm cùng danh mục (trừ sản phẩm hiện tại).</summary>
    public static async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int categoryID, int currentProductID, int take = 4)
    {
        return await _productDB.GetRelatedProductsAsync(categoryID, currentProductID, take);
    }

    /// <summary>Lấy danh sách ảnh của một mặt hàng.</summary>
    public static async Task<IReadOnlyList<ProductPhoto>> ListProductPhotosAsync(int productId)
    {
        return await _productDB.ListPhotosAsync(productId);
    }

    /// <summary>Lấy danh sách thuộc tính của một mặt hàng.</summary>
    public static async Task<IReadOnlyList<ProductAttribute>> ListProductAttributesAsync(int productId)
    {
        return await _productDB.ListAttributesAsync(productId);
    }

    // ── Categories ────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy toàn bộ danh sách loại hàng.
    /// </summary>
    /// <returns>Danh sách loại hàng.</returns>
    public static async Task<IReadOnlyList<Category>> ListCategoriesAsync()
    {
        var result = await _categoryDB.ListAsync(new PaginationSearchInput { PageSize = 0 });
        return result.DataItems;
    }

    /// <summary>
    /// Tìm kiếm và phân trang danh sách loại hàng.
    /// </summary>
    /// <param name="input">Điều kiện tìm kiếm và phân trang.</param>
    /// <returns>Kết quả phân trang.</returns>
    public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
    {
        return await _categoryDB.ListAsync(input);
    }

    /// <summary>
    /// Lấy thông tin một loại hàng.
    /// </summary>
    /// <param name="categoryId">Mã loại hàng.</param>
    /// <returns>Loại hàng hoặc null.</returns>
    public static async Task<Category?> GetCategoryAsync(int categoryId)
    {
        return await _categoryDB.GetAsync(categoryId);
    }

    /// <summary>
    /// Thêm một loại hàng mới.
    /// </summary>
    /// <param name="data">Thông tin loại hàng cần thêm.</param>
    /// <returns>Mã loại hàng được tạo mới.</returns>
    public static async Task<int> AddCategoryAsync(Category data)
    {
        return await _categoryDB.AddAsync(data);
    }

    /// <summary>
    /// Cập nhật thông tin loại hàng.
    /// </summary>
    /// <param name="data">Thông tin loại hàng cần cập nhật.</param>
    /// <returns>True nếu cập nhật thành công.</returns>
    public static async Task<bool> UpdateCategoryAsync(Category data)
    {
        return await _categoryDB.UpdateAsync(data);
    }

    /// <summary>
    /// Xóa một loại hàng.
    /// </summary>
    /// <param name="categoryId">Mã loại hàng cần xóa.</param>
    /// <returns>True nếu xóa thành công.</returns>
    public static async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        return await _categoryDB.DeleteAsync(categoryId);
    }

    /// <summary>
    /// Kiểm tra xem một loại hàng có đang được sử dụng hay không.
    /// </summary>
    /// <param name="categoryId">Mã loại hàng cần kiểm tra.</param>
    /// <returns>True nếu đang được sử dụng.</returns>
    public static async Task<bool> IsUsedCategoryAsync(int categoryId)
    {
        return await _categoryDB.IsUsedAsync(categoryId);
    }

    /// <summary>
    /// Thêm một mặt hàng mới.
    /// </summary>
    /// <param name="data">Thông tin mặt hàng cần thêm.</param>
    /// <returns>Mã mặt hàng được tạo mới.</returns>
    public static async Task<int> AddProductAsync(Product data)
    {
        return await _productDB.AddAsync(data);
    }

    /// <summary>
    /// Cập nhật thông tin mặt hàng.
    /// </summary>
    /// <param name="data">Thông tin mặt hàng cần cập nhật.</param>
    /// <returns>True nếu cập nhật thành công.</returns>
    public static async Task<bool> UpdateProductAsync(Product data)
    {
        return await _productDB.UpdateAsync(data);
    }

    /// <summary>
    /// Xóa một mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng cần xóa.</param>
    /// <returns>True nếu xóa thành công.</returns>
    public static async Task<bool> DeleteProductAsync(int productId)
    {
        return await _productDB.DeleteAsync(productId);
    }

    /// <summary>
    /// Kiểm tra xem một mặt hàng có đang được sử dụng hay không.
    /// </summary>
    /// <param name="productId">Mã mặt hàng cần kiểm tra.</param>
    /// <returns>True nếu đang được sử dụng.</returns>
    public static async Task<bool> IsUsedProductAsync(int productId)
    {
        return await _productDB.IsUsedAsync(productId);
    }

    /// <summary>
    /// Thêm thuộc tính cho mặt hàng.
    /// </summary>
    /// <param name="data">Thông tin thuộc tính cần thêm.</param>
    /// <returns>Mã thuộc tính được tạo.</returns>
    public static async Task<long> AddAttributeAsync(ProductAttribute data)
    {
        return await _productDB.AddAttributeAsync(data);
    }

    /// <summary>
    /// Cập nhật thuộc tính của mặt hàng.
    /// </summary>
    /// <param name="data">Thông tin thuộc tính cần cập nhật.</param>
    /// <returns>True nếu cập nhật thành công.</returns>
    public static async Task<bool> UpdateAttributeAsync(ProductAttribute data)
    {
        return await _productDB.UpdateAttributeAsync(data);
    }

    /// <summary>
    /// Xóa thuộc tính của mặt hàng.
    /// </summary>
    /// <param name="attributeId">Mã thuộc tính cần xóa.</param>
    /// <returns>True nếu xóa thành công.</returns>
    public static async Task<bool> DeleteAttributeAsync(long attributeId)
    {
        return await _productDB.DeleteAttributeAsync(attributeId);
    }

    /// <summary>
    /// Lấy danh sách thuộc tính của một mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng.</param>
    /// <returns>Danh sách thuộc tính.</returns>
    public static async Task<IReadOnlyList<ProductAttribute>> ListAttributesAsync(int productId)
    {
        return await _productDB.ListAttributesAsync(productId);
    }

    /// <summary>
    /// Lấy thông tin một thuộc tính của mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng.</param>
    /// <param name="attributeId">Mã thuộc tính.</param>
    /// <returns>Thuộc tính hoặc null.</returns>
    public static async Task<ProductAttribute?> GetAttributeAsync(int productId, long attributeId)
    {
        return await _productDB.GetAttributeAsync(productId, attributeId);
    }

    /// <summary>
    /// Thêm ảnh cho mặt hàng.
    /// </summary>
    /// <param name="data">Thông tin ảnh cần thêm.</param>
    /// <returns>Mã ảnh được tạo.</returns>
    public static async Task<long> AddPhotoAsync(ProductPhoto data)
    {
        return await _productDB.AddPhotoAsync(data);
    }

    /// <summary>
    /// Cập nhật ảnh của mặt hàng.
    /// </summary>
    /// <param name="data">Thông tin ảnh cần cập nhật.</param>
    /// <returns>True nếu cập nhật thành công.</returns>
    public static async Task<bool> UpdatePhotoAsync(ProductPhoto data)
    {
        return await _productDB.UpdatePhotoAsync(data);
    }

    /// <summary>
    /// Xóa ảnh của mặt hàng.
    /// </summary>
    /// <param name="photoId">Mã ảnh cần xóa.</param>
    /// <returns>True nếu xóa thành công.</returns>
    public static async Task<bool> DeletePhotoAsync(long photoId)
    {
        return await _productDB.DeletePhotoAsync(photoId);
    }

    /// <summary>
    /// Lấy danh sách ảnh của một mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng.</param>
    /// <returns>Danh sách ảnh.</returns>
    public static async Task<IReadOnlyList<ProductPhoto>> ListPhotosAsync(int productId)
    {
        return await _productDB.ListPhotosAsync(productId);
    }

    /// <summary>
    /// Lấy thông tin một ảnh của mặt hàng.
    /// </summary>
    /// <param name="productId">Mã mặt hàng.</param>
    /// <param name="photoId">Mã ảnh.</param>
    /// <returns>Ảnh hoặc null.</returns>
    public static async Task<ProductPhoto?> GetPhotoAsync(int productId, long photoId)
    {
        return await _productDB.GetPhotoAsync(productId, photoId);
    }

    // ── Suppliers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy toàn bộ danh sách nhà cung cấp.
    /// </summary>
    /// <returns>Danh sách nhà cung cấp.</returns>
    public static async Task<IReadOnlyList<Supplier>> ListSuppliersAsync()
    {
        var result = await _supplierDB.ListAsync(new PaginationSearchInput { PageSize = 0 });
        return result.DataItems;
    }

    /// <summary>
    /// Lấy thông tin một nhà cung cấp.
    /// </summary>
    /// <param name="supplierId">Mã nhà cung cấp.</param>
    /// <returns>Nhà cung cấp hoặc null.</returns>
    public static async Task<Supplier?> GetSupplierAsync(int supplierId)
    {
        return await _supplierDB.GetAsync(supplierId);
    }

    // ── Provinces ────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy toàn bộ danh sách tỉnh/thành.
    /// </summary>
    /// <returns>Danh sách tỉnh/thành.</returns>
    public static async Task<IReadOnlyList<Province>> ListProvincesAsync()
    {
        return await _provinceDB.ListAsync();
    }
}
