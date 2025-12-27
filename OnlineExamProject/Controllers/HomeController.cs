using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IExamService _examService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IExamService examService, IUserService userService)
        {
            _logger = logger;
            _examService = examService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // Admin ise Admin paneline yönlendir
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.UserName = user.FullName;
            ViewBag.UserRole = user.Role;

            if (user.Role == "Student")
            {
                var activeExams = await _examService.GetExamsForStudentAsync(userId.Value);
                ViewBag.ActiveExams = activeExams;
            }
            else if (user.Role == "Teacher")
            {
                var myExams = await _examService.GetExamsForTeacherAsync(userId.Value);
                ViewBag.MyExams = myExams;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
