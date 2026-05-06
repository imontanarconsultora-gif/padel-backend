using Microsoft.EntityFrameworkCore;
using PadelApi.Api.Middleware;
using PadelApi.Application.Interfaces;
using PadelApi.Application.Services;
using PadelApi.Domain.Interfaces;
using PadelApi.Infrastructure.Data;
using PadelApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── BASE DE DATOS ─────────────────────────────────────────────────
builder.Services.AddDbContext<PadelDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ── REPOSITORIOS ──────────────────────────────────────────────────
builder.Services.AddScoped<IAlumnoRepository,  AlumnoRepository>();
builder.Services.AddScoped<ITurnoRepository,   TurnoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<INotifLogRepository,NotifLogRepository>();

// ── SERVICIOS ─────────────────────────────────────────────────────
builder.Services.AddScoped<IReservaService,    ReservaService>();
builder.Services.AddScoped<IAlumnoService,     AlumnoService>();
builder.Services.AddScoped<ITurnoService,      TurnoService>();
builder.Services.AddScoped<IRecordatorioService, RecordatorioService>();

// ── WHATSAPP ──────────────────────────────────────────────────────
builder.Services.AddHttpClient<IWhatsAppService, CallMeBotWhatsAppService>();

// ── CORS (permite llamadas desde Netlify y admin.html) ────────────
builder.Services.AddCors(opt => opt.AddPolicy("Frontend", p =>
    p.WithOrigins(
        builder.Configuration["App:FrontendUrl"] ?? "*",
        "http://localhost:3000"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()));

// ── SWAGGER ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Pádel API", Version = "v1", Description = "API para gestión de turnos de pádel — IMontanar" });
});

builder.Services.AddControllers();

var app = builder.Build();

// ── MIGRACIONES AUTOMÁTICAS AL INICIAR ────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PadelDbContext>();
    await db.Database.MigrateAsync();
}

// ── PIPELINE ──────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");

// Swagger disponible siempre (podés restringirlo después)
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
