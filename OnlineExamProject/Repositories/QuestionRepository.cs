using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public QuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Exam)
                .FirstOrDefaultAsync(q => q.QuestionId == id);
        }

        public async Task<IEnumerable<Question>> GetByExamIdAsync(int examId)
        {
            return await _context.Questions
                .Include(q => q.Exam)
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.QuestionId)
                .ToListAsync();
        }

        public async Task<Question> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByExamIdAsync(int examId)
        {
            var questions = await _context.Questions
                .Where(q => q.ExamId == examId)
                .ToListAsync();

            if (questions.Any())
            {
                _context.Questions.RemoveRange(questions);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> QuestionExistsAsync(int id)
        {
            return await _context.Questions
                .AnyAsync(q => q.QuestionId == id);
        }

        public async Task<int> GetQuestionCountByExamIdAsync(int examId)
        {
            return await _context.Questions
                .CountAsync(q => q.ExamId == examId);
        }

        public async Task<IEnumerable<Question>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Questions
                .Include(q => q.Exam)
                .Where(q => q.CoursesID == courseId)
                .OrderByDescending(q => q.Exam.CreatedAt)
                .ToListAsync();
        }
    }
}





