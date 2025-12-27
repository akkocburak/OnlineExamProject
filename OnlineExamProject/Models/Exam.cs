using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ExamTitle { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool AllowBackNavigation { get; set; } = true;

        // Sınav tipi: Vize, Final, vb.
        [MaxLength(50)]
        public string? ExamType { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; } = null!;

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
        public virtual ICollection<ExamStudent> ExamStudents { get; set; } = new List<ExamStudent>();
    }
}

