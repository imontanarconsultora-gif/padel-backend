namespace PadelApi.Domain.Entities;

public class NotifLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;
    public TipoNotif Tipo { get; set; }
    public string Estado { get; set; } = "Enviado";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

public enum TipoNotif { Confirmacion, Cancelacion, Recordatorio, CupoLibre }
