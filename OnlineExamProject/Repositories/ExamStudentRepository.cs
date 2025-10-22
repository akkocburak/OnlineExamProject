using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class ExamStudentRepository : IExamStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public ExamStudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExamStudent>> GetByExamIdAsync(int examId)
        {
            return await _context.ExamStudents
                .Include(es => es.Student)
                .Where(es => es.ExamId == examId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExamStudent>> GetByStudentIdAsync(int studentId)
        {
            return await _context.ExamStudents
                .Include(es => es.Exam)
                .ThenInclude(e => e.Course)
                .Where(es => es.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<ExamStudent?> GetByExamAndStudentAsync(int examId, int studentId)
        {
            return await _context.ExamStudents
                .Include(es => es.Exam)
                .Include(es => es.Student)
                .FirstOrDefaultAsync(es => es.ExamId == examId && es.StudentId == studentId);
        }

        public async Task<ExamStudent> CreateAsync(ExamStudent examStudent)
        {
            _context.ExamStudents.Add(examStudent);
            await _context.SaveChangesAsync();
            return examStudent;
        }

        public async Task<bool> DeleteAsync(int examStudentId)
        {
            var examStudent = await _context.ExamStudents.FindAsync(examStudentId);
            if (examStudent == null) return false;

            _context.ExamStudents.Remove(examStudent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByExamIdAsync(int examId)
        {
            var examStudents = await _context.ExamStudents
                .Where(es => es.ExamId == examId)
                .ToListAsync();

            if (!examStudents.Any()) return false;

            _context.ExamStudents.RemoveRange(examStudents);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsStudentAssignedToExamAsync(int examId, int studentId)
        {
            return await _context.ExamStudents
                .AnyAsync(es => es.ExamId == examId && es.StudentId == studentId);
        }

        public async Task<IEnumerable<User>> GetStudentsByExamIdAsync(int examId)
        {
            return await _context.ExamStudents
                .Include(es => es.Student)
                .Where(es => es.ExamId == examId)
                .Select(es => es.Student)
                .ToListAsync();
        }
    }
}
