using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User> RegisterAsync(string fullName, string email, string password, string role);
        Task<IEnumerable<User>> GetStudentsByDepartmentAndClassAsync(string department, string @class);
        Task<IEnumerable<string>> GetAllDepartmentsAsync();
        Task<IEnumerable<string>> GetClassesByDepartmentAsync(string department);
        Task<IEnumerable<User>> GetAllStudentsAsync();
    }
}





