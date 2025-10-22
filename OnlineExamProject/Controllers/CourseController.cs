using Microsoft.AspNetCore.Mvc;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public CourseController(ICourseService courseService, IUserService userService)
        {
            _courseService = courseService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher") return RedirectToAction("Index", "Home");

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string courseName)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (!string.IsNullOrEmpty(courseName))
            {
                var course = new Course
                {
                    CourseName = courseName,
                    TeacherId = userId.Value,
                    CreatedAt = DateTime.Now
                };
                
                await _courseService.CreateCourseAsync(course);
                TempData["SuccessMessage"] = "Ders başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Ders adı boş olamaz!";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || course.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Course course)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Mevcut dersi getir
            var existingCourse = await _courseService.GetCourseByIdAsync(course.CourseId);
            if (existingCourse == null || existingCourse.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    // Sadece değiştirilebilir alanları güncelle
                    existingCourse.CourseName = course.CourseName;
                    
                    await _courseService.UpdateCourseAsync(existingCourse);
                    TempData["SuccessMessage"] = "Ders başarıyla güncellendi!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ders güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen tüm gerekli alanları doldurun.";
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || course.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            try
            {
                await _courseService.DeleteCourseAsync(id);
                TempData["SuccessMessage"] = "Ders başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ders silinirken bir hata oluştu. Bu dersle ilgili sınavlar veya veriler bulunduğu için silinemez!";
            }
            
            return RedirectToAction("Index");
        }
    }
}
