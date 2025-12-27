using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface IStudentExamRepository
    {
        Task<StudentExam?> GetByIdAsync(int id);
        Task<StudentExam?> GetByExamAndStudentAsync(int examId, int studentId);
        Task<IEnumerable<StudentExam>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<StudentExam>> GetByExamIdAsync(int examId);
        Task<StudentExam> CreateAsync(StudentExam studentExam);
        Task<StudentExam> UpdateAsync(StudentExam studentExam);
        Task<bool> DeleteAsync(int id);
        Task<bool> StudentExamExistsAsync(int examId, int studentId);
        Task<bool> HasStudentStartedExamAsync(int examId, int studentId);
        Task<bool> HasStudentCompletedExamAsync(int examId, int studentId);
        Task<int> CalculateScoreAsync(int studentExamId);
        Task<StudentExam?> StartExamAsync(int examId, int studentId);
        Task<bool> SubmitExamAsync(int studentExamId);
    }
}













