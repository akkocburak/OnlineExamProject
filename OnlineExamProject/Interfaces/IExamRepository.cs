using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface IExamRepository
    {
        Task<Exam?> GetByIdAsync(int id);
        Task<Exam?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Exam>> GetAllAsync();
        Task<IEnumerable<Exam>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Exam>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<Exam>> GetActiveExamsAsync();
        Task<IEnumerable<Exam>> GetExamsForStudentAsync(int studentId);
        Task<Exam> CreateAsync(Exam exam);
        Task<Exam> UpdateAsync(Exam exam);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExamExistsAsync(int id);
        Task<bool> IsExamActiveAsync(int examId);
        Task<bool> IsStudentEligibleForExamAsync(int examId, int studentId);
        Task<IEnumerable<Exam>> GetUpcomingExamsForStudentAsync(int studentId);
    }
}





