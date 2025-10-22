using Microsoft.AspNetCore.Mvc;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Controllers
{
    public class QuestionBankController : Controller
    {
        private readonly IQuestionBankService _questionBankService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public QuestionBankController(IQuestionBankService questionBankService, ICourseService courseService, IUserService userService)
        {
            _questionBankService = questionBankService;
            _courseService = courseService;
            _userService = userService;
        }

        // Soru bankası listesi
        public async Task<IActionResult> Index(string? searchTerm, string? difficulty, int? courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher") return RedirectToAction("Index", "Home");

            var questions = await _questionBankService.SearchAsync(userId.Value, searchTerm, difficulty, courseId);
            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);

            ViewBag.Courses = courses;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Difficulty = difficulty;
            ViewBag.CourseId = courseId;

            return View(questions);
        }

        // Yeni soru ekleme
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuestionBank model, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                // Resim yükleme işlemi
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "questions");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"qb_{DateTime.Now.Ticks}_{Path.GetFileName(imageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    model.ImagePath = $"/images/questions/{fileName}";
                }

                model.TeacherId = userId.Value;
                model.CreatedAt = DateTime.Now;

                await _questionBankService.CreateAsync(model);
                TempData["SuccessMessage"] = "Soru başarıyla soru bankasına eklendi!";
                return RedirectToAction("Index");
            }

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View(model);
        }

        // Soru düzenleme
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var question = await _questionBankService.GetByIdAsync(id);
            if (question == null || question.TeacherId != userId.Value)
                return NotFound();

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(QuestionBank model, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Mevcut soruyu getir
            var existingQuestion = await _questionBankService.GetByIdAsync(model.QuestionBankId);
            if (existingQuestion == null || existingQuestion.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    // Sadece değiştirilebilir alanları güncelle
                    existingQuestion.QuestionText = model.QuestionText;
                    existingQuestion.OptionA = model.OptionA;
                    existingQuestion.OptionB = model.OptionB;
                    existingQuestion.OptionC = model.OptionC;
                    existingQuestion.OptionD = model.OptionD;
                    existingQuestion.OptionE = model.OptionE;
                    existingQuestion.OptionCount = model.OptionCount;
                    existingQuestion.CorrectOption = model.CorrectOption;
                    existingQuestion.Points = model.Points;
                    existingQuestion.CourseId = model.CourseId;

                    // Opsiyonel resim güncelleme
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "questions");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var fileName = $"qb_{DateTime.Now.Ticks}_{Path.GetFileName(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingQuestion.ImagePath = $"/images/questions/{fileName}";
                    }

                    await _questionBankService.UpdateAsync(existingQuestion);
                    TempData["SuccessMessage"] = "Soru başarıyla güncellendi!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Soru güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen tüm gerekli alanları doldurun.";
            }

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View(model);
        }

        // Soru silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var question = await _questionBankService.GetByIdAsync(id);
            if (question == null || question.TeacherId != userId.Value)
                return NotFound();

            try
            {
                await _questionBankService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Soru başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Soru silinirken bir hata oluştu. Bu soruyla ilgili veriler bulunduğu için silinemez!";
            }
            
            return RedirectToAction("Index");
        }

        // Sınavdan soru bankasına kaydetme
        [HttpPost]
        public async Task<IActionResult> SaveToQuestionBank(int questionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var success = await _questionBankService.SaveToQuestionBankAsync(questionId, userId.Value);
            return Json(new { success = success, message = success ? "Soru soru bankasına kaydedildi!" : "Soru kaydedilemedi!" });
        }

        // Soru bankasından sınava ekleme
        [HttpPost]
        public async Task<IActionResult> AddToExam(int questionBankId, int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var success = await _questionBankService.AddToExamAsync(questionBankId, examId);
            return Json(new { success = success, message = success ? "Soru sınava eklendi!" : "Soru eklenemedi!" });
        }
    }
}
