using Microsoft.EntityFrameworkCore;
using System.IO;
using TimeWise.Models;

namespace TimeWise.Data
{
    /// <summary>
    /// TimeWise??????
    /// </summary>
    public class TimeWiseDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Category> Categories { get; set; }

        public TimeWiseDbContext()
        {
        }

        public TimeWiseDbContext(DbContextOptions<TimeWiseDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // ??????????
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TimeWise");

                // ??????
                Directory.CreateDirectory(appDataPath);

                // ???????
                string dbPath = Path.Combine(appDataPath, "timewise.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ??Category??
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ColorHex).IsRequired().HasMaxLength(7);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
            });

            // ??Appointment??
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

                // ??????
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Appointments)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ???? - ????StartTime????????SQLite TimeSpan??
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Title);
                // ?????? Date + StartTime ????????SQLite?TimeSpan????
            });

            // ????
            SeedData(modelBuilder);
        }

        /// <summary>
        /// ??????
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // ??????
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Work", ColorHex = "#ADD8E6", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Personal", ColorHex = "#90EE90", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Health", ColorHex = "#FFFFE0", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Hobbies", ColorHex = "#FFDAB9", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 5, Name = "Meeting", ColorHex = "#DDA0DD", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 6, Name = "Appointment", ColorHex = "#FFC0CB", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 7, Name = "Travel", ColorHex = "#B0C4DE", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Category { Id = 8, Name = "Education", ColorHex = "#F0E68C", IsDefault = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            );
        }

        /// <summary>
        /// ?????????UpdatedAt??
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// ???????????UpdatedAt??
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// ?????
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Appointment appointment)
                {
                    appointment.UpdatedAt = DateTime.Now;
                }
                else if (entry.Entity is Category category)
                {
                    category.UpdatedAt = DateTime.Now;
                }
            }
        }
    }
}