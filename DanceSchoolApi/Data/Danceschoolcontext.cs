using DanceSchoolApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DanceSchoolApi.Data
{
    public class DanceSchoolContext : DbContext
    {
        public DanceSchoolContext(DbContextOptions<DanceSchoolContext> options)
            : base(options)
        {
        }

        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Tariff> Tariffs => Set<Tariff>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Teacher)
                .WithMany(te => te.Teams)
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Student)
                .WithMany(student => student.Subscriptions)
                .HasForeignKey(sub => sub.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Team)
                .WithMany(team => team.Subscriptions)
                .HasForeignKey(sub => sub.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Tariff)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(sub => sub.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .Property(s => s.DateOfBirth)
                .HasColumnType("date");

            modelBuilder.Entity<Student>()
                .Property(s => s.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Teacher>()
                .Property(t => t.DateOfBirth)
                .HasColumnType("date");

            modelBuilder.Entity<Subscription>()
                .Property(s => s.PurchaseDate)
                .HasColumnType("date");

            modelBuilder.Entity<Subscription>()
                .Property(s => s.EndDate)
                .HasColumnType("date");

            modelBuilder.Entity<Subscription>()
                .Property(s => s.IsCancelled)
                .HasDefaultValue(false);

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.StudentId, s.TeamId, s.EndDate });

            modelBuilder.Entity<Tariff>()
                .Property(t => t.IsArchive)
                .HasDefaultValue(false);

            modelBuilder.Entity<Tariff>()
                .Property(t => t.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Teacher>()
                .Property(t => t.Rating)
                .HasPrecision(3, 2);
        }
    }
}
