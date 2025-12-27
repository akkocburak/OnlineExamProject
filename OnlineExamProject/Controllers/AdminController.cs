using Microsoft.AspNetCore.Mvc;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;

        public AdminController(IUserService userService, ICourseService courseService)
        {
            _userService = userService;
            _courseService = courseService;
        }

        // Admin yetkisi kontrolü
        private async Task<bool> IsAdminAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var user = await _userService.GetUserByIdAsync(userId.Value);
            return user != null && user.Role == "Admin";
        }

        // Admin ana sayfa
        public async Task<IActionResult> Index()
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            var students = await _userService.GetUsersByRoleAsync("Student");
            var teachers = await _userService.GetUsersByRoleAsync("Teacher");
            var courses = await _courseService.GetAllCoursesAsync();

            ViewBag.StudentCount = students.Count();
            ViewBag.TeacherCount = teachers.Count();
            ViewBag.CourseCount = courses.Count();

            return View();
        }

        // Kullanıcı listesi
        public async Task<IActionResult> Users(string role = "")
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            IEnumerable<User> users;
            if (string.IsNullOrEmpty(role))
            {
                users = await _userService.GetAllUsersAsync();
            }
            else
            {
                users = await _userService.GetUsersByRoleAsync(role);
            }

            ViewBag.Role = role;
            return View(users);
        }

        // Yeni kullanıcı ekleme - GET
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdminAsync().Result)
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // Yeni kullanıcı ekleme - POST
        [HttpPost]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string role, string? department, string? @class)
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
                {
                    TempData["ErrorMessage"] = "Tüm zorunlu alanları doldurun!";
                    return View();
                }

                // Admin rolü sadece manuel olarak veritabanından verilebilir
                if (role == "Admin")
                {
                    TempData["ErrorMessage"] = "Admin rolü buradan verilemez!";
                    return View();
                }

                var user = await _userService.RegisterAsync(fullName, email, password, role);
                
                // Öğrenci ise bölüm ve sınıf bilgilerini güncelle
                if (role == "Student" && (!string.IsNullOrEmpty(department) || !string.IsNullOrEmpty(@class)))
                {
                    user.Department = department;
                    user.Class = @class;
                    await _userService.UpdateUserAsync(user);

                    // Öğrenciyi uygun derslere otomatik ata
                    await AssignStudentToCoursesAsync(user);
                }

                TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu!";
                return RedirectToAction("Users");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            }

            return View();
        }

        // Öğrenciyi uygun derslere otomatik atama
        private async Task AssignStudentToCoursesAsync(User student)
        {
            if (student.Role != "Student" || string.IsNullOrEmpty(student.Department) || string.IsNullOrEmpty(student.Class))
                return;

            // Öğrencinin bölüm ve sınıfına uygun dersleri bul
            var courses = await _courseService.GetAllCoursesAsync();
            var matchingCourses = courses.Where(c => 
                c.Department == student.Department && 
                c.Class == student.Class);

            // Her eşleşen derse öğrenciyi ata
            foreach (var course in matchingCourses)
            {
                await _courseService.AssignStudentToCourseAsync(course.CourseId, student.UserId);
            }
        }

        // Kullanıcı düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        // Kullanıcı düzenleme - POST
        [HttpPost]
        public async Task<IActionResult> EditUser(int userId, string fullName, string email, string? department, string? @class)
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                    return RedirectToAction("Users");
                }

                user.FullName = fullName;
                user.Email = email;
                
                if (user.Role == "Student")
                {
                    user.Department = department;
                    user.Class = @class;

                    // Ders atamalarını güncelle
                    await UpdateStudentCourseAssignmentsAsync(user);
                }

                await _userService.UpdateUserAsync(user);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            }

            var userForView = await _userService.GetUserByIdAsync(userId);
            return View(userForView);
        }

        // Öğrencinin ders atamalarını güncelle
        private async Task UpdateStudentCourseAssignmentsAsync(User student)
        {
            if (student.Role != "Student" || string.IsNullOrEmpty(student.Department) || string.IsNullOrEmpty(student.Class))
                return;

            // Mevcut ders atamalarını kaldır
            var currentCourses = await _courseService.GetCoursesByStudentIdAsync(student.UserId);
            foreach (var course in currentCourses)
            {
                await _courseService.RemoveStudentFromCourseAsync(course.CourseId, student.UserId);
            }

            // Yeni ders atamalarını yap
            await AssignStudentToCoursesAsync(student);
        }

        // Kullanıcı silme
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                    return RedirectToAction("Users");
                }

                // Admin kendisini silemez
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (user.UserId == currentUserId)
                {
                    TempData["ErrorMessage"] = "Kendi hesabınızı silemezsiniz!";
                    return RedirectToAction("Users");
                }

                await _userService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kullanıcı silinirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Users");
        }
    }
}



