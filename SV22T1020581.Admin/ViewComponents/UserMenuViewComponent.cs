using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SV22T1020581.Admin.AppCodes;

namespace SV22T1020581.Admin.ViewComponents;

/// <summary>
/// Menu người dùng trên thanh header (tên, ảnh, đổi mật khẩu, thoát).
/// </summary>
public class UserMenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var data = (User as ClaimsPrincipal)?.GetUserData();
        var displayName = string.IsNullOrWhiteSpace(data?.DisplayName)
            ? (data?.Email ?? data?.UserName ?? "Người dùng")
            : data.DisplayName!;
        var photo = data?.Photo ?? "";

        return View(model: (displayName, photo));
    }
}
