using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class StudentExamRepository : IStudentExamRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentExam?> GetByIdAsync(int id)
        {
            return await _context.StudentExams
                .Include(se => se.Exam)
                .Include(se => se.Student)
                .Include(se => se.StudentAnswers)
                .FirstOrDefaultAsync(se => se.StudentExamId == id);
        }

        public async Task<StudentExam?> GetByExamAndStudentAsync(int examId, int studentId)
        {
            return await _context.StudentExams
                .Include(se => se.Exam)
                .Include(se => se.Student)
                .Include(se => se.StudentAnswers)
                .FirstOrDefaultAsync(se => se.ExamId == examId && se.StudentId == studentId);
        }

        public async Task<IEnumerable<StudentExam>> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentExams
                .Include(se => se.Exam)
                .Include(se => se.Exam.Course)
                .Include(se => se.Exam.Teacher)
                .Where(se => se.StudentId == studentId)
                .OrderByDescending(se => se.StartedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentExam>> GetByExamIdAsync(int examId)
        {
            return await _context.StudentExams
                .Include(se => se.Student)
                .Include(se => se.StudentAnswers)
                .Where(se => se.ExamId == examId)
                .OrderBy(se => se.Student.FullName)
                .ToListAsync();
        }

        public async Task<StudentExam> CreateAsync(StudentExam studentExam)
        {
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();
            return studentExam;
        }

        public async Task<StudentExam> UpdateAsync(StudentExam studentExam)
        {
            _context.StudentExams.Update(studentExam);
            await _context.SaveChangesAsync();
            return studentExam;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var studentExam = await _context.StudentExams.FindAsync(id);
            if (studentExam == null) return false;

            _context.StudentExams.Remove(studentExam);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StudentExamExistsAsync(int examId, int studentId)
        {
            return await _context.StudentExams
                .AnyAsync(se => se.ExamId == examId && se.StudentId == studentId);
        }

        public async Task<bool> HasStudentStartedExamAsync(int examId, int studentId)
        {
            return await _context.StudentExams
                .AnyAsync(se => se.ExamId == examId && se.StudentId == studentId && se.StartedAt.HasValue);
        }

        public async Task<bool> HasStudentCompletedExamAsync(int examId, int studentId)
        {
            return await _context.StudentExams
                .AnyAsync(se => se.ExamId == examId && se.StudentId == studentId && se.Completed);
        }

        public async Task<int> CalculateScoreAsync(int studentExamId)
        {
            // Toplam soru sayısını sınavın gerçek soru sayısından al
            var se = await _context.StudentExams
                .Include(x => x.Exam)
                .FirstOrDefaultAsync(x => x.StudentExamId == studentExamId);
            if (se == null) return 0;

            var totalQuestions = await _context.Questions
                .CountAsync(q => q.ExamId == se.ExamId);

            var correctAnswers = await _context.StudentAnswers
                .CountAsync(sa => sa.StudentExamId == studentExamId && sa.IsCorrect);

            if (totalQuestions == 0) return 0;
            return (int)Math.Round((double)correctAnswers / totalQuestions * 100);
        }

        public async Task<StudentExam?> StartExamAsync(int examId, int studentId)
        {
            var studentExam = await _context.StudentExams
                .FirstOrDefaultAsync(se => se.ExamId == examId && se.StudentId == studentId);

            if (studentExam == null)
            {
                studentExam = new StudentExam
                {
                    ExamId = examId,
                    StudentId = studentId,
                    StartedAt = DateTime.Now,
                    Completed = false
                };
                _context.StudentExams.Add(studentExam);
            }
            else
            {
                studentExam.StartedAt = DateTime.Now;
                _context.StudentExams.Update(studentExam);
            }

            await _context.SaveChangesAsync();
            return studentExam;
        }

        public async Task<bool> SubmitExamAsync(int studentExamId)
        {
            var studentExam = await _context.StudentExams.FindAsync(studentExamId);
            if (studentExam == null) return false;

            studentExam.Completed = true;
            studentExam.FinishedAt = DateTime.Now;
            studentExam.Score = await CalculateScoreAsync(studentExamId);

            _context.StudentExams.Update(studentExam);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

