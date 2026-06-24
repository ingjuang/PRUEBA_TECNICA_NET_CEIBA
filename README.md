# EventosVivos - Backend API

Sistema de gestión de eventos culturales y reservas desarrollado con .NET 10 y PostgreSQL.

## Arquitectura

Se implementó una **arquitectura por capas (N-Layer)** con separación de responsabilidades combinada con el patrón **CQRS (Command Query Responsibility Segregation)** a través de MediatR.

```
EventosVivosBackNet.Api            → Capa de presentación (Controllers, Middleware)
EventosVivosBackNet.Application    → Capa de aplicación (Commands, Queries, Handlers, DTOs, Interfaces)
EventosVivosBackNet.Domain         → Capa de dominio (Entidades)
EventosVivosBackNet.Infrastructure → Capa de infraestructura (DbContext, Repositorios, Unit of Work)
EventosVivosBackNet.UnitTests      → Pruebas unitarias (xUnit, Moq, FluentAssertions)
EventosVivosBackNet.IntegrationTests → Pruebas de integración (WebApplicationFactory, Testcontainers)
```

### Justificación de la arquitectura

- **N-Layer**: Permite una separación clara entre la lógica de negocio, acceso a datos y presentación. Cada capa tiene una responsabilidad definida y solo depende de la capa inmediatamente inferior, facilitando el mantenimiento y la testabilidad.

- **CQRS con MediatR**: Separa las operaciones de lectura (Queries) de las de escritura (Commands), lo que permite escalar y mantener cada flujo de forma independiente. MediatR actúa como mediador desacoplando los Controllers de los Handlers, eliminando dependencias directas entre capas.

- **Unit of Work + Repository**: Centraliza el acceso a datos y garantiza consistencia transaccional. El Unit of Work agrupa las operaciones de repositorio bajo una única transacción controlada por `SaveChangesAsync`.

- **Inyección de dependencias**: Todas las dependencias se resuelven a través del contenedor de DI de .NET, facilitando el testing con mocks y la sustitución de implementaciones.

## Tecnologías utilizadas

| Tecnología | Versión | Propósito |
|---|---|---|
| .NET | 10.0 | Framework principal |
| Entity Framework Core | 10.0.9 | ORM para acceso a datos |
| Npgsql | 10.0.2 | Proveedor de PostgreSQL para EF Core |
| MediatR | 12.4.1 | Implementación de CQRS |
| Swashbuckle | 10.1.7 | Documentación Swagger/OpenAPI |
| xUnit | 2.9.3 | Framework de pruebas |
| Moq | 4.20.72 | Mocking para pruebas unitarias |
| FluentAssertions | 8.10.0 | Assertions legibles en pruebas |
| Testcontainers | 4.12.0 | Contenedores Docker para pruebas de integración |
| PostgreSQL | 16+ | Base de datos relacional |

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para pruebas de integración)
- PostgreSQL 16+ (o usar la instancia de Supabase configurada)

## Instrucciones para ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd EventosVivosBackNet
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar la base de datos

La conexión a la base de datos se configura mediante la variable de entorno `DbConnection` en el archivo `EventosVivosBackNet.Api/Properties/launchSettings.json`. Ya viene preconfigurada con una instancia de Supabase.

Si deseas usar tu propia base de datos PostgreSQL, modifica el valor de `DbConnection`:

```
Host=tu-host;Database=tu-db;Username=tu-usuario;Password=tu-password;SSL Mode=Require;Trust Server Certificate=true
```

### 4. Ejecutar el seed de datos

Ejecuta el script SQL ubicado en la documentación del proyecto en tu base de datos PostgreSQL para insertar los venues de referencia y datos de prueba.

### 5. Ejecutar el proyecto

```bash
cd EventosVivosBackNet.Api
dotnet run
```

El API estará disponible en:
- **HTTP**: http://localhost:5191
- **HTTPS**: https://localhost:7127
- **Swagger UI**: Se abre automáticamente en el navegador al iniciar

