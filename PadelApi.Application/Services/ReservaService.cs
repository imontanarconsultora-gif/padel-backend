using Microsoft.Extensions.Configuration;
using PadelApi.Application.DTOs;
using PadelApi.Application.Interfaces;
using PadelApi.Domain.Entities;
using PadelApi.Domain.Exceptions;
using PadelApi.Domain.Interfaces;

namespace PadelApi.Application.Services;

public class ReservaService(
    IAlumnoRepository alumnoRepo,
    ITurnoRepository turnoRepo,
    IReservaRepository reservaRepo,
    INotifLogRepository notifRepo,
    IWhatsAppService whatsApp,
    IConfiguration config) : IReservaService
{
    private readonly int _minutosMinCancelacion =
        config.GetValue<int>("Padel:MinutosMinCancelacion", 120);

    public async Task<ReservaResponse> ReservarAsync(ReservarRequest req)
    {
        // 1. Validar teléfono
        var telNorm = NormalizarTel(req.Telefono);
        if (!ValidarTel(telNorm))
            throw new PadelException("Número de teléfono inválido.", "tel_invalido");

        // 2. Validar fecha
        if (req.FechaClase < DateOnly.FromDateTime(DateTime.Today))
            throw new PadelException("La fecha ya pasó.", "fecha_pasada");

        // 3. Buscar alumno
        var alumno = await alumnoRepo.GetByTelefonoAsync(telNorm)
            ?? throw new AlumnoNoRegistradoException();

        if (!alumno.Activa) throw new AlumnoInactivoException();

        // 4. Buscar turno
        var turno = await turnoRepo.GetByIdAsync(req.TurnoId)
            ?? throw new PadelException("Turno no encontrado.", "turno_invalido", 404);

        // 5. Verificar categoría
        if (turno.Categoria != alumno.Categoria)
            throw new CategoriaIncompatibleException(turno.Categoria, alumno.Categoria);

        // 6. Verificar que no tenga reserva ya
        if (await reservaRepo.ExisteReservaAsync(alumno.Id, turno.Id, req.FechaClase))
            throw new YaReservadaException();

        // 7. Verificar cupo
        var ocupadas = await reservaRepo.CountConfirmadasAsync(turno.Id, req.FechaClase);
        if (ocupadas >= turno.MaxAlumnos) throw new TurnoCompletoException();

        // 8. Crear reserva
        var reserva = new Reserva
        {
            AlumnoId = alumno.Id,
            TurnoId = turno.Id,
            FechaClase = req.FechaClase,
            Estado = EstadoReserva.Confirmada,
            TokenCancelacion = Guid.NewGuid().ToString("N")
        };

        await reservaRepo.CreateAsync(reserva);

        // 9. Enviar WhatsApp (fire & forget — no bloqueamos la respuesta)
        _ = Task.Run(async () =>
        {
            try
            {
                await whatsApp.EnviarConfirmacionAsync(
                    alumno.Telefono, alumno.Nombre,
                    reserva.FechaClase, turno.Hora,
                    turno.Categoria, reserva.TokenCancelacion);

                await notifRepo.CreateAsync(new NotifLog
                {
                    AlumnoId = alumno.Id,
                    Tipo = TipoNotif.Confirmacion,
                    Estado = "Enviado"
                });
            }
            catch { /* loggear en producción */ }
        });

        return new ReservaResponse(true, "Reserva confirmada. Te enviamos un mensaje de WhatsApp.", reserva.Id);
    }

    public async Task<VerificarCancelacionResponse> VerificarCancelacionAsync(string token)
    {
        var reserva = await reservaRepo.GetByTokenAsync(token);

        if (reserva is null)
            return new VerificarCancelacionResponse(false, "invalido", null);

        if (reserva.Estado == EstadoReserva.Cancelada)
            return new VerificarCancelacionResponse(false, "ya_cancelada", null);

        var turno = reserva.Turno;
        var alumno = reserva.Alumno;
        var fechaHoraClase = reserva.FechaClase.ToDateTime(turno.Hora);
        var puedeCancelar = (fechaHoraClase - DateTime.UtcNow).TotalMinutes >= _minutosMinCancelacion;

        return new VerificarCancelacionResponse(true, null, new ReservaDetalle(
            token,
            alumno.Nombre,
            alumno.Telefono,
            turno.Nombre,
            turno.Hora.ToString("HH:mm"),
            reserva.FechaClase,
            turno.Categoria,
            reserva.Estado.ToString(),
            puedeCancelar
        ));
    }

    public async Task<ApiResponse> CancelarAsync(string token)
    {
        var reserva = await reservaRepo.GetByTokenAsync(token)
            ?? throw new ReservaNoEncontradaException();

        if (reserva.Estado == EstadoReserva.Cancelada)
            throw new ReservaYaCanceladaException();

        var turno = reserva.Turno;
        var alumno = reserva.Alumno;
        var fechaHoraClase = reserva.FechaClase.ToDateTime(turno.Hora);

        if ((fechaHoraClase - DateTime.UtcNow).TotalMinutes < _minutosMinCancelacion)
            throw new CancelacionFueraDeTiempoException();

        // Marcar cancelada
        reserva.Estado = EstadoReserva.Cancelada;
        await reservaRepo.UpdateAsync(reserva);

        // Notificaciones async
        _ = Task.Run(async () =>
        {
            try
            {
                // WA confirmación cancelación
                await whatsApp.EnviarCancelacionAsync(alumno.Telefono, alumno.Nombre, reserva.FechaClase);
                await notifRepo.CreateAsync(new NotifLog { AlumnoId = alumno.Id, Tipo = TipoNotif.Cancelacion, Estado = "Enviado" });

                // Notificar cupo libre a alumnos de la misma categoría
                var candidatos = await alumnoRepo.GetByCategoriaAsync(turno.Categoria);
                var yaReservadas = await reservaRepo.GetByTurnoFechaAsync(turno.Id, reserva.FechaClase);
                var idsReservadas = yaReservadas.Select(r => r.AlumnoId).ToHashSet();

                foreach (var candidato in candidatos.Where(a => a.Activa && !idsReservadas.Contains(a.Id)))
                {
                    await whatsApp.EnviarCupoLibreAsync(
                        candidato.Telefono, candidato.Nombre,
                        turno.Id, reserva.FechaClase, turno.Categoria);

                    await notifRepo.CreateAsync(new NotifLog { AlumnoId = candidato.Id, Tipo = TipoNotif.CupoLibre, Estado = "Enviado" });
                }
            }
            catch { }
        });

        return new ApiResponse(true, "Turno cancelado correctamente.");
    }

    public async Task<IEnumerable<ReservaDto>> GetByFechaAsync(DateOnly fecha)
    {
        var reservas = await reservaRepo.GetConfirmadasHoyAsync(fecha);
        return reservas.Select(r => new ReservaDto(
            r.Id, r.Alumno.Nombre, r.Alumno.Telefono,
            r.Turno.Nombre, r.FechaClase,
            r.Estado.ToString(), r.RecordatorioEnviado, r.FechaReserva));
    }

    private static string NormalizarTel(string tel)
    {
        tel = new string(tel.Where(char.IsDigit).ToArray());
        if (tel.StartsWith("549")) return tel;
        if (tel.StartsWith("0")) tel = tel[1..];
        return "549" + tel;
    }

    private static bool ValidarTel(string tel) =>
        System.Text.RegularExpressions.Regex.IsMatch(tel, @"^549\d{10}$");
}
