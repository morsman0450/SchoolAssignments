using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Models;

namespace SchoolAssignments.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<ClassTeacherSubject> ClassTeacherSubjects { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<AutomationStatus> AutomationStatuses { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<SubmissionFile> SubmissionFiles { get; set; }
        public DbSet<ClassTeacher> ClassTeachers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            });

            // ClassTeacher
            modelBuilder.Entity<ClassTeacher>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Class)
                      .WithMany(c => c.ClassTeachers) // musíš přidat ICollection<ClassTeacher> do Class
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Teacher)
                      .WithMany() // nebo ICollection<ClassTeacher> do User, pokud chceš
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // Class
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // ClassStudent
            modelBuilder.Entity<ClassStudent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ClassId, e.StudentId }).IsUnique();

                entity.HasOne(e => e.Class)
                      .WithMany(c => c.Students)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.StudentClasses)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ClassTeacherSubject
            modelBuilder.Entity<ClassTeacherSubject>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Class)
                      .WithMany(c => c.ClassTeacherSubjects)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Teacher)
                      .WithMany(u => u.TeacherSubjects)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subject)
                      .WithMany(s => s.ClassTeacherSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Activity
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasColumnType("text");

                entity.HasOne(e => e.ClassTeacherSubject)
                      .WithMany(cts => cts.Activities)
                      .HasForeignKey(e => e.ClassTeacherSubjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Submission
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TextContent).HasColumnType("text");
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.Feedback).HasColumnType("text");
                entity.HasIndex(e => new { e.ActivityId, e.StudentId });

                entity.HasOne(e => e.Activity)
                      .WithMany(a => a.Submissions)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.Submissions)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SystemLog
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Details).HasColumnType("text");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AutomationStatus
            modelBuilder.Entity<AutomationStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastError).HasColumnType("text");
                entity.HasIndex(e => e.ServiceName).IsUnique();
            });

            // Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();

                entity.HasOne(e => e.Activity)
                      .WithMany(a => a.Questions)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AnswerOption
            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // StudentAnswer
            modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Submission)
                      .WithMany(s => s.StudentAnswers)
                      .HasForeignKey(e => e.SubmissionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AnswerOption)
                      .WithMany()
                      .HasForeignKey(e => e.AnswerOptionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SubmissionFile
            modelBuilder.Entity<SubmissionFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.Submission)
                      .WithMany(s => s.Files)
                      .HasForeignKey(e => e.SubmissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // -----------------------------
            // Users
            // -----------------------------
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@school.cz",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.Admin,
                    PasswordHash = "...", // zadej hash hesla
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // -----------------------------
            // Classes
            // -----------------------------
            modelBuilder.Entity<Class>().HasData(
                new Class
                {
                    Id = 1,
                    Name = "1.A",
                    IsActive = true
                }
            );

            // -----------------------------
            // Subjects
            // -----------------------------
            modelBuilder.Entity<Subject>().HasData(
                new Subject
                {
                    Id = 1,
                    Name = "Matematika"
                }
            );

            // -----------------------------
            // ClassTeacherSubjects
            // -----------------------------
            modelBuilder.Entity<ClassTeacherSubject>().HasData(
                new ClassTeacherSubject
                {
                    Id = 1,
                    ClassId = 1,     // musí existovat
                    TeacherId = 1,   // musí existovat
                    SubjectId = 1    // musí existovat
                }
            );

            // -----------------------------
            // Activities
            // -----------------------------
            modelBuilder.Entity<Activity>().HasData(
                new Activity
                {
                    Id = 1,
                    Title = "Ukázková aktivita",
                    Description = "Popis aktivity",
                    ClassTeacherSubjectId = 1,
                    CreatedByUserId = 1,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    MaxPoints = 10,
                    IsActive = true,
                    Type = ActivityType.Test
                }
            );

            // -----------------------------
            // AutomationStatus
            // -----------------------------
            modelBuilder.Entity<AutomationStatus>().HasData(
                new AutomationStatus
                {
                    Id = 1,
                    ServiceName = "AutoGrading",
                    IsRunning = true,
                    LastCheck = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }


    }
}
