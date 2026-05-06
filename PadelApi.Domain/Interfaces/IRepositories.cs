using PadelApi.Domain.Entities;

namespace PadelApi.Domain.Interfaces;

public interface IAlumnoRepository
{
    Task<Alumno?> GetByTelefonoAsync(string telefono);
    Task<Alumno?> GetByIdAsync(Guid id);
    Task<IEnumerable<Alumno>> GetByCategoriaAsync(string categoria);
    Task<IEnumerable<Alumno>> GetAllAsync();
    Task<Alumno> CreateAsync(Alumno alumno);
    Task<Alumno> UpdateAsync(Alumno alumno);
    Task DeleteAsync(Guid id);
}

public interface ITurnoRepository
{
    Task<Turno?> GetByIdAsync(Guid id);
    Task<IEnumerable<Turno>> GetAllAsync();
    Task<IEnumerable<Turno>> GetActivosAsync();
    Task<Turno> CreateAsync(Turno turno);
    Task<Turno> UpdateAsync(Turno turno);
    Task DeleteAsync(Guid id);
}

public interface IReservaRepository
{
    Task<Reserva?> GetByTokenAsync(string token);
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetByTurnoFechaAsync(Guid turnoId, DateOnly fecha);
    Task<IEnumerable<Reserva>> GetConfirmadasHoyAsync(DateOnly fecha);
    Task<int> CountConfirmadasAsync(Guid turnoId, DateOnly fecha);
    Task<bool> ExisteReservaAsync(Guid alumnoId, Guid turnoId, DateOnly fecha);
    Task<IEnumerable<Reserva>> GetPendientesRecordatorioAsync(DateOnly fecha, TimeOnly horaDesde, TimeOnly horaHasta);
    Task<Reserva> CreateAsync(Reserva reserva);
    Task<Reserva> UpdateAsync(Reserva reserva);
}

public interface INotifLogRepository
{
    Task CreateAsync(NotifLog log);
}
