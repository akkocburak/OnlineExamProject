using Microsoft.AspNetCore.Mvc;
using OnlineExamProject.Models;
using OnlineExamProject.Services;
using OnlineExamProject.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace OnlineExamProject.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly IStudentExamRepository _studentExamRepository;
        private readonly IStudentAnswerRepository _studentAnswerRepository;
        private readonly IExamStudentRepository _examStudentRepository;
        private readonly IQuestionBankService _questionBankService;

        public ExamController(IExamService examService, ICourseService courseService, IUserService userService, IStudentExamRepository studentExamRepository, IStudentAnswerRepository studentAnswerRepository, IExamStudentRepository examStudentRepository, IQuestionBankService questionBankService)
        {
            _examService = examService;
            _courseService = courseService;
            _userService = userService;
            _studentExamRepository = studentExamRepository;
            _studentAnswerRepository = studentAnswerRepository;
            _examStudentRepository = examStudentRepository;
            _questionBankService = questionBankService;
        }

        // Öğretmen için sınav listesi
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher") return RedirectToAction("Index", "Home");

            var exams = await _examService.GetExamsForTeacherAsync(userId.Value);
            return View(exams);
        }

        // Öğrenci için aktif sınavlar
        public async Task<IActionResult> ActiveExams()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            var exams = await _examService.GetExamsForStudentAsync(userId.Value);
            return View(exams);
        }

        // Sınav detayları
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(id, true);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Öğretmen ise sınav sonuçlarını da getir
            if (user.Role == "Teacher")
            {
                ViewBag.Results = await _examService.GetExamResultsAsync(id);
            }

            return View(exam);
        }

        // Yeni sınav oluşturma
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher") return RedirectToAction("Index", "Home");

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string examTitle, int courseId, DateTime startTime, DateTime endTime, bool allowBackNavigation, string? examType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (!string.IsNullOrEmpty(examTitle) && courseId > 0)
            {
                var exam = new Exam
                {
                    ExamTitle = examTitle,
                    CourseId = courseId,
                    TeacherId = userId.Value,
                    StartTime = startTime,
                    EndTime = endTime,
                    AllowBackNavigation = allowBackNavigation,
                    ExamType = examType,
                    CreatedAt = DateTime.Now
                };
                
                await _examService.CreateExamAsync(exam);
                TempData["SuccessMessage"] = "Sınav başarıyla oluşturuldu! Şimdi öğrencileri atayabilirsiniz.";
                return RedirectToAction("AssignStudents", new { examId = exam.ExamId });
            }

            TempData["ErrorMessage"] = "Tüm alanları doldurun!";
            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            return View();
        }

        // Soru ekleme sayfası
        [HttpGet]
        public async Task<IActionResult> AddQuestions(int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(examId, true);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            var questions = await _examService.GetExamQuestionsAsync(examId);

            // Aynı derse ait ve aynı sınav tipine sahip önceki soruları göster
            var reusableQuestions = await _examService.GetQuestionsByCourseIdAndExamTypeAsync(exam.CourseId, exam.ExamType);

            ViewBag.ExamId = examId;
            ViewBag.ExamTitle = exam.ExamTitle;
            ViewBag.ExamType = exam.ExamType;
            ViewBag.ReusableQuestions = reusableQuestions;
            ViewBag.Exam = exam; // Sınav bilgisini ViewBag'e ekle
            ViewBag.CanEdit = DateTime.Now < exam.StartTime; // Sınav başlamadıysa düzenlenebilir
            return View(questions);
        }

        // Soru ekleme
        [HttpPost]
        public async Task<IActionResult> AddQuestion(int examId, string questionText, int optionCount, 
            string optionA, string optionB, string optionC, string optionD, string? optionE, char correctOption, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(examId);
            if (exam == null || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            string? imagePath = null;
            
            // Resim yükleme işlemi
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "questions");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{examId}_{DateTime.Now.Ticks}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                
                imagePath = $"/images/questions/{fileName}";
            }

            var question = new Question
            {
                ExamId = examId,
                QuestionText = questionText,
                ImagePath = imagePath,
                OptionCount = optionCount,
                OptionA = optionA,
                OptionB = optionB,
                OptionC = optionC,
                OptionD = optionD,
                OptionE = optionE,
                CorrectOption = correctOption,
                CoursesID = exam.CourseId
            };

            await _examService.AddQuestionAsync(question);
            TempData["SuccessMessage"] = "Soru başarıyla eklendi!";
            return RedirectToAction("AddQuestions", new { examId = examId });
        }

        // Soru silme
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var question = await _examService.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound();

            var exam = await _examService.GetExamByIdAsync(question.ExamId);
            if (exam == null || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            try
            {
                // Sınavın başlayıp başlamadığını kontrol et
                if (DateTime.Now >= exam.StartTime)
                {
                    TempData["ErrorMessage"] = "Bu sınav başladığı için soru silinemez!";
                    return RedirectToAction("AddQuestions", new { examId = question.ExamId });
                }

                await _examService.DeleteQuestionAsync(questionId);
                TempData["SuccessMessage"] = "Soru başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Soru silinirken bir hata oluştu. Bu soruyla ilgili veriler bulunduğu için silinemez!";
            }
            
            return RedirectToAction("AddQuestions", new { examId = question.ExamId });
        }

        // Soru düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var question = await _examService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound();

            var exam = await _examService.GetExamByIdAsync(question.ExamId);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            return View(question);
        }

        // Soru düzenleme - POST
        [HttpPost]
        public async Task<IActionResult> EditQuestion(Question model, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Mevcut soruyu getir
            var existingQuestion = await _examService.GetQuestionByIdAsync(model.QuestionId);
            if (existingQuestion == null) return NotFound();

            var exam = await _examService.GetExamByIdAsync(existingQuestion.ExamId);
            if (exam == null || exam.TeacherId != userId.Value)
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

                    // Opsiyonel resim güncelleme
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "questions");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var fileName = $"{existingQuestion.ExamId}_{DateTime.Now.Ticks}_{Path.GetFileName(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingQuestion.ImagePath = $"/images/questions/{fileName}";
                    }

                    await _examService.UpdateQuestionAsync(existingQuestion);
                    TempData["SuccessMessage"] = "Soru başarıyla güncellendi!";
                    return RedirectToAction("AddQuestions", new { examId = existingQuestion.ExamId });
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

            return View(model);
        }

        // Sınav düzenleme
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(id, true); // Soruları da getir
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            // Sınavın başlayıp başlamadığını kontrol et
            if (DateTime.Now >= exam.StartTime)
            {
                TempData["ErrorMessage"] = "Bu sınav başladığı için düzenlenemez!";
                return RedirectToAction("Index");
            }

            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;

            // Atanmış öğrencileri getir
            var assignedStudents = await _examService.GetAssignedStudentsAsync(id);
            ViewBag.AssignedStudents = assignedStudents;

            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Exam exam)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Mevcut sınavı getir
            var existingExam = await _examService.GetExamByIdAsync(exam.ExamId);
            if (existingExam == null || existingExam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            // Sınavın başlayıp başlamadığını kontrol et
            if (DateTime.Now >= existingExam.StartTime)
            {
                TempData["ErrorMessage"] = "Bu sınav başladığı için düzenlenemez!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Sadece değiştirilebilir alanları güncelle
                    existingExam.ExamTitle = exam.ExamTitle;
                    existingExam.CourseId = exam.CourseId;
                    existingExam.StartTime = exam.StartTime;
                    existingExam.EndTime = exam.EndTime;
                    existingExam.AllowBackNavigation = exam.AllowBackNavigation;

                    await _examService.UpdateExamAsync(existingExam);
                    TempData["SuccessMessage"] = "Sınav başarıyla güncellendi!";
                    return RedirectToAction("Edit", new { id = exam.ExamId }); // Aynı sayfaya geri dön
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sınav güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen tüm gerekli alanları doldurun.";
            }

            // Hata durumunda sayfayı yeniden yükle
            var courses = await _courseService.GetCoursesByTeacherIdAsync(userId.Value);
            ViewBag.Courses = courses;
            var assignedStudents = await _examService.GetAssignedStudentsAsync(exam.ExamId);
            ViewBag.AssignedStudents = assignedStudents;
            
            // Sınavı tekrar getir (sorularla birlikte)
            var examWithDetails = await _examService.GetExamByIdAsync(exam.ExamId, true);
            return View(examWithDetails);
        }

        // Sınav silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            try
            {
                // Sınavın bitip bitmediğini kontrol et
                if (DateTime.Now > exam.EndTime)
                {
                    TempData["ErrorMessage"] = "Bu sınav tamamlandığı için silinemez!";
                    return RedirectToAction("Index");
                }

                // Sınavın başlayıp başlamadığını kontrol et
                if (DateTime.Now >= exam.StartTime)
                {
                    TempData["ErrorMessage"] = "Bu sınav başladığı için silinemez!";
                    return RedirectToAction("Index");
                }

                await _examService.DeleteExamAsync(id);
                TempData["SuccessMessage"] = "Sınav başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sınav silinirken bir hata oluştu. Bu sınavla ilgili veriler bulunduğu için silinemez!";
            }
            
            return RedirectToAction("Index");
        }

        // Sınava başlama (öğrenci) - GET
        [HttpGet]
        public async Task<IActionResult> StartExam(int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            var exam = await _examService.StartExamForStudentAsync(examId, userId.Value);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Bu sınava katılamazsınız!";
                return RedirectToAction("ActiveExams");
            }

            return RedirectToAction("TakeExam", new { id = examId });
        }

        // Sınava başlama (öğrenci) - POST
        [HttpPost]
        public async Task<IActionResult> StartExamPost(int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            var exam = await _examService.StartExamForStudentAsync(examId, userId.Value);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Bu sınava katılamazsınız!";
                return RedirectToAction("ActiveExams");
            }

            return RedirectToAction("TakeExam", new { id = examId });
        }

        // Sınav alma sayfası (öğrenci)
        public async Task<IActionResult> TakeExam(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            var exam = await _examService.GetExamByIdAsync(id, true);
            if (exam == null) return NotFound();

            if (!await _examService.CanStudentTakeExamAsync(id, userId.Value))
            {
                TempData["ErrorMessage"] = "Bu sınava katılamazsınız!";
                return RedirectToAction("ActiveExams");
            }

            var questions = await _examService.GetExamQuestionsAsync(id);
            // Model.Questions'in her zaman dolu olmasını garanti et
            if (questions != null)
            {
                exam.Questions = questions.ToList();
            }
            
            // StudentExam kaydını garanti et
            var studentExam = await _studentExamRepository.GetByExamAndStudentAsync(id, userId.Value);
            if (studentExam == null)
            {
                studentExam = await _studentExamRepository.StartExamAsync(id, userId.Value);
            }
            ViewBag.StudentExamId = studentExam?.StudentExamId;

            return View(exam);
        }

        // Cevap kaydetme
        public class SaveAnswerDto { public int studentExamId {get;set;} public int questionId {get;set;} public char selectedOption {get;set;} }
        // Reuse/Bank ekleme için DTO'lar
        public class AddSingleDto { public int examId { get; set; } public int questionId { get; set; } }
        public class AddBulkDto { public int examId { get; set; } public List<int> questionIds { get; set; } = new(); }
        public class AddBankBulkDto { public int examId { get; set; } public List<int> questionBankIds { get; set; } = new(); }

        [HttpPost]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var success = await _examService.SaveStudentAnswerAsync(dto.studentExamId, dto.questionId, dto.selectedOption);
            return Json(new { success = success });
        }

        // Sınavı tamamlama
        [HttpPost]
        public async Task<IActionResult> SubmitExam(int studentExamId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Son puanı doğru hesaplamak için submit öncesi cevapları ve soru sayısını kontrol et
            var success = await _examService.SubmitStudentExamAsync(studentExamId);
            if (success)
            {
                TempData["SuccessMessage"] = "Sınav başarıyla tamamlandı!";
                return RedirectToAction("MyResults");
            }

            TempData["ErrorMessage"] = "Sınav tamamlanırken bir hata oluştu!";
            return RedirectToAction("ActiveExams");
        }

        // Öğrenci cevaplarını görüntüle (öğretmen)
        [HttpGet]
        public async Task<IActionResult> ViewAnswers(int studentExamId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var studentExam = await _studentExamRepository.GetByIdAsync(studentExamId);
            if (studentExam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || studentExam.Exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            var answers = await _studentAnswerRepository.GetByStudentExamIdAsync(studentExamId);
            ViewBag.Answers = answers;
            return View(studentExam);
        }

        // Sınav sonuçlarını Excel formatında indir (öğretmen)
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(examId, true);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            // Sınav sonuçlarını getir
            var results = await _examService.GetExamResultsAsync(examId);
            var questions = await _examService.GetExamQuestionsAsync(examId);
            var orderedQuestions = questions.OrderBy(q => q.QuestionId).ToList();

            // EPPlus lisans ayarı (non-commercial kullanım için)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sınav Sonuçları");

                // Başlık satırı
                worksheet.Cells[1, 1].Value = "Öğrenci Adı";
                worksheet.Cells[1, 2].Value = "Puan";
                worksheet.Cells[1, 3].Value = "Başlangıç Tarihi";
                worksheet.Cells[1, 4].Value = "Bitiş Tarihi";
                worksheet.Cells[1, 5].Value = "Durum";

                // Soru başlıkları
                int colIndex = 6;
                foreach (var question in orderedQuestions)
                {
                    worksheet.Cells[1, colIndex].Value = $"Soru {question.QuestionId}";
                    colIndex++;
                }

                // Başlık satırını formatla
                using (var range = worksheet.Cells[1, 1, 1, colIndex - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(OfficeOpenXml.Style.ExcelIndexedColor.Indexed45);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Veri satırları
                int rowIndex = 2;
                foreach (var result in results)
                {
                    worksheet.Cells[rowIndex, 1].Value = result.Student.FullName;
                    worksheet.Cells[rowIndex, 2].Value = result.Score.HasValue ? result.Score.Value : 0;
                    worksheet.Cells[rowIndex, 3].Value = result.StartedAt?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                    worksheet.Cells[rowIndex, 4].Value = result.FinishedAt?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                    worksheet.Cells[rowIndex, 5].Value = result.Completed ? "Tamamlandı" : "Devam Ediyor";

                    // Öğrencinin cevaplarını getir
                    var answers = await _studentAnswerRepository.GetByStudentExamIdAsync(result.StudentExamId);
                    // Aynı soruya birden fazla cevap varsa, en son verilen cevabı al (en yüksek AnswerId)
                    var answerDict = answers
                        .GroupBy(a => a.QuestionId)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AnswerId).First());

                    // Her soru için cevabı yaz
                    colIndex = 6;
                    foreach (var question in orderedQuestions)
                    {
                        if (answerDict.ContainsKey(question.QuestionId))
                        {
                            var answer = answerDict[question.QuestionId];
                            var cellValue = $"{answer.SelectedOption} ({GetOptionText(question, answer.SelectedOption)})";
                            if (!answer.IsCorrect)
                            {
                                cellValue += $" - Doğru: {question.CorrectOption}";
                            }
                            worksheet.Cells[rowIndex, colIndex].Value = cellValue;
                            
                            // Doğru cevap ise yeşil, yanlış ise kırmızı
                            if (answer.IsCorrect)
                            {
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(OfficeOpenXml.Style.ExcelIndexedColor.Indexed43);
                            }
                            else
                            {
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(OfficeOpenXml.Style.ExcelIndexedColor.Indexed38);
                            }
                        }
                        else
                        {
                            worksheet.Cells[rowIndex, colIndex].Value = "Cevaplanmadı";
                            worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(OfficeOpenXml.Style.ExcelIndexedColor.Indexed22);
                        }
                        colIndex++;
                    }

                    rowIndex++;
                }

                // Sütun genişliklerini ayarla
                worksheet.Cells.AutoFitColumns();

                // Dosya adı
                var fileName = $"{exam.ExamTitle.Replace(" ", "_")}_Sonuclar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Seçenek metnini getir
        private string GetOptionText(Question question, char option)
        {
            return option switch
            {
                'A' => question.OptionA,
                'B' => question.OptionB,
                'C' => question.OptionC,
                'D' => question.OptionD,
                'E' => question.OptionE ?? "",
                _ => ""
            };
        }

        // Öğrenci sonuçları
        public async Task<IActionResult> MyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            // Öğrencinin tüm sınav sonuçlarını getir
            var results = await _studentExamRepository.GetByStudentIdAsync(userId.Value);
            ViewBag.Results = results;
            return View();
        }

        // Öğrenci atama sayfası
        [HttpGet]
        public async Task<IActionResult> AssignStudents(int examId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(examId);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            // Tüm bölümleri getir
            var departments = await _userService.GetAllDepartmentsAsync();
            ViewBag.ExamId = examId;
            ViewBag.ExamTitle = exam.ExamTitle;
            ViewBag.CourseId = exam.CourseId;
            ViewBag.Departments = departments;
            return View(new List<User>()); // İlk yüklemede boş liste
        }

        // AJAX: Bölüme göre sınıfları getir
        [HttpGet]
        public async Task<IActionResult> GetClassesByDepartment(string department)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            var classes = await _userService.GetClassesByDepartmentAsync(department);
            return Json(new { success = true, classes = classes });
        }

        // AJAX: Bölüm ve sınıfa göre öğrencileri getir
        [HttpGet]
        public async Task<IActionResult> GetStudentsByDepartmentAndClass(string department, string @class)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            var students = await _userService.GetStudentsByDepartmentAndClassAsync(department, @class);
            var studentList = students.Select(s => new { 
                userId = s.UserId, 
                fullName = s.FullName, 
                email = s.Email 
            }).ToList();
            return Json(new { success = true, students = studentList });
        }

        // Öğrenci atama işlemi
        [HttpPost]
        public async Task<IActionResult> AssignStudents(int examId, List<int> selectedStudents)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exam = await _examService.GetExamByIdAsync(examId);
            if (exam == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Teacher" || exam.TeacherId != userId.Value)
                return RedirectToAction("Index", "Home");

            if (selectedStudents != null && selectedStudents.Any())
            {
                var success = await _examService.AssignStudentsToExamAsync(examId, selectedStudents);
                if (success)
                {
                    TempData["SuccessMessage"] = "Öğrenciler başarıyla atandı! Şimdi soruları ekleyebilirsiniz.";
                    return RedirectToAction("AddQuestions", new { examId = examId });
                }
            }

            TempData["ErrorMessage"] = "En az bir öğrenci seçmelisiniz!";
            var departments = await _userService.GetAllDepartmentsAsync();
            ViewBag.ExamId = examId;
            ViewBag.ExamTitle = exam.ExamTitle;
            ViewBag.CourseId = exam.CourseId;
            ViewBag.Departments = departments;
            return View(new List<User>());
        }

        // Beklenen sınavlar (öğrenci)
        public async Task<IActionResult> UpcomingExams()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.Role != "Student") return RedirectToAction("Index", "Home");

            var exams = await _examService.GetUpcomingExamsForStudentAsync(userId.Value);
            return View(exams);
        }

        // Soru bankasından sınava ekleme
        [HttpPost]
        public async Task<IActionResult> AddFromQuestionBank(int examId, int questionBankId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var exam = await _examService.GetExamByIdAsync(examId);
            if (exam == null || exam.TeacherId != userId.Value)
                return Json(new { success = false, message = "Sınav bulunamadı!" });

            var success = await _questionBankService.AddToExamAsync(questionBankId, examId);
            return Json(new { success = success, message = success ? "Soru sınava eklendi!" : "Soru eklenemedi!" });
        }

        // Soru bankasından çoklu ekleme
        [HttpPost]
        public async Task<IActionResult> AddFromQuestionBankBulk([FromBody] AddBankBulkDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var exam = await _examService.GetExamByIdAsync(dto.examId);
            if (exam == null || exam.TeacherId != userId.Value)
                return Json(new { success = false, message = "Sınav bulunamadı!" });

            if (dto.questionBankIds == null || dto.questionBankIds.Count == 0)
                return Json(new { success = false, message = "Lütfen en az bir soru seçin." });

            var addedCount = 0;
            foreach (var qbId in dto.questionBankIds)
            {
                var ok = await _questionBankService.AddToExamAsync(qbId, dto.examId);
                if (ok) addedCount++;
            }

            var anySuccess = addedCount > 0;
            return Json(new { success = anySuccess, message = anySuccess ? $"{addedCount} soru eklendi." : "Soru eklenemedi!" });
        }

        // Mevcut sorulardan tekli ekleme (soru bankası = önceki sorular)
        [HttpPost]
        public async Task<IActionResult> AddFromExistingQuestion([FromBody] AddSingleDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var exam = await _examService.GetExamByIdAsync(dto.examId);
            if (exam == null || exam.TeacherId != userId.Value)
                return Json(new { success = false, message = "Sınav bulunamadı!" });

            var source = await _examService.GetQuestionByIdAsync(dto.questionId);
            if (source == null) return Json(new { success = false, message = "Soru bulunamadı!" });

            // Aynı soru metninin bu sınava daha önce eklenip eklenmediğini kontrol et
            var existingQuestions = await _examService.GetExamQuestionsAsync(dto.examId);
            if (existingQuestions.Any(q => q.QuestionText == source.QuestionText))
            {
                return Json(new { success = false, message = "Bu soru zaten sınava eklenmiş!" });
            }

            var newQuestion = new Question
            {
                ExamId = dto.examId,
                QuestionText = source.QuestionText,
                ImagePath = source.ImagePath,
                OptionCount = source.OptionCount,
                OptionA = source.OptionA,
                OptionB = source.OptionB,
                OptionC = source.OptionC,
                OptionD = source.OptionD,
                OptionE = source.OptionE,
                CorrectOption = source.CorrectOption,
                CoursesID = exam.CourseId
            };

            await _examService.AddQuestionAsync(newQuestion);
            return Json(new { success = true, message = "Soru sınava eklendi!" });
        }

        // Mevcut sorulardan çoklu ekleme
        [HttpPost]
        public async Task<IActionResult> AddFromExistingQuestionBulk([FromBody] AddBulkDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var exam = await _examService.GetExamByIdAsync(dto.examId);
            if (exam == null || exam.TeacherId != userId.Value)
                return Json(new { success = false, message = "Sınav bulunamadı!" });

            if (dto.questionIds == null || dto.questionIds.Count == 0)
                return Json(new { success = false, message = "Lütfen en az bir soru seçin." });

            // Mevcut soruları al
            var existingQuestions = await _examService.GetExamQuestionsAsync(dto.examId);
            var existingQuestionTexts = existingQuestions.Select(q => q.QuestionText).ToHashSet();

            var added = 0;
            var skipped = 0;
            foreach (var qid in dto.questionIds)
            {
                var source = await _examService.GetQuestionByIdAsync(qid);
                if (source == null) continue;

                // Aynı soru metninin bu sınava daha önce eklenip eklenmediğini kontrol et
                if (existingQuestionTexts.Contains(source.QuestionText))
                {
                    skipped++;
                    continue;
                }

                var newQuestion = new Question
                {
                    ExamId = dto.examId,
                    QuestionText = source.QuestionText,
                    ImagePath = source.ImagePath,
                    OptionCount = source.OptionCount,
                    OptionA = source.OptionA,
                    OptionB = source.OptionB,
                    OptionC = source.OptionC,
                    OptionD = source.OptionD,
                    OptionE = source.OptionE,
                    CorrectOption = source.CorrectOption,
                    CoursesID = exam.CourseId
                };

                await _examService.AddQuestionAsync(newQuestion);
                existingQuestionTexts.Add(source.QuestionText); // Eklenen soruyu set'e ekle
                added++;
            }

            var message = added > 0 
                ? $"{added} soru eklendi." + (skipped > 0 ? $" {skipped} soru zaten mevcut olduğu için atlandı." : "")
                : "Soru eklenemedi!";
            return Json(new { success = added > 0, message = message });
        }

        // Sınav sorusunu soru bankasına kaydetme
        [HttpPost]
        public async Task<IActionResult> SaveToQuestionBank(int questionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi dolmuş!" });

            var success = await _questionBankService.SaveToQuestionBankAsync(questionId, userId.Value);
            return Json(new { success = success, message = success ? "Soru soru bankasına kaydedildi!" : "Soru kaydedilemedi!" });
        }
    }
}
