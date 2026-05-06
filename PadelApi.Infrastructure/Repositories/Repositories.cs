using Microsoft.EntityFrameworkCore;
using PadelApi.Domain.Entities;
using PadelApi.Domain.Interfaces;
using PadelApi.Infrastructure.Data;

namespace PadelApi.Infrastructure.Repositories;

public class AlumnoRepository(PadelDbContext db) : IAlumnoRepository
{
    public Task<Alumno?> GetByTelefonoAsync(string tel) =>
        db.Alumnos.FirstOrDefaultAsync(a => a.Telefono == tel);

    public Task<Alumno?> GetByIdAsync(Guid id) =>
        db.Alumnos.FindAsync(id).AsTask();

    public async Task<IEnumerable<Alumno>> GetByCategoriaAsync(string categoria) =>
        await db.Alumnos.Where(a => a.Categoria == categoria && a.Activa).ToListAsync();

    public async Task<IEnumerable<Alumno>> GetAllAsync() =>
        await db.Alumnos.OrderBy(a => a.Nombre).ToListAsync();

    public async Task<Alumno> CreateAsync(Alumno alumno)
    {
        db.Alumnos.Add(alumno);
        await db.SaveChangesAsync();
        return alumno;
    }

    public async Task<Alumno> UpdateAsync(Alumno alumno)
    {
        db.Alumnos.Update(alumno);
        await db.SaveChangesAsync();
        return alumno;
    }

    public async Task DeleteAsync(Guid id)
    {
        var a = await db.Alumnos.FindAsync(id);
        if (a is not null) { db.Alumnos.Remove(a); await db.SaveChangesAsync(); }
    }
}

public class TurnoRepository(PadelDbContext db) : ITurnoRepository
{
    public Task<Turno?> GetByIdAsync(Guid id) =>
        db.Turnos.FindAsync(id).AsTask();

    public async Task<IEnumerable<Turno>> GetAllAsync() =>
        await db.Turnos.OrderBy(t => t.DiaSemana).ThenBy(t => t.Hora).ToListAsync();

    public async Task<IEnumerable<Turno>> GetActivosAsync() =>
        await db.Turnos.Where(t => t.Activo).OrderBy(t => t.DiaSemana).ThenBy(t => t.Hora).ToListAsync();

    public async Task<Turno> CreateAsync(Turno turno)
    {
        db.Turnos.Add(turno);
        await db.SaveChangesAsync();
        return turno;
    }

    public async Task<Turno> UpdateAsync(Turno turno)
    {
        db.Turnos.Update(turno);
        await db.SaveChangesAsync();
        return turno;
    }

    public async Task DeleteAsync(Guid id)
    {
        var t = await db.Turnos.FindAsync(id);
        if (t is not null) { db.Turnos.Remove(t); await db.SaveChangesAsync(); }
    }
}

public class ReservaRepository(PadelDbContext db) : IReservaRepository
{
    public Task<Reserva?> GetByTokenAsync(string token) =>
        db.Reservas
          .Include(r => r.Alumno)
          .Include(r => r.Turno)
          .FirstOrDefaultAsync(r => r.TokenCancelacion == token);

    public Task<Reserva?> GetByIdAsync(Guid id) =>
        db.Reservas.Include(r => r.Alumno).Include(r => r.Turno).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Reserva>> GetByTurnoFechaAsync(Guid turnoId, DateOnly fecha) =>
        await db.Reservas
            .Where(r => r.TurnoId == turnoId && r.FechaClase == fecha && r.Estado == EstadoReserva.Confirmada)
            .ToListAsync();

    public async Task<IEnumerable<Reserva>> GetConfirmadasHoyAsync(DateOnly fecha) =>
        await db.Reservas
            .Include(r => r.Alumno)
            .Include(r => r.Turno)
            .Where(r => r.FechaClase == fecha && r.Estado == EstadoReserva.Confirmada)
            .ToListAsync();

    public Task<int> CountConfirmadasAsync(Guid turnoId, DateOnly fecha) =>
        db.Reservas.CountAsync(r =>
            r.TurnoId == turnoId &&
            r.FechaClase == fecha &&
            r.Estado == EstadoReserva.Confirmada);

    public Task<bool> ExisteReservaAsync(Guid alumnoId, Guid turnoId, DateOnly fecha) =>
        db.Reservas.AnyAsync(r =>
            r.AlumnoId == alumnoId &&
            r.TurnoId == turnoId &&
            r.FechaClase == fecha &&
            r.Estado == EstadoReserva.Confirmada);

    public async Task<IEnumerable<Reserva>> GetPendientesRecordatorioAsync(
        DateOnly fecha, TimeOnly horaDesde, TimeOnly horaHasta) =>
        await db.Reservas
            .Include(r => r.Alumno)
            .Include(r => r.Turno)
            .Where(r =>
                r.FechaClase == fecha &&
                r.Estado == EstadoReserva.Confirmada &&
                !r.RecordatorioEnviado &&
                r.Turno.Hora >= horaDesde &&
                r.Turno.Hora <= horaHasta)
            .ToListAsync();

    public async Task<Reserva> CreateAsync(Reserva r)
    {
        db.Reservas.Add(r);
        await db.SaveChangesAsync();
        return r;
    }

    public async Task<Reserva> UpdateAsync(Reserva r)
    {
        db.Reservas.Update(r);
        await db.SaveChangesAsync();
        return r;
    }
}

public class NotifLogRepository(PadelDbContext db) : INotifLogRepository
{
    public async Task CreateAsync(NotifLog log)
    {
        db.NotifLogs.Add(log);
        await db.SaveChangesAsync();
    }
}
