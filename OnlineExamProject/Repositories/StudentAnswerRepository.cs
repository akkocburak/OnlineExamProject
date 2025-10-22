using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class StudentAnswerRepository : IStudentAnswerRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentAnswerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentAnswer?> GetByIdAsync(int id)
        {
            return await _context.StudentAnswers
                .Include(sa => sa.StudentExam)
                .Include(sa => sa.Question)
                .FirstOrDefaultAsync(sa => sa.AnswerId == id);
        }

        public async Task<IEnumerable<StudentAnswer>> GetByStudentExamIdAsync(int studentExamId)
        {
            return await _context.StudentAnswers
                .Include(sa => sa.Question)
                .Where(sa => sa.StudentExamId == studentExamId)
                .OrderBy(sa => sa.Question.QuestionId)
                .ToListAsync();
        }

        public async Task<StudentAnswer?> GetByStudentExamAndQuestionAsync(int studentExamId, int questionId)
        {
            return await _context.StudentAnswers
                .Include(sa => sa.StudentExam)
                .Include(sa => sa.Question)
                .FirstOrDefaultAsync(sa => sa.StudentExamId == studentExamId && sa.QuestionId == questionId);
        }

        public async Task<StudentAnswer> CreateAsync(StudentAnswer studentAnswer)
        {
            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();
            return studentAnswer;
        }

        public async Task<StudentAnswer> UpdateAsync(StudentAnswer studentAnswer)
        {
            _context.StudentAnswers.Update(studentAnswer);
            await _context.SaveChangesAsync();
            return studentAnswer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var studentAnswer = await _context.StudentAnswers.FindAsync(id);
            if (studentAnswer == null) return false;

            _context.StudentAnswers.Remove(studentAnswer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByStudentExamIdAsync(int studentExamId)
        {
            var answers = await _context.StudentAnswers
                .Where(sa => sa.StudentExamId == studentExamId)
                .ToListAsync();

            if (answers.Any())
            {
                _context.StudentAnswers.RemoveRange(answers);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AnswerExistsAsync(int studentExamId, int questionId)
        {
            return await _context.StudentAnswers
                .AnyAsync(sa => sa.StudentExamId == studentExamId && sa.QuestionId == questionId);
        }

        public async Task<bool> SaveAnswerAsync(int studentExamId, int questionId, char selectedOption)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null) return false;

            var existingAnswer = await _context.StudentAnswers
                .FirstOrDefaultAsync(sa => sa.StudentExamId == studentExamId && sa.QuestionId == questionId);

            var isCorrect = selectedOption == question.CorrectOption;

            if (existingAnswer != null)
            {
                existingAnswer.SelectedOption = selectedOption;
                existingAnswer.IsCorrect = isCorrect;
                _context.StudentAnswers.Update(existingAnswer);
            }
            else
            {
                var studentAnswer = new StudentAnswer
                {
                    StudentExamId = studentExamId,
                    QuestionId = questionId,
                    SelectedOption = selectedOption,
                    IsCorrect = isCorrect
                };
                _context.StudentAnswers.Add(studentAnswer);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCorrectAnswerCountAsync(int studentExamId)
        {
            return await _context.StudentAnswers
                .CountAsync(sa => sa.StudentExamId == studentExamId && sa.IsCorrect);
        }
    }
}





