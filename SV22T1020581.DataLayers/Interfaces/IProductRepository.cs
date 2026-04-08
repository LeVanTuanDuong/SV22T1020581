using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Common;

namespace SV22T1020581.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu mặt hàng, thuộc tính và ảnh.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>Danh sách sản phẩm phân trang.</summary>
        Task<PagedResult<Product>> ListAsync(Models.Catalog.ProductSearchInput input);

        /// <summary>Lấy một sản phẩm theo mã.</summary>
        Task<Product?> GetAsync(int productId);

        /// <summary>Thêm sản phẩm mới.</summary>
        Task<int> AddAsync(Product data);

        /// <summary>Cập nhật sản phẩm.</summary>
        Task<bool> UpdateAsync(Product data);

        /// <summary>Xóa sản phẩm.</summary>
        Task<bool> DeleteAsync(int productId);

        /// <summary>Kiểm tra sản phẩm có đang được sử dụng không.</summary>
        Task<bool> IsUsedAsync(int productId);

        /// <summary>Danh sách thuộc tính của sản phẩm.</summary>
        Task<IReadOnlyList<ProductAttribute>> ListAttributesAsync(int productId);

        /// <summary>Lấy một thuộc tính.</summary>
        Task<ProductAttribute?> GetAttributeAsync(int productId, long attributeId);

        /// <summary>Thêm thuộc tính.</summary>
        Task<long> AddAttributeAsync(ProductAttribute data);

        /// <summary>Cập nhật thuộc tính.</summary>
        Task<bool> UpdateAttributeAsync(ProductAttribute data);

        /// <summary>Xóa thuộc tính.</summary>
        Task<bool> DeleteAttributeAsync(long attributeId);

        /// <summary>Danh sách ảnh của sản phẩm.</summary>
        Task<IReadOnlyList<ProductPhoto>> ListPhotosAsync(int productId);

        /// <summary>Lấy một ảnh.</summary>
        Task<ProductPhoto?> GetPhotoAsync(int productId, long photoId);

        /// <summary>Thêm ảnh.</summary>
        Task<long> AddPhotoAsync(ProductPhoto data);

        /// <summary>Cập nhật ảnh.</summary>
        Task<bool> UpdatePhotoAsync(ProductPhoto data);

        /// <summary>Xóa ảnh.</summary>
        Task<bool> DeletePhotoAsync(long photoId);

        /// <summary>Lấy sản phẩm nổi bật.</summary>
        Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int topPerCategory = 1);

        /// <summary>Lấy sản phẩm cùng danh mục.</summary>
        Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int categoryID, int currentProductID, int take = 4);
    }
}
