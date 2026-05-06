using Microsoft.EntityFrameworkCore;
using PadelApi.Domain.Entities;

namespace PadelApi.Infrastructure.Data;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Alumno>   Alumnos   => Set<Alumno>();
    public DbSet<Turno>    Turnos    => Set<Turno>();
    public DbSet<Reserva>  Reservas  => Set<Reserva>();
    public DbSet<NotifLog> NotifLogs => Set<NotifLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Alumno ────────────────────────────────────────────────
        mb.Entity<Alumno>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Telefono).IsUnique();
            e.Property(a => a.Nombre).HasMaxLength(100).IsRequired();
            e.Property(a => a.Telefono).HasMaxLength(20).IsRequired();
            e.Property(a => a.Categoria).HasMaxLength(50).IsRequired();
        });

        // ── Turno ─────────────────────────────────────────────────
        mb.Entity<Turno>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            e.Property(t => t.Categoria).HasMaxLength(50).IsRequired();
            e.Property(t => t.Hora).HasColumnType("time");
            e.Property(t => t.Genero).HasConversion<string>().HasMaxLength(20);
        });

        // ── Reserva ───────────────────────────────────────────────
        mb.Entity<Reserva>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.TokenCancelacion).IsUnique();
            e.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.FechaClase).HasColumnType("date");
            e.Property(r => r.TokenCancelacion).HasMaxLength(64).IsRequired();

            e.HasOne(r => r.Alumno)
             .WithMany(a => a.Reservas)
             .HasForeignKey(r => r.AlumnoId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Turno)
             .WithMany(t => t.Reservas)
             .HasForeignKey(r => r.TurnoId)
             .OnDelete(DeleteBehavior.Restrict);

            // Índice compuesto: un alumno no puede tener 2 reservas confirmadas al mismo turno/fecha
            e.HasIndex(r => new { r.AlumnoId, r.TurnoId, r.FechaClase });
        });

        // ── NotifLog ──────────────────────────────────────────────
        mb.Entity<NotifLog>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Tipo).HasConversion<string>().HasMaxLength(30);
            e.Property(n => n.Estado).HasMaxLength(60);

            e.HasOne(n => n.Alumno)
             .WithMany()
             .HasForeignKey(n => n.AlumnoId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
