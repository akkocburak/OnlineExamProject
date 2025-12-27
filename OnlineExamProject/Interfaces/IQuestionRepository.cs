using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetByExamIdAsync(int examId);
        Task<Question> CreateAsync(Question question);
        Task<Question> UpdateAsync(Question question);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByExamIdAsync(int examId);
        Task<bool> QuestionExistsAsync(int id);
        Task<int> GetQuestionCountByExamIdAsync(int examId);
        Task<IEnumerable<Question>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Question>> GetByCourseIdAndExamTypeAsync(int courseId, string? examType);
    }
}





