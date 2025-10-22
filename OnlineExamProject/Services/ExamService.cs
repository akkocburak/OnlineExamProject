using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IStudentExamRepository _studentExamRepository;
        private readonly IStudentAnswerRepository _studentAnswerRepository;
        private readonly IExamStudentRepository _examStudentRepository;

        public ExamService(
            IExamRepository examRepository,
            IQuestionRepository questionRepository,
            IStudentExamRepository studentExamRepository,
            IStudentAnswerRepository studentAnswerRepository,
            IExamStudentRepository examStudentRepository)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _studentExamRepository = studentExamRepository;
            _studentAnswerRepository = studentAnswerRepository;
            _examStudentRepository = examStudentRepository;
        }

        public async Task<IEnumerable<Exam>> GetActiveExamsAsync()
        {
            return await _examRepository.GetActiveExamsAsync();
        }

        public async Task<IEnumerable<Exam>> GetExamsForTeacherAsync(int teacherId)
        {
            return await _examRepository.GetByTeacherIdAsync(teacherId);
        }

        public async Task<IEnumerable<Exam>> GetExamsForStudentAsync(int studentId)
        {
            return await _examRepository.GetExamsForStudentAsync(studentId);
        }

        public async Task<Exam?> GetExamByIdAsync(int id, bool includeDetails = false)
        {
            if (includeDetails)
            {
                return await _examRepository.GetByIdWithDetailsAsync(id);
            }
            return await _examRepository.GetByIdAsync(id);
        }

        public async Task<Exam> CreateExamAsync(Exam exam)
        {
            return await _examRepository.CreateAsync(exam);
        }

        public async Task<Exam> UpdateExamAsync(Exam exam)
        {
            return await _examRepository.UpdateAsync(exam);
        }

        public async Task<bool> DeleteExamAsync(int id)
        {
            // Önce sınavın sorularını sil
            await _questionRepository.DeleteByExamIdAsync(id);
            
            // Sonra sınavı sil
            return await _examRepository.DeleteAsync(id);
        }

        public async Task<bool> IsExamActiveAsync(int examId)
        {
            return await _examRepository.IsExamActiveAsync(examId);
        }

        public async Task<bool> CanStudentTakeExamAsync(int examId, int studentId)
        {
            return await _examRepository.IsStudentEligibleForExamAsync(examId, studentId);
        }

        public async Task<Exam?> StartExamForStudentAsync(int examId, int studentId)
        {
            if (!await CanStudentTakeExamAsync(examId, studentId))
            {
                return null;
            }

            var studentExam = await _studentExamRepository.StartExamAsync(examId, studentId);
            if (studentExam == null) return null;

            return await _examRepository.GetByIdAsync(examId);
        }

        public async Task<IEnumerable<StudentExam>> GetExamResultsAsync(int examId)
        {
            return await _studentExamRepository.GetByExamIdAsync(examId);
        }

        public async Task<StudentExam?> GetStudentExamResultAsync(int examId, int studentId)
        {
            return await _studentExamRepository.GetByExamAndStudentAsync(examId, studentId);
        }

        public async Task<bool> SubmitStudentExamAsync(int studentExamId)
        {
            return await _studentExamRepository.SubmitExamAsync(studentExamId);
        }

        public async Task<bool> SaveStudentAnswerAsync(int studentExamId, int questionId, char selectedOption)
        {
            return await _studentAnswerRepository.SaveAnswerAsync(studentExamId, questionId, selectedOption);
        }

        public async Task<IEnumerable<Question>> GetExamQuestionsAsync(int examId)
        {
            return await _questionRepository.GetByExamIdAsync(examId);
        }

        public async Task<Question?> GetQuestionByIdAsync(int questionId)
        {
            return await _questionRepository.GetByIdAsync(questionId);
        }

        public async Task<Question> AddQuestionAsync(Question question)
        {
            return await _questionRepository.CreateAsync(question);
        }

        public async Task<Question> UpdateQuestionAsync(Question question)
        {
            return await _questionRepository.UpdateAsync(question);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            return await _questionRepository.DeleteAsync(questionId);
        }

        public async Task<bool> AssignStudentsToExamAsync(int examId, List<int> studentIds)
        {
            try
            {
                // Önce mevcut atamaları sil
                await _examStudentRepository.DeleteByExamIdAsync(examId);

                // Yeni atamaları ekle
                foreach (var studentId in studentIds)
                {
                    var examStudent = new ExamStudent
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        AssignedAt = DateTime.Now
                    };
                    await _examStudentRepository.CreateAsync(examStudent);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetStudentsForExamAssignmentAsync(int courseId)
        {
            // Bu metod CourseService'den öğrencileri getirecek
            // Şimdilik boş liste döndürüyoruz, CourseService implementasyonu gerekli
            return new List<User>();
        }

        public async Task<IEnumerable<Exam>> GetUpcomingExamsForStudentAsync(int studentId)
        {
            return await _examRepository.GetUpcomingExamsForStudentAsync(studentId);
        }

        public async Task<IEnumerable<Question>> GetQuestionsByCourseIdAsync(int courseId)
        {
            return await _questionRepository.GetByCourseIdAsync(courseId);
        }

        public async Task<IEnumerable<User>> GetAssignedStudentsAsync(int examId)
        {
            return await _examStudentRepository.GetStudentsByExamIdAsync(examId);
        }
    }
}

