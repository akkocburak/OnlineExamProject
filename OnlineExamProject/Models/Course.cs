using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public int TeacherId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; } = null!;

        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}

