using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Data;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetStudentsByDepartmentAndClassAsync(string department, string @class)
        {
            return await _context.Users
                .Where(u => u.Role == "Student" && u.Department == department && u.Class == @class)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAllDepartmentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Student" && !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetClassesByDepartmentAsync(string department)
        {
            return await _context.Users
                .Where(u => u.Role == "Student" && u.Department == department && !string.IsNullOrEmpty(u.Class))
                .Select(u => u.Class!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.Department)
                .ThenBy(u => u.Class)
                .ThenBy(u => u.FullName)
                .ToListAsync();
        }
    }
}













