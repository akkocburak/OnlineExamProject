using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IExamRepository _examRepository;
        private readonly IExamService _examService;

        public CourseService(ICourseRepository courseRepository, IExamRepository examRepository, IExamService examService)
        {
            _courseRepository = courseRepository;
            _examRepository = examRepository;
            _examService = examService;
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
    }
}

