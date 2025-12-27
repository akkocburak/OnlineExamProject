using System.Security.Cryptography;
using System.Text;
using OnlineExamProject.Interfaces;
using OnlineExamProject.Models;

namespace OnlineExamProject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _userRepository.GetByRoleAsync(role);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Şifreyi hash'le
            user.PasswordHash = HashPassword(user.PasswordHash);
            return await _userRepository.CreateAsync(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.UserExistsAsync(email);
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword) return null;

            return user;
        }

        public async Task<User> RegisterAsync(string fullName, string email, string password, string role)
        {
            if (await UserExistsAsync(email))
            {
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password, // CreateUserAsync metodunda hash'lenecek
                Role = role,
                CreatedAt = DateTime.Now
            };

            return await CreateUserAsync(user);
        }

        public async Task<IEnumerable<User>> GetStudentsByDepartmentAndClassAsync(string department, string @class)
        {
            return await _userRepository.GetStudentsByDepartmentAndClassAsync(department, @class);
        }

        public async Task<IEnumerable<string>> GetAllDepartmentsAsync()
        {
            return await _userRepository.GetAllDepartmentsAsync();
        }

        public async Task<IEnumerable<string>> GetClassesByDepartmentAsync(string department)
        {
            return await _userRepository.GetClassesByDepartmentAsync(department);
        }

        public async Task<IEnumerable<User>> GetAllStudentsAsync()
        {
            return await _userRepository.GetAllStudentsAsync();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}





