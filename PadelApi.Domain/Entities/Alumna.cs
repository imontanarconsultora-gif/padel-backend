namespace PadelApi.Domain.Entities;

public class Alumno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;   // 549XXXXXXXXXX — identificador único
    public string Categoria { get; set; } = string.Empty;  // "8va Principiante", "7ma Avanzada", etc.
    public bool Activa { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
