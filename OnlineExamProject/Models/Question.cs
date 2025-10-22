using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        // Şekilli sorular için resim yolu
        public string? ImagePath { get; set; }

        // Seçenek sayısı (4 veya 5)
        [Required]
        [Range(4, 5)]
        public int OptionCount { get; set; } = 4;

        [Required]
        [MaxLength(300)]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string OptionD { get; set; } = string.Empty;

        // 5. seçenek (opsiyonel)
        [MaxLength(300)]
        public string? OptionE { get; set; }

        [Required]
        [MaxLength(1)]
        public char CorrectOption { get; set; }

        // Soru puanı
        [Required]
        [Range(1, 100)]
        public int Points { get; set; } = 1;

        // Ders ID (soru bankası için)
        public int? CoursesID { get; set; }

        // Navigation Properties
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; } = null!;

        [ForeignKey("CoursesID")]
        public virtual Course? Course { get; set; }

        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}

