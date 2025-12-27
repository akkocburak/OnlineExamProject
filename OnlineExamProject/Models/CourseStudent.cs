using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("CourseStudents")]
    public class CourseStudent
    {
        [Key]
        public int CourseStudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public DateTime? AssignedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;
    }
}



