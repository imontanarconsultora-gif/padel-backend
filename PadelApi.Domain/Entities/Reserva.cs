namespace PadelApi.Domain.Entities;

public class Reserva
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;

    public Guid TurnoId { get; set; }
    public Turno Turno { get; set; } = null!;

    public DateOnly FechaClase { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Confirmada;
    public string TokenCancelacion { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;
    public bool RecordatorioEnviado { get; set; } = false;
}

public enum EstadoReserva { Confirmada, Cancelada, Ausente }
