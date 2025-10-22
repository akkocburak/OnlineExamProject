using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("StudentAnswers")]
    public class StudentAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int StudentExamId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [MaxLength(1)]
        public char SelectedOption { get; set; }

        public bool IsCorrect { get; set; }

        // Navigation Properties
        [ForeignKey("StudentExamId")]
        public virtual StudentExam StudentExam { get; set; } = null!;

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
    }
}

