using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Models;
namespace SchoolAssignments.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets pro jednotlivé tabulky
        public DbSet<User> Users { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<AutomationStatus> AutomationStatuses { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<SubmissionFile> SubmissionFiles { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User konfigurace
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

            // Class konfigurace
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Subject).HasMaxLength(50).IsRequired();

                // Vztah s učitelem
                entity.HasOne(e => e.Teacher)
                      .WithMany(e => e.TeacherClasses)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ClassStudent konfigurace
            modelBuilder.Entity<ClassStudent>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Zajistí, že student může být v jedné třídě pouze jednou
                entity.HasIndex(e => new { e.ClassId, e.StudentId }).IsUnique();

                // Vztahy
                entity.HasOne(e => e.Class)
                      .WithMany(e => e.Students)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(e => e.StudentClasses)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Activity konfigurace
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasColumnType("text");

                // Vztah s třídou
                entity.HasOne(e => e.Class)
                      .WithMany(e => e.Activities)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Vztah s učitelem (CreatedByUser)
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany() // pokud nechceš kolekci "CreatedActivities" u User
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Submission konfigurace
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TextContent).HasColumnType("text");
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.Feedback).HasColumnType("text");

                entity.HasIndex(e => new { e.ActivityId, e.StudentId });

                // Vztahy
                entity.HasOne(e => e.Activity)
                      .WithMany(e => e.Submissions)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(e => e.Submissions)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SystemLog konfigurace
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Details).HasColumnType("text");

                // Vztah s uživatelem (může být null pro systémové akce)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AutomationStatus konfigurace
            modelBuilder.Entity<AutomationStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastError).HasColumnType("text");
                entity.HasIndex(e => e.ServiceName).IsUnique();
            });
            // Question konfigurace
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();

                entity.HasOne(e => e.Activity)
                      .WithMany(a => a.Questions)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AnswerOption konfigurace
            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.IsCorrect).IsRequired();

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // StudentAnswer konfigurace
            modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Vztah StudentAnswer -> Submission (mnoho StudentAnswer na jednu Submission)
                entity.HasOne(e => e.Submission)
                      .WithMany(s => s.StudentAnswers)
                      .HasForeignKey(e => e.SubmissionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Vztah StudentAnswer -> Question (mnoho StudentAnswer na jednu Question)
                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Vztah StudentAnswer -> AnswerOption (jedna odpověď je jedna možnost)
                entity.HasOne(e => e.AnswerOption)
                      .WithMany()
                      .HasForeignKey(e => e.AnswerOptionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });



            // Seed data pro základní role
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Vytvoření základního admin účtu
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@school.cz",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.Admin,
                    PasswordHash = "$2a$11$heJOy7PjCswWolczamTSPuqjj./ZYYLrb3PsUhFO.FwO7rgTgTvti", // Změňte v produkci!
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Základní automation status
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
