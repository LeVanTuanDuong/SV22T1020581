using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Sales;
using SV22T1020581.Shop.AppCodes;

namespace SV22T1020581.Shop.Controllers
{
    /// <summary>
    /// Controller quản lý sản phẩm cho khách hàng
    /// </summary>
    public class ProductController : Controller
    {
        private const int PAGESIZE = 12;

        /// <summary>
        /// Hiển thị danh sách sản phẩm với bộ lọc (tìm kiếm, loại hàng, khoảng giá)
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="categoryID">Mã loại hàng (0 = tất cả)</param>
        /// <param name="minPrice">Giá tối thiểu</param>
        /// <param name="maxPrice">Giá tối đa</param>
        /// <param name="searchValue">Giá trị tìm kiếm theo tên</param>
        public async Task<IActionResult> Index(
            int page = 1,
            int categoryID = 0,
            decimal minPrice = 0,
            decimal maxPrice = 0,
            string searchValue = "")
        {
            var input = new ProductSearchInput()
            {
                Page = page,
                PageSize = PAGESIZE,
                SearchValue = searchValue,
                CategoryID = categoryID,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var result = await CatalogDataService.ListProductsAsync(input);
            var categories = await CatalogDataService.ListCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.CategoryID = categoryID;
            ViewBag.ActiveCategory = categoryID > 0 ? (int?)categoryID : null;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SearchValue = searchValue;
            ViewBag.Title = string.IsNullOrWhiteSpace(searchValue)
                ? "Danh sách sản phẩm"
                : $"Kết quả tìm kiếm: \"{searchValue}\"";

            return View(result);
        }

        /// <summary>
        /// Hiển thị chi tiết sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var relatedProducts = await CatalogDataService.GetRelatedProductsAsync(
                product.CategoryID ?? 0, id, 4);
            var photos = await CatalogDataService.ListProductPhotosAsync(id);
            var attributes = await CatalogDataService.ListProductAttributesAsync(id);
            var categories = await CatalogDataService.ListCategoriesAsync();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.Photos = photos;
            ViewBag.Attributes = attributes;
            ViewBag.Categories = categories;
            ViewBag.ActiveCategory = product.CategoryID is int cid && cid > 0 ? cid : (int?)null;
            ViewBag.Title = product.ProductName;

            return View(product);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng (AJAX)
        /// </summary>
        /// <param name="productID">Mã sản phẩm</param>
        /// <param name="quantity">Số lượng</param>
        [HttpPost]
        public IActionResult AddToCart(int productID, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var product = CatalogDataService.GetProductAsync(productID).GetAwaiter().GetResult();
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var item = new OrderDetailViewInfo()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Photo = product.Photo ?? "",
                Unit = product.Unit,
                SalePrice = product.Price,
                Quantity = quantity
            };

            ShoppingCartService.AddCartItem(item);

            int totalQty = ShoppingCartService.GetTotalQuantity();
            decimal totalAmount = ShoppingCartService.GetTotalAmount();

            return Json(new
            {
                success = true,
                message = $"Đã thêm \"{product.ProductName}\" vào giỏ hàng!",
                totalQuantity = totalQty,
                totalAmount = totalAmount.ToString("N0")
            });
        }

        /// <summary>
        /// Lấy danh sách loại hàng cho filter (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await CatalogDataService.ListCategoriesAsync();
            var items = categories.Select(c => new { value = c.CategoryID, text = c.CategoryName }).ToList();
            return Json(items);
        }
    }
}
