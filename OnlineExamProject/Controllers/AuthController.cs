using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExamProject.Models;
using OnlineExamProject.Services;
using System.Diagnostics;

namespace OnlineExamProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user != null)
                {
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(email, password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    TempData["SuccessMessage"] = $"Hoş geldiniz, {user.FullName}!";

                    // Kullanıcı rolüne göre yönlendir
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                TempData["ErrorMessage"] = "E-posta veya şifre hatalı!";
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Veritabanı bağlantı hatası: {Message}", sqlEx.Message);
                TempData["ErrorMessage"] = $"Veritabanı bağlantı hatası: {sqlEx.Message}. Lütfen veritabanı ayarlarını kontrol edin.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası: {Message}\n{StackTrace}", ex.Message, ex.StackTrace);
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}. Lütfen tekrar deneyin.";
            }

            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
        }
    }
}





