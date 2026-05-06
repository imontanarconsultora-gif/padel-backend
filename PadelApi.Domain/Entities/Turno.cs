namespace PadelApi.Domain.Entities;

public class Turno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;     // "Lunes 18:00 Avanzada"
    public int DiaSemana { get; set; }                     // 0=Dom, 1=Lun … 6=Sáb
    public TimeOnly Hora { get; set; }                     // 18:00
    public string Categoria { get; set; } = string.Empty;
    public int MaxAlumnos { get; set; } = 4;
    public bool Activo { get; set; } = true;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
