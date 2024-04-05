using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020025.BusinessLayers;

namespace SV20T1020025.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(); 
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username = "", string password = "")
        {
            ViewBag.Username = username;
            
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) 
            {
                ModelState.AddModelError("Error", "Nhập tên và mật khẩu");
                return View();
            }

            var userAccount = UserAccountService.Authorize(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại!");
                return View();
            }

            //Đăng nhập thành công, tạo dữ liệu để lưu thông tin đăng nhập
            var userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                SessionId = HttpContext.Session.Id,
                AdditionalData = "",
                Roles = userAccount.RoleNames.Split(',').ToList(),
            };
            //Thiết lập  phiên đăng nhập cho tài khoản
            await HttpContext.SignInAsync(userData.CreatePrincipal());
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View(); // Chuyển hướng đến trang ChangePassword

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            // Kiểm tra xem đã nhập mật khẩu hiện tại, mật khẩu mới và xác thực mật khẩu mới chưa
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                ViewBag.ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            // Lấy thông tin người dùng đang đăng nhập
            var userData = User.GetUserData();

            // Kiểm tra mật khẩu hiện tại nhập vào có khớp với mật khẩu của người dùng trong cơ sở dữ liệu không
            if (!UserAccountService.VerifyPassword(userData.UserId, currentPassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            // Kiểm tra mật khẩu mới và nhập lại mật khẩu mới có trùng nhau không
            if (newPassword != confirmNewPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác thực không trùng với mật khẩu mới.";
                return View();
            }

            // Cập nhật mật khẩu mới cho người dùng
            if (UserAccountService.ChangePassword(userData.UserName, currentPassword, newPassword))
            {
                // Đăng xuất người dùng sau khi đổi mật khẩu thành công
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi đổi mật khẩu.";
                return View();
            }
        }


    }

}
