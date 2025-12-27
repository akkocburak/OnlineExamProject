using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        // Öğrenci için bölüm ve sınıf bilgileri
        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(50)]
        public string? Class { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
        public virtual ICollection<ExamStudent> ExamStudents { get; set; } = new List<ExamStudent>();
        public virtual ICollection<CourseStudent> CourseStudents { get; set; } = new List<CourseStudent>();
    }
}

