using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020581.BusinessLayers;
using SV22T1020581.Shop.AppCodes;

namespace SV22T1020581.Shop.Controllers
{
    /// <summary>Controller tài khoản khách hàng (Shop): đăng nhập, đăng ký, đổi mật khẩu.</summary>
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ModelState.Clear();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            ViewBag.Username = username;
            ViewBag.Password = password;
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập email và mật khẩu!");
                return View();
            }

            var userAccount = await UserAccountService.Authorize(AccountTypes.Customer, username.Trim(), password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Email hoặc mật khẩu không đúng! Vui lòng thử lại.");
                return View();
            }

            var roles = new List<string> { WebUserRoles.Customer };
            if (!string.IsNullOrWhiteSpace(userAccount.RoleNames))
            {
                foreach (var role in userAccount.RoleNames.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    roles.Add(role.Trim());
            }

            var webUser = new WebUserData
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo ?? "",
                Roles = roles
            };
            var principal = webUser.CreatePrincipal();

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 4)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Password), "Mật khẩu tối thiểu 4 ký tự.");
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.ConfirmPassword), "Mật khẩu xác nhận không trùng khớp.");
                return View(model);
            }

            var customerId = await CustomerAccountService.RegisterAsync(
                model.CustomerName.Trim(),
                string.IsNullOrWhiteSpace(model.ContactName) ? model.CustomerName.Trim() : model.ContactName.Trim(),
                model.Email.Trim(),
                model.Password,
                string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                string.IsNullOrWhiteSpace(model.Province) ? null : model.Province.Trim(),
                string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim());

            if (customerId <= 0)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email đã được sử dụng hoặc không thể tạo tài khoản.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        /// <summary>Trang thông tin tài khoản khách hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out var customerId))
                return RedirectToAction(nameof(Login));

            var customer = await CustomerAccountService.GetCustomerAsync(customerId);
            return View(customer);
        }

        /// <summary>Form chỉnh sửa thông tin cá nhân.</summary>
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out var customerId))
                return RedirectToAction(nameof(Login));

            var customer = await CustomerAccountService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction(nameof(Profile));

            ViewBag.Provinces = await GetProvinceSelectListAsync();
            var model = new EditProfileViewModel
            {
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Email = customer.Email,
                Phone = customer.Phone,
                Province = customer.Province,
                Address = customer.Address
            };
            return View(model);
        }

        /// <summary>Lưu thông tin cá nhân và cập nhật phiên đăng nhập (tên, email hiển thị).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out var customerId))
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            var existing = await CustomerAccountService.GetCustomerAsync(customerId);
            if (existing == null)
                return RedirectToAction(nameof(Profile));

            var email = model.Email.Trim();
            if (!await CustomerAccountService.ValidateEmailAsync(email, customerId))
            {
                ModelState.AddModelError(nameof(EditProfileViewModel.Email), "Email này đã được sử dụng bởi tài khoản khác.");
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            existing.CustomerName = model.CustomerName.Trim();
            existing.ContactName = string.IsNullOrWhiteSpace(model.ContactName) ? model.CustomerName.Trim() : model.ContactName.Trim();
            existing.Email = email;
            existing.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            existing.Province = string.IsNullOrWhiteSpace(model.Province) ? null : model.Province.Trim();
            existing.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

            var ok = await CustomerAccountService.UpdateCustomerAsync(existing);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật thông tin. Vui lòng thử lại.");
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            var roles = new List<string> { WebUserRoles.Customer };
            if (userData.Roles != null)
            {
                foreach (var r in userData.Roles.Where(x => x != WebUserRoles.Customer))
                    roles.Add(r);
            }

            var webUser = new WebUserData
            {
                UserId = customerId.ToString(),
                UserName = existing.Email,
                DisplayName = existing.CustomerName,
                Email = existing.Email,
                Photo = userData.Photo ?? "",
                Roles = roles
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                webUser.CreatePrincipal(),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });

            TempData["SuccessMessage"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Profile));
        }

        private static async Task<List<SelectListItem>> GetProvinceSelectListAsync()
        {
            var list = new List<SelectListItem>
            {
                new() { Value = "", Text = "-- Chọn tỉnh/thành phố --" }
            };
            var provinces = await CatalogDataService.ListProvincesAsync();
            foreach (var item in provinces)
                list.Add(new SelectListItem { Value = item.ProvinceName, Text = item.ProvinceName });
            return list;
        }

        // Đăng xuất khỏi hệ thống.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Hiển thị trang thay đổi mật khẩu.
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Xử lý đổi mật khẩu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không trùng khớp!");
                return View();
            }

            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserId) || !int.TryParse(userData.UserId, out var customerId))
                return RedirectToAction("Login");

            var ok = await CustomerAccountService.ChangePasswordAsync(customerId, oldPassword, newPassword);
            if (!ok)
            {
                ModelState.AddModelError("Error", "Mật khẩu hiện tại không đúng!");
                return View();
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>Form đăng ký khách hàng.</summary>
    public class RegisterViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Tên giao dịch")]
        public string? ContactName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Tỉnh/Thành phố")]
        public string? Province { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Xác nhận mật khẩu")]
        [System.ComponentModel.DataAnnotations.Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>Chỉnh sửa hồ sơ khách hàng (Shop).</summary>
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = "";

        [Display(Name = "Tên giao dịch")]
        public string? ContactName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = "";

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public string? Province { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }
    }
}
