using Microsoft.AspNetCore.Mvc;
using PadelApi.Application.DTOs;
using PadelApi.Application.Interfaces;

namespace PadelApi.Api.Controllers;

// ── RESERVAS ──────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class ReservasController(IReservaService svc) : ControllerBase
{
    /// <summary>Crear una nueva reserva. Llamado desde reservas.html</summary>
    [HttpPost]
    public async Task<IActionResult> Reservar([FromBody] ReservarRequest req)
    {
        var result = await svc.ReservarAsync(req);
        return Ok(result);
    }

    /// <summary>Verificar token antes de mostrar la página de cancelación</summary>
    [HttpGet("verificar-cancelacion")]
    public async Task<IActionResult> Verificar([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new ApiResponse(false, "Token requerido.", "invalido"));

        var result = await svc.VerificarCancelacionAsync(token);
        return Ok(result);
    }

    /// <summary>Confirmar cancelación. Llamado desde cancelar.html</summary>
    [HttpPost("cancelar")]
    public async Task<IActionResult> Cancelar([FromBody] CancelarRequest req)
    {
        var result = await svc.CancelarAsync(req.Token);
        return Ok(result);
    }

    /// <summary>Listar reservas por fecha (para el panel admin)</summary>
    [HttpGet]
    public async Task<IActionResult> GetByFecha([FromQuery] DateOnly? fecha)
    {
        var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await svc.GetByFechaAsync(f);
        return Ok(result);
    }
}

// ── ALUMNOS ───────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class AlumnosController(IAlumnoService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var a = await svc.GetByIdAsync(id);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearAlumnoRequest req)
    {
        var a = await svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, a);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarAlumnoRequest req)
    {
        var a = await svc.UpdateAsync(id, req);
        return Ok(a);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }
}

// ── TURNOS ────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class TurnosController(ITurnoService svc) : ControllerBase
{
    /// <summary>Listar todos los turnos. fecha opcional para calcular cupos</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? fecha) =>
        Ok(await svc.GetAllAsync(fecha));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await svc.GetByIdAsync(id);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearTurnoRequest req)
    {
        var t = await svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarTurnoRequest req)
    {
        var t = await svc.UpdateAsync(id, req);
        return Ok(t);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }
}

// ── RECORDATORIOS (llamado por Cron o n8n) ────────────────────────
[ApiController]
[Route("api/[controller]")]
public class RecordatoriosController(IRecordatorioService svc) : ControllerBase
{
    /// <summary>Procesar recordatorios — llamar cada 30 min desde n8n Cron o cron del SO</summary>
    [HttpPost("procesar")]
    public async Task<IActionResult> Procesar()
    {
        await svc.ProcesarRecordatoriosAsync();
        return Ok(new { ok = true, mensaje = "Recordatorios procesados." });
    }
}
