using PadelApi.Application.DTOs;

namespace PadelApi.Application.Interfaces;

public interface IReservaService
{
    Task<ReservaResponse> ReservarAsync(ReservarRequest request);
    Task<VerificarCancelacionResponse> VerificarCancelacionAsync(string token);
    Task<ApiResponse> CancelarAsync(string token);
    Task<IEnumerable<ReservaDto>> GetByFechaAsync(DateOnly fecha);
}

public interface IAlumnoService
{
    Task<IEnumerable<AlumnoDto>> GetAllAsync();
    Task<AlumnoDto?> GetByIdAsync(Guid id);
    Task<AlumnoDto> CreateAsync(CrearAlumnoRequest request);
    Task<AlumnoDto> UpdateAsync(Guid id, ActualizarAlumnoRequest request);
    Task DeleteAsync(Guid id);
}

public interface ITurnoService
{
    Task<IEnumerable<TurnoDto>> GetAllAsync(DateOnly? fechaParaCupos = null);
    Task<TurnoDto?> GetByIdAsync(Guid id);
    Task<TurnoDto> CreateAsync(CrearTurnoRequest request);
    Task<TurnoDto> UpdateAsync(Guid id, ActualizarTurnoRequest request);
    Task DeleteAsync(Guid id);
}

public interface IRecordatorioService
{
    Task ProcesarRecordatoriosAsync();
}

public interface IWhatsAppService
{
    Task EnviarConfirmacionAsync(string telefono, string nombre, DateOnly fecha, TimeOnly hora, string categoria, string tokenCancelacion);
    Task EnviarCancelacionAsync(string telefono, string nombre, DateOnly fecha);
    Task EnviarRecordatorioAsync(string telefono, string nombre, DateOnly fecha, TimeOnly hora, string categoria, string tokenCancelacion);
    Task EnviarCupoLibreAsync(string telefono, string nombre, Guid turnoId, DateOnly fecha, string categoria);
}
