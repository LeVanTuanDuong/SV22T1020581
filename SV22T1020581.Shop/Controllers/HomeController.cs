using Microsoft.AspNetCore.Mvc;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.Shop.Controllers
{
    /// <summary>
    /// Controller hiển thị trang chủ và sản phẩm
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Trang chủ - hiển thị sản phẩm nổi bật
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var featuredProducts = await CatalogDataService.GetFeaturedProductsAsync(8);
            var categories = await CatalogDataService.ListCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.ActiveCategory = null;
            return View(featuredProducts);
        }

        /// <summary>
        /// Hiển thị trang lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
