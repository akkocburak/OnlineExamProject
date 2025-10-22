using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineExamProject.Models;

namespace OnlineExamProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for our custom entities
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<ExamStudent> ExamStudents { get; set; }
        public DbSet<QuestionBank> QuestionBank { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Role constraint
                entity.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Student', 'Teacher')");
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.CourseName).IsRequired().HasMaxLength(100);
                
                // Foreign key relationship
                entity.HasOne(e => e.Teacher)
                      .WithMany(u => u.Courses)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Exam entity
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.ExamId);
                entity.Property(e => e.ExamTitle).IsRequired().HasMaxLength(200);
                
                // Foreign key relationships
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Exams)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Teacher)
                      .WithMany(u => u.Exams)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.CourseId);
            });

            // Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.OptionA).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionB).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionC).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionD).IsRequired().HasMaxLength(300);
                entity.Property(e => e.CorrectOption).IsRequired().HasMaxLength(1);
                
                // Foreign key relationship
                entity.HasOne(e => e.Exam)
                      .WithMany(ex => ex.Questions)
                      .HasForeignKey(e => e.ExamId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CorrectOption constraint
                entity.HasCheckConstraint("CK_Questions_CorrectOption", "[CorrectOption] IN ('A', 'B', 'C', 'D')");
            });

            // Configure StudentExam entity
            modelBuilder.Entity<StudentExam>(entity =>
            {
                entity.HasKey(e => e.StudentExamId);
                
                // Foreign key relationships
                entity.HasOne(e => e.Exam)
                      .WithMany(ex => ex.StudentExams)
                      .HasForeignKey(e => e.ExamId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.StudentExams)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint
                entity.HasIndex(e => new { e.ExamId, e.StudentId }).IsUnique();
            });

            // Configure StudentAnswer entity
            modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.HasKey(e => e.AnswerId);
                entity.Property(e => e.SelectedOption).IsRequired().HasMaxLength(1);
                
                // Foreign key relationships
                entity.HasOne(e => e.StudentExam)
                      .WithMany(se => se.StudentAnswers)
                      .HasForeignKey(e => e.StudentExamId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.StudentAnswers)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // SelectedOption constraint
                entity.HasCheckConstraint("CK_StudentAnswers_SelectedOption", "[SelectedOption] IN ('A', 'B', 'C', 'D')");
            });

            // Configure ExamStudent entity
            modelBuilder.Entity<ExamStudent>(entity =>
            {
                entity.HasKey(e => e.ExamStudentId);
                
                // Foreign key relationships
                entity.HasOne(e => e.Exam)
                      .WithMany(ex => ex.ExamStudents)
                      .HasForeignKey(e => e.ExamId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.ExamStudents)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint - bir öğrenci aynı sınava sadece bir kez atanabilir
                entity.HasIndex(e => new { e.ExamId, e.StudentId }).IsUnique();
            });

            // Configure QuestionBank entity
            modelBuilder.Entity<QuestionBank>(entity =>
            {
                entity.HasKey(e => e.QuestionBankId);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.OptionA).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionB).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionC).IsRequired().HasMaxLength(300);
                entity.Property(e => e.OptionD).IsRequired().HasMaxLength(300);
                entity.Property(e => e.CorrectOption).IsRequired().HasMaxLength(1);
                entity.Property(e => e.Difficulty).HasMaxLength(20);
                entity.Property(e => e.Tags).HasMaxLength(200);
                
                // Foreign key relationships
                entity.HasOne(e => e.Teacher)
                      .WithMany()
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // CorrectOption constraint
                entity.HasCheckConstraint("CK_QuestionBank_CorrectOption", "[CorrectOption] IN ('A', 'B', 'C', 'D', 'E')");
                
                // Difficulty constraint
                entity.HasCheckConstraint("CK_QuestionBank_Difficulty", "[Difficulty] IN ('Kolay', 'Orta', 'Zor')");
            });
        }
    }
}
