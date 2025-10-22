using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(email, password);
                if (user != null)
                {
                    // Seçilen rol ile kullanıcının gerçek rolü eşleşiyor mu kontrol et
                    if (user.Role != role)
                    {
                        TempData["ErrorMessage"] = $"Bu hesap {(user.Role == "Student" ? "öğrenci" : "öğretmen")} hesabıdır. Lütfen doğru rolü seçin.";
                        return View();
                    }

                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    TempData["SuccessMessage"] = $"Hoş geldiniz, {user.FullName}!";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = "E-posta veya şifre hatalı!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası");
                TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string role)
        {
            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur!");
                    return View();
                }

                var user = await _userService.RegisterAsync(fullName, email, password, role);
                
                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register hatası");
                ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
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





