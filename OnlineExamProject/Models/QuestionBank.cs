using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamProject.Models
{
    [Table("QuestionBank")]
    public class QuestionBank
    {
        [Key]
        public int QuestionBankId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public int CourseId { get; set; }

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

        // Soru kategorisi/tags
        [MaxLength(200)]
        public string? Tags { get; set; }

        // Soru zorluk seviyesi
        [MaxLength(20)]
        public string Difficulty { get; set; } = "Orta";

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
