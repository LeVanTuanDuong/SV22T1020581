using Microsoft.AspNetCore.Mvc;
using SV22T1020581.Shop.AppCodes;

namespace SV22T1020581.Shop.ViewComponents
{
    /// <summary>
    /// ViewComponent hiển thị số lượng sản phẩm trong giỏ hàng
    /// </summary>
    public class CartBadgeViewComponent : ViewComponent
    {
        /// <summary>
        /// Trả về View chứa badge số lượng giỏ hàng
        /// </summary>
        public IViewComponentResult Invoke()
        {
            int totalQty = ShoppingCartService.GetTotalQuantity();
            return View(totalQty);
        }
    }
}
