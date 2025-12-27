using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;
using OnlineExamProject.Services;

namespace OnlineExamProject.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IExamRepository _examRepository;
        private readonly IExamService _examService;
        private readonly IUserService _userService;

        public CourseService(ICourseRepository courseRepository, IExamRepository examRepository, IExamService examService, IUserService userService)
        {
            _courseRepository = courseRepository;
            _examRepository = examRepository;
            _examService = examService;
            _userService = userService;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherIdAsync(int teacherId)
        {
            return await _courseRepository.GetByTeacherIdAsync(teacherId);
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            return await _courseRepository.CreateAsync(course);
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            return await _courseRepository.UpdateAsync(course);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            // Bu derse bağlı tüm sınavları sil (sınav soruları ve ilişkili kayıtlar servis içinde temizlenir)
            var exams = await _examRepository.GetByCourseIdAsync(id);
            foreach (var exam in exams)
            {
                await _examService.DeleteExamAsync(exam.ExamId);
            }

            // Ardından dersi sil
            return await _courseRepository.DeleteAsync(id);
        }

        public async Task<bool> CourseExistsAsync(int id)
        {
            return await _courseRepository.CourseExistsAsync(id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId)
        {
            return await _courseRepository.GetCoursesByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<User>> GetStudentsByCourseIdAsync(int courseId)
        {
            return await _courseRepository.GetStudentsByCourseIdAsync(courseId);
        }

        public async Task<bool> AssignStudentToCourseAsync(int courseId, int studentId)
        {
            return await _courseRepository.AssignStudentToCourseAsync(courseId, studentId);
        }

        public async Task<bool> RemoveStudentFromCourseAsync(int courseId, int studentId)
        {
            return await _courseRepository.RemoveStudentFromCourseAsync(courseId, studentId);
        }

        public async Task AssignStudentsToCourseByDepartmentAndClassAsync(int courseId, string department, string @class)
        {
            // Bu bölüm ve sınıftaki tüm öğrencileri bul
            var students = await _userService.GetStudentsByDepartmentAndClassAsync(department, @class);
            
            // Her öğrenciyi derse ata
            foreach (var student in students)
            {
                await _courseRepository.AssignStudentToCourseAsync(courseId, student.UserId);
            }
        }

        public async Task UpdateStudentAssignmentsForCourseAsync(int courseId, string department, string @class)
        {
            // Mevcut öğrenci atamalarını kaldır
            await _courseRepository.RemoveAllStudentsFromCourseAsync(courseId);
            
            // Bu bölüm ve sınıftaki tüm öğrencileri bul
            var students = await _userService.GetStudentsByDepartmentAndClassAsync(department, @class);
            
            // Yeni öğrencileri ata
            foreach (var student in students)
            {
                await _courseRepository.AssignStudentToCourseAsync(courseId, student.UserId);
            }
        }

        public async Task<bool> RemoveAllStudentsFromCourseAsync(int courseId)
        {
            return await _courseRepository.RemoveAllStudentsFromCourseAsync(courseId);
        }
    }
}

