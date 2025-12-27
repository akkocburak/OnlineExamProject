using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class QuestionBankRepository : IQuestionBankRepository
    {
        private readonly ApplicationDbContext _context;

        public QuestionBankRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuestionBank>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.QuestionBank
                .Include(qb => qb.Course)
                .Where(qb => qb.TeacherId == teacherId)
                .OrderByDescending(qb => qb.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionBank>> GetByCourseIdAsync(int courseId)
        {
            return await _context.QuestionBank
                .Include(qb => qb.Teacher)
                .Where(qb => qb.CourseId == courseId)
                .OrderByDescending(qb => qb.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionBank>> GetByTeacherAndCourseAsync(int teacherId, int courseId)
        {
            return await _context.QuestionBank
                .Include(qb => qb.Course)
                .Where(qb => qb.TeacherId == teacherId && qb.CourseId == courseId)
                .OrderByDescending(qb => qb.CreatedAt)
                .ToListAsync();
        }

        public async Task<QuestionBank?> GetByIdAsync(int id)
        {
            return await _context.QuestionBank
                .Include(qb => qb.Course)
                .Include(qb => qb.Teacher)
                .FirstOrDefaultAsync(qb => qb.QuestionBankId == id);
        }

        public async Task<QuestionBank> CreateAsync(QuestionBank questionBank)
        {
            _context.QuestionBank.Add(questionBank);
            await _context.SaveChangesAsync();
            return questionBank;
        }

        public async Task<QuestionBank> UpdateAsync(QuestionBank questionBank)
        {
            _context.QuestionBank.Update(questionBank);
            await _context.SaveChangesAsync();
            return questionBank;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var questionBank = await _context.QuestionBank.FindAsync(id);
            if (questionBank == null) return false;

            _context.QuestionBank.Remove(questionBank);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<QuestionBank>> SearchAsync(int teacherId, string? searchTerm, string? difficulty, int? courseId)
        {
            var query = _context.QuestionBank
                .Include(qb => qb.Course)
                .Where(qb => qb.TeacherId == teacherId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(qb => qb.QuestionText.Contains(searchTerm) || 
                                        qb.Tags.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(qb => qb.Difficulty == difficulty);
            }

            if (courseId.HasValue)
            {
                query = query.Where(qb => qb.CourseId == courseId.Value);
            }

            return await query.OrderByDescending(qb => qb.CreatedAt).ToListAsync();
        }

        public async Task<bool> AddToExamAsync(int questionBankId, int examId)
        {
            var questionBank = await _context.QuestionBank.FindAsync(questionBankId);
            if (questionBank == null) return false;

            var question = new Question
            {
                ExamId = examId,
                QuestionText = questionBank.QuestionText,
                ImagePath = questionBank.ImagePath,
                OptionCount = questionBank.OptionCount,
                OptionA = questionBank.OptionA,
                OptionB = questionBank.OptionB,
                OptionC = questionBank.OptionC,
                OptionD = questionBank.OptionD,
                OptionE = questionBank.OptionE,
                CorrectOption = questionBank.CorrectOption
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
