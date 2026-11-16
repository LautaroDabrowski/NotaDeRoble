# Changelog

Todas las versiones relevantes de InstitutoBlazor.

## [0.3.0] — 2026-11-16

### Agregado

- Validación de servidor con FluentValidation y expresiones regulares para DNI y teléfono.
- Paginación, búsqueda y filtro por departamento ejecutados en SQL Server.
- Componentes `Paginacion`, `ThemeToggle`, `CopyLink` y `ServerValidation`.
- Modo oscuro persistente y diseño responsive accesible (375 px, navegación por teclado).
- Health check `/health` con verificación real de esquema y conexión.
- Migraciones EF Core: esquema inicial y datos de demostración deterministas.
- Pruebas de persistencia en SQLite y SQL Server (7 casos).
- Prueba E2E con Playwright y capturas en `docs/capturas/`.
- CI con SQL Server temporal, artefacto de publicación y script SQL idempotente.
- Despliegue en Ubuntu Server con systemd, Nginx, Docker y runner self-hosted.
- Documentación de base de datos, despliegue, flujo Git/GitHub y presentación.

### Cambiado

- `EmpleadoService` ahora depende de `IDbContextFactory` y devuelve resultados tipados.
- Sesión persistente con expiración de 8 horas y restauración después del primer render.
- Cadena de conexión fuera del repositorio (user secrets o variables de entorno).
- Proyecto actualizado a .NET 10 y EF Core 10.

### Seguridad

- Índices únicos de DNI, email y nombre de usuario.
- Hash de contraseñas con `PasswordHasher<Usuario>` y rehash automático.
- SQL Server y Kestrel accesibles solo por loopback detrás de Nginx.

## [0.2.0] — Versión anterior

- Migración inicial de PHP/MySQL a Blazor Web App con SQL Server.
- CRUD de empleados, registro, login y sesión.
- Componentes reutilizables de formulario, alertas y confirmación.

## [0.1.0] — Versión inicial

- Estructura base de Blazor Web App con .NET y Entity Framework Core.
- Modelo de empleados con departamento y localidad.
- Primeras pantallas de listado, alta y edición.
- Configuración inicial del proyecto y script SQL de referencia.
