using OnlineExamProject.Models;

namespace OnlineExamProject.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(int id);
        Task<IEnumerable<Course>> GetAllAsync();
        Task<IEnumerable<Course>> GetByTeacherIdAsync(int teacherId);
        Task<Course> CreateAsync(Course course);
        Task<Course> UpdateAsync(Course course);
        Task<bool> DeleteAsync(int id);
        Task<bool> CourseExistsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId);
        Task<IEnumerable<User>> GetStudentsByCourseIdAsync(int courseId);
        Task<bool> AssignStudentToCourseAsync(int courseId, int studentId);
        Task<bool> RemoveStudentFromCourseAsync(int courseId, int studentId);
        Task<bool> RemoveAllStudentsFromCourseAsync(int courseId);
    }
}

