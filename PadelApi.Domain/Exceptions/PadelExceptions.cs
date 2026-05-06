namespace PadelApi.Domain.Exceptions;

public class PadelException : Exception
{
    public string Codigo { get; }
    public int StatusCode { get; }

    public PadelException(string mensaje, string codigo, int statusCode = 400)
        : base(mensaje)
    {
        Codigo = codigo;
        StatusCode = statusCode;
    }
}

public class AlumnoNoRegistradoException()
    : PadelException("Tu número no está registrado en el club. Hablá con la profe.", "no_registrado", 404);

public class AlumnoInactivoException()
    : PadelException("Tu cuenta está inactiva. Contactá al club.", "inactivo", 403);

public class TurnoCompletoException()
    : PadelException("Lo sentimos, el turno ya está completo.", "completo", 409);

public class YaReservadaException()
    : PadelException("Ya tenés una reserva para este turno en esa fecha.", "ya_reservada", 409);

public class CategoriaIncompatibleException(string turno, string alumno)
    : PadelException($"Este turno es para categoría {turno}. Tu categoría es {alumno}.", "categoria_incorrecta", 409);

public class ReservaNoEncontradaException()
    : PadelException("Reserva no encontrada.", "invalido", 404);

public class ReservaYaCanceladaException()
    : PadelException("Esta reserva ya fue cancelada.", "ya_cancelada", 409);

public class CancelacionFueraDeTiempoException()
    : PadelException("Ya no se puede cancelar. El plazo mínimo es 2 horas antes del turno.", "fuera_de_tiempo", 409);
