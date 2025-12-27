using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<IEnumerable<Course>> GetCoursesByTeacherIdAsync(int teacherId);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> CourseExistsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId);
        Task<IEnumerable<User>> GetStudentsByCourseIdAsync(int courseId);
        Task<bool> AssignStudentToCourseAsync(int courseId, int studentId);
        Task<bool> RemoveStudentFromCourseAsync(int courseId, int studentId);
        Task AssignStudentsToCourseByDepartmentAndClassAsync(int courseId, string department, string @class);
        Task UpdateStudentAssignmentsForCourseAsync(int courseId, string department, string @class);
        Task<bool> RemoveAllStudentsFromCourseAsync(int courseId);
    }
}













