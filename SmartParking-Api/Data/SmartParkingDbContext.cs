using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Models;

namespace SmartParking_Api.Data;

public class SmartParkingDbContext : DbContext
{
    public SmartParkingDbContext(DbContextOptions<SmartParkingDbContext> options) : base(options) {}

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Parque> Parques => Set<Parque>();
    public DbSet<Lugar> Lugares => Set<Lugar>();
    public DbSet<Sensor> Sensores => Set<Sensor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.Property(x => x.UserName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.Role).IsRequired().HasMaxLength(20);
            e.Property(x => x.PasswordHash).IsRequired();

            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });


        modelBuilder.Entity<Parque>(entity =>
        {
            entity.ToTable("Parques");
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Endereco).IsRequired().HasMaxLength(400);
        });

        modelBuilder.Entity<Lugar>(entity =>
        {
            entity.ToTable("Lugares");
            entity.Property(l => l.Estado).IsRequired().HasMaxLength(20);
            
        });
        modelBuilder.Entity<Sensor>(e =>
        {
            e.ToTable("Sensores");
            e.HasIndex(s => s.ApiKeyHash).IsUnique();

            e.HasOne(s => s.Lugar)
                .WithMany() // ou .WithOne() se quiseres 1-para-1
                .HasForeignKey(s => s.LugarId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(s => s.LugarId).IsUnique(); // se quiseres 1 sensor por lugar
        });
    }
}