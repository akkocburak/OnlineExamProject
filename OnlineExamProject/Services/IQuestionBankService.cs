using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public interface IQuestionBankService
    {
        Task<IEnumerable<QuestionBank>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<QuestionBank>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<QuestionBank>> GetByTeacherAndCourseAsync(int teacherId, int courseId);
        Task<QuestionBank?> GetByIdAsync(int id);
        Task<QuestionBank> CreateAsync(QuestionBank questionBank);
        Task<QuestionBank> UpdateAsync(QuestionBank questionBank);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<QuestionBank>> SearchAsync(int teacherId, string? searchTerm, string? difficulty, int? courseId);
        Task<bool> AddToExamAsync(int questionBankId, int examId);
        Task<bool> SaveToQuestionBankAsync(int questionId, int teacherId);
    }
}
