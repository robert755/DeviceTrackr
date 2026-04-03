using DeviceTrackr.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceTrackr.Api.Data;

public class DeviceTrackrDbContext(DbContextOptions<DeviceTrackrDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Type).HasConversion<int>();
            entity.Property(x => x.OperatingSystem).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OsVersion).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Processor).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.AssignedUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}
