using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Models;

namespace SmartParking_Api.Data;

public class SmartParkingDbContext : DbContext
{
    public SmartParkingDbContext(DbContextOptions<SmartParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Parque> Parques => Set<Parque>();
    public DbSet<Lugar> Lugares => Set<Lugar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Parque>(entity =>
        {
            entity.ToTable("Parques");
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Endereco).IsRequired().HasMaxLength(400);
        });

        modelBuilder.Entity<Lugar>(entity =>
        {
            entity.ToTable("Lugares");
            entity.Property(l => l.Codigo).IsRequired().HasMaxLength(50);
            entity.Property(l => l.Estado).IsRequired().HasMaxLength(20);

            entity.HasOne(l => l.Parque)
                .WithMany(p => p.Lugares)
                .HasForeignKey(l => l.ParqueId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}