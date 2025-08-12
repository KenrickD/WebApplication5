using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<OTPSetting> OTPSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OTPSetting>(entity =>
            {
                entity.HasKey(e => e.OTPId);
                entity.Property(e => e.OTPId).IsRequired();
                entity.Property(e => e.OTPAction).HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Whatsapp).IsRequired();
            });
        }
    }
}