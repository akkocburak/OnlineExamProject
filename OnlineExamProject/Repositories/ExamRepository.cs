using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly ApplicationDbContext _context;

        public ExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .FirstOrDefaultAsync(e => e.ExamId == id);
        }

        public async Task<Exam?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Include(e => e.Questions)
                .Include(e => e.StudentExams)
                .FirstOrDefaultAsync(e => e.ExamId == id);
        }

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Include(e => e.Questions)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Include(e => e.Questions)
                .Where(e => e.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetActiveExamsAsync()
        {
            var now = DateTime.Now;
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Where(e => e.StartTime <= now && e.EndTime >= now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetExamsForStudentAsync(int studentId)
        {
            var now = DateTime.Now;
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Include(e => e.Questions)
                .Include(e => e.ExamStudents)
                .Where(e => e.ExamStudents.Any(es => es.StudentId == studentId)) // Sadece atanmış öğrenciler
                .Where(e => e.StartTime <= now && e.EndTime >= now)
                // Öğrenci daha önce bu sınavı TAMAMLAMADI ise listelensin (başlamış ama bitirmemiş olabilir)
                .Where(e => !e.StudentExams.Any(se => se.StudentId == studentId && se.Completed))
                .ToListAsync();
        }

        public async Task<Exam> CreateAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return exam;
        }

        public async Task<Exam> UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
            return exam;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return false;

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExamExistsAsync(int id)
        {
            return await _context.Exams
                .AnyAsync(e => e.ExamId == id);
        }

        public async Task<bool> IsExamActiveAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return false;

            var now = DateTime.Now;
            return exam.StartTime <= now && exam.EndTime >= now;
        }

        public async Task<bool> IsStudentEligibleForExamAsync(int examId, int studentId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return false;

            var now = DateTime.Now;
            if (exam.StartTime > now || exam.EndTime < now) return false;

            // Öğrenci bu sınava atanmış mı kontrol et
            var isAssigned = await _context.ExamStudents
                .AnyAsync(es => es.ExamId == examId && es.StudentId == studentId);
            
            if (!isAssigned) return false;

            // Öğrenci daha önce bu sınavı tamamlamışsa tekrar giremez
            var completedExam = await _context.StudentExams
                .AnyAsync(se => se.ExamId == examId && se.StudentId == studentId && se.Completed);
            
            return !completedExam;
        }

        public async Task<IEnumerable<Exam>> GetUpcomingExamsForStudentAsync(int studentId)
        {
            var now = DateTime.Now;
            return await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                .Include(e => e.Questions)
                .Include(e => e.ExamStudents)
                .Where(e => e.ExamStudents.Any(es => es.StudentId == studentId)) // Sadece atanmış öğrenciler
                .Where(e => e.StartTime > now) // Henüz başlamamış sınavlar
                .Where(e => !e.StudentExams.Any(se => se.StudentId == studentId && se.Completed)) // Tamamlanmamış
                .ToListAsync();
        }
    }
}

