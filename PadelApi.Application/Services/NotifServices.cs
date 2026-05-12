using System.Globalization;
using Microsoft.Extensions.Configuration;
using PadelApi.Application.Interfaces;
using PadelApi.Domain.Entities;
using PadelApi.Domain.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace PadelApi.Application.Services;

public class RecordatorioService(
    IReservaRepository reservaRepo,
    INotifLogRepository notifRepo,
    IWhatsAppService whatsApp,
    IConfiguration config) : IRecordatorioService
{
    private readonly int _minutos = config.GetValue<int>("Padel:MinutosMinCancelacion", 120);

    public async Task ProcesarRecordatoriosAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoy = DateOnly.FromDateTime(ahora);

        var horaDesde = TimeOnly.FromDateTime(ahora.AddMinutes(_minutos - 15));
        var horaHasta = TimeOnly.FromDateTime(ahora.AddMinutes(_minutos + 15));

        var reservas = await reservaRepo.GetPendientesRecordatorioAsync(hoy, horaDesde, horaHasta);

        foreach (var r in reservas)
        {
            try
            {
                await whatsApp.EnviarRecordatorioAsync(
                    r.Alumno.Telefono, r.Alumno.Nombre,
                    r.FechaClase, r.Turno.Hora,
                    r.Turno.Categoria, r.TokenCancelacion);

                r.RecordatorioEnviado = true;
                await reservaRepo.UpdateAsync(r);

                await notifRepo.CreateAsync(new NotifLog
                {
                    AlumnoId = r.AlumnoId,
                    Tipo = TipoNotif.Recordatorio,
                    Estado = "Enviado"
                });
            }
            catch (Exception ex)
            {
                await notifRepo.CreateAsync(new NotifLog
                {
                    AlumnoId = r.AlumnoId,
                    Tipo = TipoNotif.Recordatorio,
                    Estado = $"Error: {ex.Message[..Math.Min(50, ex.Message.Length)]}"
                });
            }
        }
    }
}

// ── WhatsApp via Twilio ───────────────────────────────────────────
public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly string _from;
    private readonly string _baseUrl;

    public TwilioWhatsAppService(IConfiguration config)
    {
        var accountSid = config["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid no configurado.");
        var authToken  = config["Twilio:AuthToken"]  ?? throw new InvalidOperationException("Twilio:AuthToken no configurado.");
        _from    = config["Twilio:FromNumber"] ?? "whatsapp:+14155238886";
        _baseUrl = config["App:BaseUrl"] ?? "";
        TwilioClient.Init(accountSid, authToken);
    }

    public Task EnviarConfirmacionAsync(string tel, string nombre, DateOnly fecha,
        TimeOnly hora, string categoria, string token)
    {
        var fechaStr = fecha.ToString("dddd d 'de' MMMM", new CultureInfo("es-AR"));
        var link = $"{_baseUrl}/cancelar.html?token={token}";
        var msg = $"Hola {nombre}! ✅\n\nTu turno de pádel quedó confirmado:\n📅 {fechaStr}\n⏰ {hora:HH:mm}\n🏷 Categoría: {categoria}\n\nPara cancelar (hasta 2hs antes):\n🔗 {link}\n\n¡Nos vemos en la cancha! 🎾";
        return EnviarAsync(tel, msg);
    }

    public Task EnviarCancelacionAsync(string tel, string nombre, DateOnly fecha)
    {
        var fechaStr = fecha.ToString("dddd d 'de' MMMM", new CultureInfo("es-AR"));
        var msg = $"Hola {nombre}!\n\nTu turno fue cancelado correctamente:\n📅 {fechaStr}\n\nEsperamos verte en el próximo turno! 🎾";
        return EnviarAsync(tel, msg);
    }

    public Task EnviarRecordatorioAsync(string tel, string nombre, DateOnly fecha,
        TimeOnly hora, string categoria, string token)
    {
        var fechaStr = fecha.ToString("d 'de' MMMM", new CultureInfo("es-AR"));
        var link = $"{_baseUrl}/cancelar.html?token={token}";
        var msg = $"¡Hola {nombre}! ⏰\n\nTe recordamos que en 2 horas tenés clase de pádel:\n📅 Hoy {fechaStr}\n⏰ {hora:HH:mm}\n🏷 {categoria}\n\n¿No podés venir? Cancelá acá:\n🔗 {link}\n\n¡Hasta pronto! 🎾";
        return EnviarAsync(tel, msg);
    }

    public Task EnviarCupoLibreAsync(string tel, string nombre, Guid turnoId,
        DateOnly fecha, string categoria)
    {
        var fechaStr = fecha.ToString("dddd d 'de' MMMM", new CultureInfo("es-AR"));
        var link = $"{_baseUrl}/reservas.html?turno={turnoId}&fecha={fecha:yyyy-MM-dd}&tel={tel}";
        var msg = $"¡Hola {nombre}! 🎾\n\nQuedó un cupo libre en:\n📅 {fechaStr}\n🏷 Categoría: {categoria}\n\n¿Querés sumarte? Reservá acá:\n🔗 {link}\n\n¡Primero en llegar, primero en jugar! ⚡";
        return EnviarAsync(tel, msg);
    }

    private async Task EnviarAsync(string tel, string mensaje)
    {
        // tel tiene formato 549XXXXXXXXXX → Twilio necesita whatsapp:+549XXXXXXXXXX
        var to = new PhoneNumber($"whatsapp:+{tel}");
        await MessageResource.CreateAsync(
            body: mensaje,
            from: new PhoneNumber(_from),
            to: to);
    }
}
