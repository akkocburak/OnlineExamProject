using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface IExamStudentRepository
    {
        Task<IEnumerable<ExamStudent>> GetByExamIdAsync(int examId);
        Task<IEnumerable<ExamStudent>> GetByStudentIdAsync(int studentId);
        Task<ExamStudent?> GetByExamAndStudentAsync(int examId, int studentId);
        Task<ExamStudent> CreateAsync(ExamStudent examStudent);
        Task<bool> DeleteAsync(int examStudentId);
        Task<bool> DeleteByExamIdAsync(int examId);
        Task<bool> IsStudentAssignedToExamAsync(int examId, int studentId);
        Task<IEnumerable<User>> GetStudentsByExamIdAsync(int examId);
    }
}
