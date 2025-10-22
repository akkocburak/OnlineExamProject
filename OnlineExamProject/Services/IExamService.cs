using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public interface IExamService
    {
        Task<IEnumerable<Exam>> GetActiveExamsAsync();
        Task<IEnumerable<Exam>> GetExamsForTeacherAsync(int teacherId);
        Task<IEnumerable<Exam>> GetExamsForStudentAsync(int studentId);
        Task<Exam?> GetExamByIdAsync(int id, bool includeDetails = false);
        Task<Exam> CreateExamAsync(Exam exam);
        Task<Exam> UpdateExamAsync(Exam exam);
        Task<bool> DeleteExamAsync(int id);
        Task<bool> IsExamActiveAsync(int examId);
        Task<bool> CanStudentTakeExamAsync(int examId, int studentId);
        Task<Exam?> StartExamForStudentAsync(int examId, int studentId);
        Task<IEnumerable<StudentExam>> GetExamResultsAsync(int examId);
        Task<StudentExam?> GetStudentExamResultAsync(int examId, int studentId);
        Task<bool> SubmitStudentExamAsync(int studentExamId);
        Task<bool> SaveStudentAnswerAsync(int studentExamId, int questionId, char selectedOption);
        Task<IEnumerable<Question>> GetExamQuestionsAsync(int examId);
        Task<Question?> GetQuestionByIdAsync(int questionId);
        Task<Question> AddQuestionAsync(Question question);
        Task<Question> UpdateQuestionAsync(Question question);
        Task<bool> DeleteQuestionAsync(int questionId);
        Task<bool> AssignStudentsToExamAsync(int examId, List<int> studentIds);
        Task<IEnumerable<User>> GetStudentsForExamAssignmentAsync(int courseId);
        Task<IEnumerable<Exam>> GetUpcomingExamsForStudentAsync(int studentId);
        Task<IEnumerable<Question>> GetQuestionsByCourseIdAsync(int courseId);
        Task<IEnumerable<User>> GetAssignedStudentsAsync(int examId);
    }
}

