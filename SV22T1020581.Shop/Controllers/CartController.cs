using Microsoft.AspNetCore.Mvc;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Sales;
using SV22T1020581.Shop.AppCodes;
using System.Security.Claims;

namespace SV22T1020581.Shop.Controllers
{
    /// <summary>
    /// Controller quản lý giỏ hàng
    /// </summary>
    public class CartController : Controller
    {
        /// <summary>
        /// Hiển thị giỏ hàng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            decimal totalAmount = ShoppingCartService.GetTotalAmount();
            ViewBag.TotalAmount = totalAmount;
            return View(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng (từ trang chi tiết)
        /// </summary>
        /// <param name="productID">Mã sản phẩm</param>
        /// <param name="quantity">Số lượng</param>
        [HttpPost]
        public IActionResult Add(int productID, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var product = CatalogDataService.GetProductAsync(productID).GetAwaiter().GetResult();
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Product");
            }

            var item = new SV22T1020581.Models.Sales.OrderDetailViewInfo()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Photo = product.Photo ?? "",
                Unit = product.Unit,
                SalePrice = product.Price,
                Quantity = quantity
            };

            ShoppingCartService.AddCartItem(item);
            TempData["SuccessMessage"] = $"Đã thêm \"{product.ProductName}\" vào giỏ hàng!";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="productID">Mã sản phẩm</param>
        /// <param name="quantity">Số lượng mới</param>
        [HttpPost]
        public IActionResult Update(int productID, int quantity)
        {
            if (quantity <= 0)
            {
                ShoppingCartService.RemoveCartItem(productID);
            }
            else
            {
                var item = ShoppingCartService.GetCartItem(productID);
                if (item != null)
                {
                    ShoppingCartService.UpdateCartItem(productID, quantity, item.SalePrice);
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa một sản phẩm khỏi giỏ hàng
        /// </summary>
        /// <param name="productID">Mã sản phẩm cần xóa</param>
        [HttpPost]
        public IActionResult Remove(int productID)
        {
            ShoppingCartService.RemoveCartItem(productID);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult Clear()
        {
            ShoppingCartService.ClearCart();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Lấy số lượng sản phẩm trong giỏ hàng (AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetCartCount()
        {
            int count = ShoppingCartService.GetTotalQuantity();
            return Json(new { count });
        }
    }
}
