using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface IStudentAnswerRepository
    {
        Task<StudentAnswer?> GetByIdAsync(int id);
        Task<IEnumerable<StudentAnswer>> GetByStudentExamIdAsync(int studentExamId);
        Task<StudentAnswer?> GetByStudentExamAndQuestionAsync(int studentExamId, int questionId);
        Task<StudentAnswer> CreateAsync(StudentAnswer studentAnswer);
        Task<StudentAnswer> UpdateAsync(StudentAnswer studentAnswer);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByStudentExamIdAsync(int studentExamId);
        Task<bool> AnswerExistsAsync(int studentExamId, int questionId);
        Task<bool> SaveAnswerAsync(int studentExamId, int questionId, char selectedOption);
        Task<int> GetCorrectAnswerCountAsync(int studentExamId);
    }
}










