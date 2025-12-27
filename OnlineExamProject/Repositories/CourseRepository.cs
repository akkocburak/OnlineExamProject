using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == id);
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<Course> CreateAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<Course> UpdateAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CourseExistsAsync(int id)
        {
            return await _context.Courses
                .AnyAsync(c => c.CourseId == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId)
        {
            return await _context.CourseStudents
                .Where(cs => cs.StudentId == studentId)
                .Include(cs => cs.Course)
                .ThenInclude(c => c.Teacher)
                .Select(cs => cs.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsByCourseIdAsync(int courseId)
        {
            return await _context.CourseStudents
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Student)
                .Select(cs => cs.Student)
                .ToListAsync();
        }

        public async Task<bool> AssignStudentToCourseAsync(int courseId, int studentId)
        {
            try
            {
                // Zaten atanmış mı kontrol et
                var exists = await _context.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);

                if (exists) return true; // Zaten atanmış, başarılı sayılır

                var courseStudent = new CourseStudent
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    AssignedAt = DateTime.Now
                };

                _context.CourseStudents.Add(courseStudent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStudentFromCourseAsync(int courseId, int studentId)
        {
            try
            {
                var courseStudent = await _context.CourseStudents
                    .FirstOrDefaultAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);

                if (courseStudent == null) return true; // Zaten yok, başarılı sayılır

                _context.CourseStudents.Remove(courseStudent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveAllStudentsFromCourseAsync(int courseId)
        {
            try
            {
                var courseStudents = await _context.CourseStudents
                    .Where(cs => cs.CourseId == courseId)
                    .ToListAsync();

                if (courseStudents.Any())
                {
                    _context.CourseStudents.RemoveRange(courseStudents);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}













