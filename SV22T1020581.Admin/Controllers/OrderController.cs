using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Sales;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Admin.AppCodes;
using System.Globalization;
using SV22T1020581.Admin.Models;

namespace SV22T1020581.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        private const int ORDER_PAGE_SIZE = 15;
        private const int PRODUCT_PAGE_SIZE = 20;
        private const string ORDER_SEARCH_SESSION = "OrderSearchInput";
        private const string PRODUCT_SEARCH_SESSION = "ProductSearchForOrder";

        // --- QUẢN LÝ DANH SÁCH & TRA CỨU ---

        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<SV22T1020581.Models.Sales.OrderSearchInput>(ORDER_SEARCH_SESSION);
            if (input == null)
            {
                input = new SV22T1020581.Models.Sales.OrderSearchInput
                {
                    Page = 1,
                    PageSize = ORDER_PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    DateFrom = null,
                    DateTo = null
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và lọc đơn hàng
        /// </summary>
        public async Task<IActionResult> Search(SV22T1020581.Models.Sales.OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(ORDER_SEARCH_SESSION, input);
            return PartialView(result);
        }

        // --- LẬP ĐƠN HÀNG (GIỎ HÀNG) ---

        /// <summary>
        /// Hiển thị giao diện tạo đơn hàng (giỏ hàng)
        /// </summary>
        public async Task<IActionResult> Create(int page = 1, string searchValue = "")
        {
            var input = ApplicationContext.GetSessionData<SV22T1020581.Models.Catalog.ProductSearchInput>(PRODUCT_SEARCH_SESSION);
            if (input == null || Request.Query.ContainsKey("page") || Request.Query.ContainsKey("searchValue"))
            {
                input = new SV22T1020581.Models.Catalog.ProductSearchInput
                {
                    Page = page,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = searchValue ?? ""
                };
            }
            else
                input.PageSize = PRODUCT_PAGE_SIZE;

            var products = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_SESSION, input);

            var customersPaged = await PartnerDataService.ListCustomersAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 0,
                SearchValue = ""
            });

            ViewBag.Products = products;
            ViewBag.Customers = customersPaged.DataItems.ToList();
            ViewBag.Provinces = (await CatalogDataService.ListProvincesAsync()).ToList();
            ViewBag.ProductSearch = input.SearchValue;
            ViewBag.Cart = ShoppingCartService.GetShoppingCart();

            var currentUser = User.GetUserData();
            ViewBag.CurrentEmployeeID = currentUser?.UserId != null
                ? int.Parse(currentUser.UserId!) : 0;
            ViewBag.CurrentEmployeeName = currentUser?.DisplayName ?? "Nhân viên";

            return View(input);
        }

        /// <summary>
        /// Lập đơn hàng từ giỏ hàng (form trên trang Create).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int customerId, string? deliveryProvince, string? deliveryAddress)
        {
            if (customerId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn khách hàng.";
                return RedirectToAction(nameof(Create));
            }

            if (string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ tỉnh/thành và địa chỉ giao hàng.";
                return RedirectToAction(nameof(Create));
            }

            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng chọn mặt hàng.";
                return RedirectToAction(nameof(Create));
            }

            var order = new Order
            {
                CustomerID = customerId,
                DeliveryProvince = deliveryProvince.Trim(),
                DeliveryAddress = deliveryAddress.Trim(),
                EmployeeID = null
            };

            var orderId = await SalesDataService.AddOrderAsync(order);
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Không thể tạo đơn hàng.";
                return RedirectToAction(nameof(Create));
            }

            foreach (var item in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });
            }

            ShoppingCartService.ClearCart();
            TempData["SuccessMessage"] = $"Đã lập đơn hàng #{orderId}.";
            return RedirectToAction(nameof(Detail), new { id = orderId });
        }

        /// <summary>
        /// Tìm kiếm mặt hàng để thêm vào đơn hàng
        /// </summary>
        public async Task<IActionResult> SearchProduct(SV22T1020581.Models.Catalog.ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_SESSION, input);
            return PartialView(result);
        }

        /// <summary>
        /// Hiển thị giỏ hàng
        /// </summary>
        public IActionResult ShowShoppingCart()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            return PartialView("ShowShoppingCart", cart);
        }

        /// <summary>
        /// Thêm mặt hàng vào giỏ hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(OrderDetailViewInfo item)
        {
            if (item.ProductID <= 0 || item.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng không hợp lệ.";
                return RedirectToAction(nameof(Create));
            }

            var product = await CatalogDataService.GetProductAsync(item.ProductID);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mặt hàng.";
                return RedirectToAction(nameof(Create));
            }

            if (!product.IsSelling)
            {
                TempData["ErrorMessage"] = "Mặt hàng đang ngừng bán.";
                return RedirectToAction(nameof(Create));
            }

            if (item.SalePrice <= 0)
                item.SalePrice = product.Price;

            item.ProductName = product.ProductName;
            item.Unit = product.Unit;
            item.Photo = product.Photo ?? "";

            ShoppingCartService.AddCartItem(item);
            TempData["SuccessMessage"] = $"Đã thêm {product.ProductName} vào giỏ hàng.";
            return RedirectToAction(nameof(Create));
        }

        /// <summary>
        /// Xóa mặt hàng khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            if (productId > 0)
                ShoppingCartService.RemoveCartItem(productId);
            TempData["SuccessMessage"] = "Đã xóa mặt hàng khỏi giỏ.";
            return RedirectToAction(nameof(Create));
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            ShoppingCartService.ClearCart();
            TempData["SuccessMessage"] = "Đã xóa toàn bộ mặt hàng trong giỏ hàng.";
            return RedirectToAction(nameof(Create));
        }

        /// <summary>
        /// Cập nhật số lượng và giá của mặt hàng trong giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult UpdateCartItem(int id, int quantity, decimal salePrice)
        {
            if (quantity <= 0 || salePrice <= 0)
                return Json("Số lượng và giá bán không hợp lệ");

            ShoppingCartService.UpdateCartItem(id, quantity, salePrice);
            return Json("");
        }

        /// <summary>
        /// Khởi tạo đơn hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Init(string customerName, string deliveryProvince, string address)
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
                return Json("Giỏ hàng trống. Vui lòng chọn mặt hàng.");

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(address))
                return Json("Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao hàng.");

            // Tìm khách hàng theo tên (chính xác tuyệt đối)
            var customer = (await PartnerDataService.ListCustomersAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 1,
                SearchValue = customerName
            })).DataItems.FirstOrDefault(c => c.CustomerName.Trim().ToLower() == customerName.Trim().ToLower());

            int customerID = 0;
            if (customer != null)
            {
                customerID = customer.CustomerID;
            }
            else
            {
                // Nếu không tìm thấy, tạo khách hàng mới với thông tin cơ bản
                customerID = await PartnerDataService.AddCustomerAsync(new SV22T1020581.Models.Partner.Customer
                {
                    CustomerName = customerName,
                    ContactName = customerName, // Dùng tạm tên khách hàng cho tên giao dịch
                    Province = deliveryProvince,
                    Address = address,
                    Email = "" // Để trống email
                });
            }

            if (customerID <= 0)
                return Json("Không thể xác định hoặc tạo khách hàng mới.");

            Order order = new Order
            {
                CustomerID = customerID,
                OrderTime = DateTime.Now,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = address,
                EmployeeID = null,
                Status = (int)OrderStatusEnum.New
            };

            int orderID = await SalesDataService.AddOrderAsync(order);
            if (orderID > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail
                    {
                        OrderID = orderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice
                    });
                }
                ShoppingCartService.ClearCart();
                return Json(new { success = true, orderID = orderID });
            }

            return Json("Không thể lập đơn hàng.");
        }

        // --- CHI TIẾT & XỬ LÝ NGHIỆP VỤ ---

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            SV22T1020581.Models.Partner.Customer? customer = null;
            if (order.CustomerID.HasValue)
            {
                customer = await PartnerDataService.GetCustomerAsync(order.CustomerID.Value);
            }

            SV22T1020581.Models.HR.Employee? employee = null;
            if (order.EmployeeID.HasValue)
            {
                employee = await HRDataService.GetEmployeeAsync(order.EmployeeID.Value);
            }

            var model = new OrderDetailPageModel
            {
                Order = order,
                Customer = customer,
                Employee = employee,
                Details = details
            };
            return View(model);
        }

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        public async Task<IActionResult> Accept(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var model = new OrderAcceptDialogModel
            {
                OrderId = id,
                Employees = new List<SV22T1020581.Models.HR.Employee>()
            };
            var allEmployees = await HRDataService.ListEmployeesAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            model.Employees = allEmployees.DataItems.ToList();

            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái giao hàng - GET
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var model = new OrderShippingDialogModel
            {
                OrderId = id,
                Shippers = new List<SV22T1020581.Models.Partner.Shipper>()
            };
            var allShippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            model.Shippers = allShippers.DataItems.ToList();

            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái giao hàng - POST
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            if (shipperID <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn người giao hàng.";
                return RedirectToAction("Detail", new { id });
            }
            bool result = await SalesDataService.ShipOrderAsync(id, shipperID);
            if (!result) TempData["ErrorMessage"] = "Không thể chuyển trạng thái sang đang giao hàng.";
            else TempData["SuccessMessage"] = "Đã chuyển đơn hàng sang trạng thái đang giao hàng.";
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Duyệt đơn hàng - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id, int employeeId)
        {
            if (employeeId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn nhân viên phụ trách.";
                return RedirectToAction("Detail", new { id });
            }
            bool result = await SalesDataService.AcceptOrderAsync(id, employeeId);
            if (!result) TempData["ErrorMessage"] = "Không thể duyệt đơn hàng này.";
            else TempData["SuccessMessage"] = "Duyệt đơn hàng thành công.";
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hộp thoại xác nhận hoàn tất đơn hàng (GET — nội dung load vào modal).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Finish(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Không tìm thấy đơn hàng.</p></div>",
                    "text/html; charset=utf-8");
            }
            if (order.Status != (int)OrderStatusEnum.Shipping)
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Chỉ có thể hoàn tất khi đơn đang ở trạng thái <strong>Đang giao</strong>.</p></div>",
                    "text/html; charset=utf-8");
            }
            return View(id);
        }

        /// <summary>
        /// Hoàn tất đơn hàng (POST sau khi xác nhận trong modal).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishConfirm(int id)
        {
            bool result = await SalesDataService.CompleteOrderAsync(id);
            if (!result) TempData["ErrorMessage"] = "Không thể hoàn tất đơn hàng này.";
            else TempData["SuccessMessage"] = "Đơn hàng đã hoàn tất thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public async Task<IActionResult> Reject(int id)
        {
            int employeeID = 1;
            bool result = await SalesDataService.RejectOrderAsync(id, employeeID);
            if (!result) TempData["ErrorMessage"] = "Không thể từ chối đơn hàng này.";
            else TempData["SuccessMessage"] = "Đã từ chối đơn hàng thành công.";
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hộp thoại xác nhận hủy đơn (GET — nội dung modal).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Không tìm thấy đơn hàng.</p></div>",
                    "text/html; charset=utf-8");
            }
            if (order.Status != (int)OrderStatusEnum.New && order.Status != (int)OrderStatusEnum.Accepted)
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Chỉ có thể hủy đơn ở trạng thái <strong>Chờ duyệt</strong> hoặc <strong>Đã duyệt</strong>.</p></div>",
                    "text/html; charset=utf-8");
            }
            return View(id);
        }

        /// <summary>
        /// Hủy đơn hàng (POST sau khi xác nhận trong modal).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirm(int id)
        {
            bool result = await SalesDataService.CancelOrderAsync(id);
            if (!result) TempData["ErrorMessage"] = "Không thể hủy đơn hàng này.";
            else TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Hộp thoại xác nhận xóa đơn (GET — nội dung modal).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Không tìm thấy đơn hàng.</p></div>",
                    "text/html; charset=utf-8");
            }
            if (order.Status is not (1 or -1 or -2))
            {
                return Content(
                    "<div class=\"modal-body text-danger\"><p>Chỉ có thể xóa đơn ở trạng thái chờ duyệt, đã hủy hoặc bị từ chối.</p></div>",
                    "text/html; charset=utf-8");
            }
            return View(id);
        }

        /// <summary>
        /// Xóa vĩnh viễn đơn hàng (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);
            if (!result) TempData["ErrorMessage"] = "Không thể xóa đơn hàng này.";
            else TempData["SuccessMessage"] = "Đã xóa đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Chỉnh sửa mặt hàng trong đơn hàng (chỉ khi đơn hàng chưa được duyệt)
        /// </summary>
        public async Task<IActionResult> EditCartItem(int id, int productId)
        {
            var detail = await SalesDataService.GetDetailAsync(id, productId);
            return View(detail);
        }

        /// <summary>
        /// Cập nhật chi tiết mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(OrderDetail data)
        {
            if (data.Quantity <= 0 || data.SalePrice <= 0)
                return Json("Số lượng và giá bán không hợp lệ");
            
            bool result = await SalesDataService.UpdateDetailAsync(data);
            if (!result) return Json("Không thể cập nhật chi tiết đơn hàng");

            return Json("");
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteCartItem(int id, int productId)
        {
            bool result = await SalesDataService.DeleteDetailAsync(id, productId);
            if (!result) TempData["ErrorMessage"] = "Không thể xóa mặt hàng khỏi đơn hàng";
            return RedirectToAction("Detail", new { id });
        }
    }
}