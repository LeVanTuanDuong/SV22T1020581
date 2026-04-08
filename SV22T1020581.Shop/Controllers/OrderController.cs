using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Sales;
using SV22T1020581.Shop.AppCodes;
using System.Security.Claims;

namespace SV22T1020581.Shop.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        private const int PAGESIZE = 10;

        /// <summary>
        /// Hiển thị trang thanh toán (checkout)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                TempData["InfoMessage"] = "Giỏ hàng trống. Vui lòng chọn sản phẩm.";
                return RedirectToAction("Index", "Product");
            }

            var userData = (User as ClaimsPrincipal)?.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            var customer = await CustomerAccountService.GetCustomerAsync(int.Parse(userData.UserId ?? "0"));
            ViewBag.Provinces = await GetProvinces();
            ViewBag.TotalAmount = ShoppingCartService.GetTotalAmount();
            ViewBag.Cart = cart;

            var checkoutModel = new CheckoutViewModel
            {
                CustomerName = customer?.CustomerName ?? "",
                ContactName = customer?.ContactName ?? "",
                Email = customer?.Email ?? "",
                Phone = customer?.Phone ?? "",
                Address = customer?.Address ?? "",
                Province = customer?.Province ?? ""
            };

            return View(checkoutModel);
        }

        /// <summary>
        /// Xử lý đặt hàng
        /// </summary>
        /// <param name="model">Thông tin đặt hàng</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                TempData["InfoMessage"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Product");
            }

            var userData = (User as ClaimsPrincipal)?.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập tên người nhận.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại.");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError("Address", "Vui lòng nhập địa chỉ giao hàng.");
            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError("Province", "Vui lòng chọn tỉnh/thành phố.");

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await GetProvinces();
                ViewBag.TotalAmount = ShoppingCartService.GetTotalAmount();
                ViewBag.Cart = cart;
                return View(model);
            }

            var cartLines = cart.Select(m => new OrderCartLine
            {
                ProductID = m.ProductID,
                Quantity = m.Quantity,
                SalePrice = m.SalePrice
            }).ToList();

            int customerId = int.Parse(userData.UserId ?? "0");
            int orderId = await OrderService.CreateOrderAsync(
                customerId,
                model.Province,
                $"{model.Address}, {model.Province}",
                cartLines);

            if (orderId <= 0)
            {
                ModelState.AddModelError("Error", "Không thể tạo đơn hàng. Vui lòng thử lại.");
                ViewBag.Provinces = await GetProvinces();
                ViewBag.TotalAmount = ShoppingCartService.GetTotalAmount();
                ViewBag.Cart = cart;
                return View(model);
            }

            ShoppingCartService.ClearCart();

            return RedirectToAction(nameof(Success), new { orderId });
        }

        /// <summary>
        /// Hiển thị trang đặt hàng thành công
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await OrderService.GetOrderAsync(orderId);
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(order);
        }

        /// <summary>
        /// Hiển thị danh sách đơn hàng của khách hàng (lịch sử mua hàng)
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        [HttpGet]
        [Authorize]
        public Task<IActionResult> MyOrders(int page = 1) =>
            CustomerOrdersPageAsync(page, "Đơn hàng của tôi", "MyOrders");

        /// <summary>Tất cả đơn đã đặt (danh sách theo thời gian).</summary>
        [HttpGet]
        [Authorize]
        public Task<IActionResult> PurchaseHistory(int page = 1) =>
            CustomerOrdersPageAsync(page, "Lịch sử mua hàng", "PurchaseHistory");

        private async Task<IActionResult> CustomerOrdersPageAsync(int page, string pageTitle, string viewName)
        {
            var userData = (User as ClaimsPrincipal)?.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            int customerId = int.Parse(userData.UserId ?? "0");

            var orders = await OrderService.GetOrdersByCustomerAsync(customerId, page, PAGESIZE);
            ViewBag.Title = pageTitle;
            ViewBag.OrdersListAction = viewName == "PurchaseHistory" ? "PurchaseHistory" : "MyOrders";
            return View(viewName, orders);
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Detail(int id)
        {
            var userData = (User as ClaimsPrincipal)?.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            var order = await OrderService.GetOrderAsync(id);
            if (order == null)
            {
                return RedirectToAction("MyOrders");
            }

            int customerId = int.Parse(userData.UserId ?? "0");
            if (order.CustomerID != customerId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var details = await OrderService.GetOrderDetailsAsync(id);
            ViewBag.Details = details;
            ViewBag.Title = $"Đơn hàng #{id}";
            return View(order);
        }

        /// <summary>
        /// Lấy danh sách tỉnh/thành cho dropdown
        /// </summary>
        private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetProvinces()
        {
            var list = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = "", Text = "-- Tỉnh/Thành phố --" }
            };
            var provinces = await CatalogDataService.ListProvincesAsync();
            foreach (var item in provinces)
            {
                list.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName
                });
            }
            return list;
        }
    }

    /// <summary>
    /// Model dùng cho trang checkout
    /// </summary>
    public class CheckoutViewModel
    {
        /// <summary>
        /// Tên người nhận hàng
        /// </summary>
        public string CustomerName { get; set; } = "";
        /// <summary>
        /// Tên liên hệ giao dịch
        /// </summary>
        public string ContactName { get; set; } = "";
        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        public string Phone { get; set; } = "";
        /// <summary>
        /// Địa chỉ giao hàng
        /// </summary>
        public string Address { get; set; } = "";
        /// <summary>
        /// Tỉnh/thành giao hàng
        /// </summary>
        public string Province { get; set; } = "";
    }
}
