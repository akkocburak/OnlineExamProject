using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Services
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly IQuestionBankRepository _questionBankRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamRepository _examRepository;

        public QuestionBankService(
            IQuestionBankRepository questionBankRepository,
            IQuestionRepository questionRepository,
            IExamRepository examRepository)
        {
            _questionBankRepository = questionBankRepository;
            _questionRepository = questionRepository;
            _examRepository = examRepository;
        }

        public async Task<IEnumerable<QuestionBank>> GetByTeacherIdAsync(int teacherId)
        {
            return await _questionBankRepository.GetByTeacherIdAsync(teacherId);
        }

        public async Task<IEnumerable<QuestionBank>> GetByCourseIdAsync(int courseId)
        {
            return await _questionBankRepository.GetByCourseIdAsync(courseId);
        }

        public async Task<IEnumerable<QuestionBank>> GetByTeacherAndCourseAsync(int teacherId, int courseId)
        {
            return await _questionBankRepository.GetByTeacherAndCourseAsync(teacherId, courseId);
        }

        public async Task<QuestionBank?> GetByIdAsync(int id)
        {
            return await _questionBankRepository.GetByIdAsync(id);
        }

        public async Task<QuestionBank> CreateAsync(QuestionBank questionBank)
        {
            return await _questionBankRepository.CreateAsync(questionBank);
        }

        public async Task<QuestionBank> UpdateAsync(QuestionBank questionBank)
        {
            return await _questionBankRepository.UpdateAsync(questionBank);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _questionBankRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<QuestionBank>> SearchAsync(int teacherId, string? searchTerm, string? difficulty, int? courseId)
        {
            return await _questionBankRepository.SearchAsync(teacherId, searchTerm, difficulty, courseId);
        }

        public async Task<bool> AddToExamAsync(int questionBankId, int examId)
        {
            return await _questionBankRepository.AddToExamAsync(questionBankId, examId);
        }

        public async Task<bool> SaveToQuestionBankAsync(int questionId, int teacherId)
        {
            try
            {
                var question = await _questionRepository.GetByIdAsync(questionId);
                if (question == null) return false;

                var exam = await _examRepository.GetByIdAsync(question.ExamId);
                if (exam == null || exam.TeacherId != teacherId) return false;

                var questionBank = new QuestionBank
                {
                    TeacherId = teacherId,
                    CourseId = exam.CourseId,
                    QuestionText = question.QuestionText,
                    ImagePath = question.ImagePath,
                    OptionCount = question.OptionCount,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD,
                    OptionE = question.OptionE,
                    CorrectOption = question.CorrectOption,
                    Points = 1, // Varsayılan puan (Question modelinde Points yok, QuestionBank'te var)
                    CreatedAt = DateTime.Now
                };

                await _questionBankRepository.CreateAsync(questionBank);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
