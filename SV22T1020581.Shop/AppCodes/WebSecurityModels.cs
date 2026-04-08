using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SV22T1020581.Shop.AppCodes
{
    /// <summary>
    /// Thông tin tài khoản người dùng được lưu trong phiên đăng nhập (cookie)
    /// </summary>
    public class WebUserData
    {
        /// <summary>
        /// ID người dùng
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// Tên hiển thị
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// Danh sách vai trò
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// Lấy danh sách các Claim chứa thông tin của user
        /// </summary>
        /// <returns>Danh sách claim</returns>
        private List<Claim> Claims
        {
            get
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(nameof(UserId), UserId ?? ""),
                    new Claim(nameof(UserName), UserName ?? ""),
                    new Claim(nameof(DisplayName), DisplayName ?? ""),
                    new Claim(nameof(Email), Email ?? ""),
                    new Claim(nameof(Photo), Photo ?? "")
                };
                if (Roles != null)
                    foreach (var role in Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                return claims;
            }
        }

        /// <summary>
        /// Tạo ClaimsPrincipal dựa trên thông tin của người dùng
        /// </summary>
        /// <returns>ClaimsPrincipal đã được tạo</returns>
        public ClaimsPrincipal CreatePrincipal()
        {
            var claimIdentity = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            return claimPrincipal;
        }
    }

    /// <summary>
    /// Định nghĩa tên các role sử dụng trong phân quyền
    /// </summary>
    public class WebUserRoles
    {
        /// <summary>
        /// Khách hàng
        /// </summary>
        public const string Customer = "customer";
    }

    /// <summary>
    /// Các phương thức mở rộng cho xác thực tài khoản người dùng
    /// </summary>
    public static class WebUserExtensions
    {
        /// <summary>
        /// Đọc thông tin của user từ ClaimsPrincipal
        /// </summary>
        /// <param name="principal">Đối tượng principal chứa claims</param>
        /// <returns>Thông tin người dùng hoặc null</returns>
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                    return null;

                var userData = new WebUserData();

                userData.UserId = principal.FindFirstValue(nameof(userData.UserId));
                userData.UserName = principal.FindFirstValue(nameof(userData.UserName));
                userData.DisplayName = principal.FindFirstValue(nameof(userData.DisplayName));
                userData.Email = principal.FindFirstValue(nameof(userData.Email));
                userData.Photo = principal.FindFirstValue(nameof(userData.Photo));

                userData.Roles = new List<string>();
                foreach (var claim in principal.FindAll(ClaimTypes.Role))
                {
                    userData.Roles.Add(claim.Value);
                }

                return userData;
            }
            catch
            {
                return null;
            }
        }
    }
}
