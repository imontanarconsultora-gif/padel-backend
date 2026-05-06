# PadelApi — Backend propio para gestión de turnos
## Desarrollado por IMontanar · Sunchales, Santa Fe

---

## Estructura del proyecto

```
PadelApi/
├── PadelApi.Domain/          # Entidades, interfaces, excepciones (sin dependencias)
│   ├── Entities/             # Alumna, Turno, Reserva, NotifLog
│   ├── Interfaces/           # IAlumnaRepository, ITurnoRepository, IReservaRepository...
│   └── Exceptions/           # PadelExceptions (errores de negocio tipados)
│
├── PadelApi.Application/     # Lógica de negocio, DTOs, servicios
│   ├── DTOs/                 # Requests y Responses
│   ├── Interfaces/           # IReservaService, IAlumnaService, IWhatsAppService...
│   └── Services/             # ReservaService, AlumnaService, TurnoService,
│                             # RecordatorioService, CallMeBotWhatsAppService
│
├── PadelApi.Infrastructure/  # EF Core + PostgreSQL
│   ├── Data/                 # PadelDbContext
│   └── Repositories/        # Implementaciones de repositorios
│
├── PadelApi.Api/             # Controllers REST + Middleware + Program.cs
│   ├── Controllers/         # ReservasController, AlumnasController,
│   │                        # TurnosController, RecordatoriosController
│   ├── Middleware/          # ExceptionMiddleware (manejo global de errores)
│   ├── Program.cs           # DI, pipeline, migraciones automáticas
│   └── appsettings.json     # Configuración
│
├── Dockerfile               # Para deploy en Railway/Render/VPS
├── docker-compose.yml       # Dev local con Postgres incluido
└── PadelApi.sln
```

---

## Levantar en desarrollo local

### Opción A — Docker (recomendado, sin instalar nada)

```bash
# Clonar / copiar el proyecto
cd PadelApi

# Levantar Postgres + API juntos
docker-compose up -d

# Ver logs
docker-compose logs -f api

# La API queda en: http://localhost:8080
# Swagger en:     http://localhost:8080/swagger
```

### Opción B — Sin Docker (necesitás .NET 8 SDK y Postgres instalado)

```bash
# 1. Crear base de datos en Postgres
psql -U postgres -c "CREATE DATABASE padel_db;"

# 2. Configurar connection string en appsettings.json
#    "Default": "Host=localhost;Port=5432;Database=padel_db;Username=postgres;Password=TU_PASS"

# 3. Instalar EF tools (una sola vez)
dotnet tool install --global dotnet-ef

# 4. Crear y aplicar migración inicial
dotnet ef migrations add InitialCreate \
  --project PadelApi.Infrastructure \
  --startup-project PadelApi.Api

dotnet ef database update \
  --project PadelApi.Infrastructure \
  --startup-project PadelApi.Api

# 5. Correr la API
cd PadelApi.Api
dotnet run

# Swagger en: https://localhost:5001/swagger
```

---

## Endpoints disponibles

### Reservas (usados por el frontend HTML)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/reservas` | Crear reserva |
| GET | `/api/reservas/verificar-cancelacion?token=XXX` | Verificar token antes de cancelar |
| POST | `/api/reservas/cancelar` | Confirmar cancelación |
| GET | `/api/reservas?fecha=2025-06-16` | Listar reservas por fecha (admin) |

### Alumnas (panel admin)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/alumnas` | Listar todas |
| GET | `/api/alumnas/{id}` | Obtener una |
| POST | `/api/alumnas` | Crear |
| PUT | `/api/alumnas/{id}` | Actualizar |
| DELETE | `/api/alumnas/{id}` | Eliminar |

### Turnos (panel admin)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/turnos?fecha=2025-06-16` | Listar con cupos calculados |
| GET | `/api/turnos/{id}` | Obtener uno |
| POST | `/api/turnos` | Crear |
| PUT | `/api/turnos/{id}` | Actualizar |
| DELETE | `/api/turnos/{id}` | Eliminar |

### Recordatorios (llamado por Cron)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/recordatorios/procesar` | Disparar proceso de recordatorios |

---

## Ejemplos de requests

### Crear reserva
```json
POST /api/reservas
{
  "telefono": "3415551234",
  "turnoId": "uuid-del-turno",
  "fechaClase": "2025-06-16"
}
```

### Crear alumna
```json
POST /api/alumnas
{
  "nombre": "María García",
  "telefono": "3415551234",
  "categoria": "8va Avanzada",
  "activa": true
}
```

### Crear turno
```json
POST /api/turnos
{
  "nombre": "Lunes 18:00 Avanzada",
  "diaSemana": 1,
  "hora": "18:00:00",
  "categoria": "8va Avanzada",
  "maxAlumnas": 4,
  "activo": true
}
```

---

## Deploy en Railway (gratis)

1. Subir el proyecto a GitHub
2. Railway → **New Project → Deploy from GitHub**
3. Seleccionar el repo
4. Agregar servicio PostgreSQL desde Railway (botón "+ New → Database → PostgreSQL")
5. En Variables del servicio API:
   ```
   ConnectionStrings__Default=postgresql://usuario:pass@host:port/padel_db
   CallMeBot__ApiKey=TU_APIKEY
   App__BaseUrl=https://TU-API.railway.app
   App__FrontendUrl=https://tuclub.netlify.app
   ```
6. Railway detecta el Dockerfile automáticamente y hace el deploy

---

## Conectar con n8n

En lugar de los nodos de Airtable, usar **HTTP Request** a esta API:

```
Nodo: HTTP Request
URL: https://TU-API.railway.app/api/reservas
Method: POST
Body (JSON): { "telefono": "...", "turnoId": "...", "fechaClase": "..." }
```

El flujo 3 (Recordatorios) simplifica a UN SOLO nodo:
```
Cron cada 30 min → HTTP Request POST /api/recordatorios/procesar
```
Toda la lógica de recordatorios vive en el backend, no en n8n.

---

## Configuración (appsettings.json)

| Clave | Descripción | Default |
|-------|-------------|---------|
| `ConnectionStrings:Default` | Connection string de Postgres | — |
| `CallMeBot:ApiKey` | Tu API key de CallMeBot | — |
| `App:BaseUrl` | URL pública de la API | — |
| `App:FrontendUrl` | URL del frontend en Netlify (para CORS) | — |
| `Padel:MinutosMinCancelacion` | Minutos mínimos para cancelar antes del turno | 120 |
