using Microsoft.AspNetCore.Mvc;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Catalog;

namespace SV22T1020581.Shop.ViewComponents
{
    /// <summary>
    /// ViewComponent hiển thị menu điều hướng chính với danh mục loại hàng
    /// </summary>
    public class MainNavViewComponent : ViewComponent
    {
        /// <summary>
        /// Trả về danh sách loại hàng cho menu
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await CatalogDataService.ListCategoriesAsync();
            return View(categories);
        }
    }
}