## Ejecutar pruebas

### Pruebas unitarias

No requieren infraestructura externa. Usan Moq para simular las dependencias.

```bash
dotnet test EventosVivosBackNet.UnitTests --verbosity normal
```

### Pruebas de integración

Requieren **Docker Desktop** en ejecución. Testcontainers levanta automáticamente un contenedor PostgreSQL temporal para cada ejecución.

**Paso a paso:**

1. **Instalar Docker Desktop** desde https://www.docker.com/products/docker-desktop/

2. **Iniciar Docker Desktop** y esperar a que el engine esté en estado "running" (icono verde en la bandeja del sistema)

3. **Verificar que Docker funciona** ejecutando en terminal:
   ```bash
   docker info
   ```
   Debes ver información del engine sin errores.

4. **Ejecutar las pruebas**:
   ```bash
   dotnet test EventosVivosBackNet.IntegrationTests --verbosity normal
   ```

   La primera ejecución descargará la imagen `postgres:16-alpine` (~80MB), las siguientes serán más rápidas.

5. **¿Qué sucede internamente?**
   - Testcontainers crea un contenedor PostgreSQL efímero
   - `WebApplicationFactory` levanta el API en memoria apuntando a ese contenedor
   - El fixture crea el esquema con `EnsureCreatedAsync()` y ejecuta el seed de venues
   - Cada test clase comparte la misma instancia del contenedor (via `IClassFixture`)
   - Al finalizar, el contenedor se destruye automáticamente

### Ejecutar todas las pruebas

```bash
dotnet test --verbosity normal
```

## Endpoints de la API

| Método | Ruta | Descripción | RF |
|---|---|---|---|
| `POST` | `/api/events` | Crear evento | RF-01 |
| `GET` | `/api/events` | Listar eventos con filtros | RF-02 |
| `GET` | `/api/events/{id}/occupancy` | Reporte de ocupación | RF-06 |
| `POST` | `/api/reservations` | Reservar entradas | RF-03 |
| `PATCH` | `/api/reservations/{id}/confirm` | Confirmar pago | RF-04 |
| `PATCH` | `/api/reservations/{id}/cancel` | Cancelar reserva | RF-05 |
| `GET` | `/api/venues` | Listar venues | Referencia |
| `GET` | `/api/health` | Estado del servicio | — |

### Filtros disponibles en `GET /api/events`

| Parámetro | Tipo | Descripción |
|---|---|---|
| `eventType` | string | Filtrar por tipo: `conferencia`, `taller`, `concierto` |
| `startDateFrom` | DateTime | Fecha de inicio desde |
| `startDateTo` | DateTime | Fecha de inicio hasta |
| `venueId` | long | Filtrar por venue |
| `status` | string | Filtrar por estado: `activo`, `cancelado`, `completado` |
| `titleSearch` | string | Búsqueda parcial por título (case-insensitive) |

## Reglas de negocio implementadas

| ID | Regla | Implementación |
|---|---|---|
| RN-01 | Capacidad del venue | `CreateEventCommandHandler` |
| RN-02 | Superposición de venues | `CreateEventCommandHandler` + `ExistsOverlappingEventAsync` |
| RN-03 | Restricción horario nocturno | `CreateEventCommandHandler` |
| RN-04 | Restricción reserva tardía (<1h) | `CreateReservationCommandHandler` |
| RN-05 | Límite entradas por transacción (precio >$100) | `CreateReservationCommandHandler` |
| RN-06 | Estado automático completado | `CreateReservationCommandHandler` + `GetOccupancyReportQueryHandler` |
| RN-07 | Cancelación con penalización (<48h) | `CancelReservationCommandHandler` |

## Cobertura de pruebas

- **41 pruebas unitarias**: Cubren todos los handlers (Commands y Queries), validaciones de entrada, y cada regla de negocio
- **14 pruebas de integración**: Cubren los flujos completos end-to-end a través de los endpoints HTTP con base de datos real
