using PadelApi.Application.DTOs;
using PadelApi.Application.Interfaces;
using PadelApi.Domain.Entities;
using PadelApi.Domain.Exceptions;
using PadelApi.Domain.Interfaces;

namespace PadelApi.Application.Services;

public class AlumnoService(IAlumnoRepository repo) : IAlumnoService
{
    public async Task<IEnumerable<AlumnoDto>> GetAllAsync() =>
        (await repo.GetAllAsync()).Select(ToDto);

    public async Task<AlumnoDto?> GetByIdAsync(Guid id)
    {
        var a = await repo.GetByIdAsync(id);
        return a is null ? null : ToDto(a);
    }

    public async Task<AlumnoDto> CreateAsync(CrearAlumnoRequest req)
    {
        var tel = NormalizarTel(req.Telefono);
        var existente = await repo.GetByTelefonoAsync(tel);
        if (existente is not null)
            throw new PadelException("El teléfono ya está registrado.", "tel_duplicado", 409);

        var alumno = new Alumno
        {
            Nombre = req.Nombre.Trim(),
            Telefono = tel,
            Categoria = req.Categoria,
            Activa = req.Activa
        };
        return ToDto(await repo.CreateAsync(alumno));
    }

    public async Task<AlumnoDto> UpdateAsync(Guid id, ActualizarAlumnoRequest req)
    {
        var alumno = await repo.GetByIdAsync(id)
            ?? throw new PadelException("Alumno no encontrado.", "not_found", 404);

        var tel = NormalizarTel(req.Telefono);
        var conMismoTel = await repo.GetByTelefonoAsync(tel);
        if (conMismoTel is not null && conMismoTel.Id != id)
            throw new PadelException("El teléfono ya está registrado en otro alumno.", "tel_duplicado", 409);

        alumno.Nombre = req.Nombre.Trim();
        alumno.Telefono = tel;
        alumno.Categoria = req.Categoria;
        alumno.Activa = req.Activa;

        return ToDto(await repo.UpdateAsync(alumno));
    }

    public async Task DeleteAsync(Guid id)
    {
        _ = await repo.GetByIdAsync(id)
            ?? throw new PadelException("Alumno no encontrado.", "not_found", 404);
        await repo.DeleteAsync(id);
    }

    private static AlumnoDto ToDto(Alumno a) =>
        new(a.Id, a.Nombre, a.Telefono, a.Categoria, a.Activa, a.FechaAlta);

    private static string NormalizarTel(string tel)
    {
        tel = new string(tel.Where(char.IsDigit).ToArray());
        if (tel.StartsWith("549")) return tel;
        if (tel.StartsWith("0")) tel = tel[1..];
        return "549" + tel;
    }
}

public class TurnoService(ITurnoRepository turnoRepo, IReservaRepository reservaRepo) : ITurnoService
{
    public async Task<IEnumerable<TurnoDto>> GetAllAsync(DateOnly? fechaParaCupos = null)
    {
        var turnos = await turnoRepo.GetAllAsync();
        var fecha = fechaParaCupos ?? DateOnly.FromDateTime(DateTime.Today);
        var dtos = new List<TurnoDto>();

        foreach (var t in turnos)
        {
            var ocupadas = await reservaRepo.CountConfirmadasAsync(t.Id, fecha);
            dtos.Add(ToDto(t, t.MaxAlumnos - (int)ocupadas));
        }
        return dtos;
    }

    public async Task<TurnoDto?> GetByIdAsync(Guid id)
    {
        var t = await turnoRepo.GetByIdAsync(id);
        if (t is null) return null;
        var ocupadas = await reservaRepo.CountConfirmadasAsync(t.Id, DateOnly.FromDateTime(DateTime.Today));
        return ToDto(t, t.MaxAlumnos - (int)ocupadas);
    }

    public async Task<TurnoDto> CreateAsync(CrearTurnoRequest req)
    {
        var turno = new Turno
        {
            Nombre = req.Nombre.Trim(),
            DiaSemana = req.DiaSemana,
            Hora = req.Hora,
            Categoria = req.Categoria,
            Genero = req.Genero,
            MaxAlumnos = req.MaxAlumnos,
            Activo = req.Activo
        };
        return ToDto(await turnoRepo.CreateAsync(turno), turno.MaxAlumnos);
    }

    public async Task<TurnoDto> UpdateAsync(Guid id, ActualizarTurnoRequest req)
    {
        var turno = await turnoRepo.GetByIdAsync(id)
            ?? throw new PadelException("Turno no encontrado.", "not_found", 404);

        turno.Nombre = req.Nombre.Trim();
        turno.DiaSemana = req.DiaSemana;
        turno.Hora = req.Hora;
        turno.Categoria = req.Categoria;
        turno.Genero = req.Genero;
        turno.MaxAlumnos = req.MaxAlumnos;
        turno.Activo = req.Activo;

        return ToDto(await turnoRepo.UpdateAsync(turno), turno.MaxAlumnos);
    }

    public async Task DeleteAsync(Guid id)
    {
        _ = await turnoRepo.GetByIdAsync(id)
            ?? throw new PadelException("Turno no encontrado.", "not_found", 404);
        await turnoRepo.DeleteAsync(id);
    }

    private static TurnoDto ToDto(Turno t, int cuposLibres) =>
        new(t.Id, t.Nombre, t.DiaSemana, t.Hora, t.Categoria, t.Genero, t.MaxAlumnos, t.Activo, cuposLibres);
}
