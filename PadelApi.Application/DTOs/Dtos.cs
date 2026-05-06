namespace PadelApi.Application.DTOs;

// ── REQUESTS ──────────────────────────────────────────────────────
public record ReservarRequest(
    string Telefono,
    Guid TurnoId,
    DateOnly FechaClase
);

public record CancelarRequest(string Token);

public record CrearAlumnoRequest(
    string Nombre,
    string Telefono,
    string Categoria,
    bool Activa = true
);

public record ActualizarAlumnoRequest(
    string Nombre,
    string Telefono,
    string Categoria,
    bool Activa
);

public record CrearTurnoRequest(
    string Nombre,
    int DiaSemana,
    TimeOnly Hora,
    string Categoria,
    int MaxAlumnos = 4,
    bool Activo = true
);

public record ActualizarTurnoRequest(
    string Nombre,
    int DiaSemana,
    TimeOnly Hora,
    string Categoria,
    int MaxAlumnos,
    bool Activo
);

// ── RESPONSES ─────────────────────────────────────────────────────
public record ApiResponse(bool Ok, string Mensaje, string? Codigo = null);

public record ReservaResponse(
    bool Ok,
    string Mensaje,
    Guid? ReservaId = null
);

public record VerificarCancelacionResponse(
    bool Ok,
    string? Codigo,
    ReservaDetalle? Reserva
);

public record ReservaDetalle(
    string Token,
    string Alumno,
    string AlumnoTel,
    string Turno,
    string Hora,
    DateOnly FechaClase,
    string Categoria,
    string Estado,
    bool PuedeCancelar
);

public record AlumnoDto(
    Guid Id,
    string Nombre,
    string Telefono,
    string Categoria,
    bool Activa,
    DateTime FechaAlta
);

public record TurnoDto(
    Guid Id,
    string Nombre,
    int DiaSemana,
    TimeOnly Hora,
    string Categoria,
    int MaxAlumnos,
    bool Activo,
    int CuposLibres
);

public record ReservaDto(
    Guid Id,
    string AlumnoNombre,
    string AlumnoTel,
    string TurnoNombre,
    DateOnly FechaClase,
    string Estado,
    bool RecordatorioEnviado,
    DateTime FechaReserva
);
