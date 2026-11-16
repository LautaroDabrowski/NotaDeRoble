# Base de datos

La fuente de verdad del esquema es **Entity Framework Core**. Las migraciones crean las tablas, índices y datos de demostración. `Database/01_InstitutoEmpresa.sql` se conserva como referencia del esquema PHP original, pero no hace falta ejecutarlo si se usan las migraciones.

## 1. Modelo

| Tabla | Clave | Campos principales | Restricciones |
|---|---|---|---|
| `departamento` | `id_Departamento` (identity) | `Nombre` | — |
| `localidad` | `CodPostal` | `Nombre`, `Provincia` | — |
| `empleados` | `id` (identity) | `dni`, `nombre`, `apellido`, `telefono`, `Direccion`, `id_Departamento`, `CodPostal` | índice único en `dni`; FK a `departamento` y `localidad` con `ON DELETE RESTRICT` |
| `usuario` | `id` (identity) | `nombre`, `email`, `contrasenia` | índices únicos en `nombre` y `email` |
| `__EFMigrationsHistory` | — | historial de migraciones | creada por EF |

`contrasenia` guarda el hash generado por `PasswordHasher<Usuario>`; nunca la contraseña en texto plano.

La migración `DatosDemostracion` inserta una sola vez:

- 5 departamentos: Ventas, Producción, Marketing, Administración, Gerencia.
- 5 localidades: 1865, 1984, 1980, 1804 y 1842.
- 12 empleados ficticios con DNI `90000001` a `90000012`.

No se crean usuarios de demostración: el primero se registra desde `/registro`. Las pruebas de navegador usan un usuario temporal.

## 2. Cadena de conexión

Nunca se versiona una contraseña. En desarrollo se usa **user secrets**:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=127.0.0.1,1433;Database=InstitutoBlazorDemo;User Id=sa;Password=CLAVE_LOCAL;Encrypt=True;TrustServerCertificate=True"
```

En producción se define `ConnectionStrings__DefaultConnection` en `/etc/institutoblazor/institutoblazor.env` (ver `docs/DESPLIEGUE.md`).

Para pruebas existe una variable independiente:

```bash
INSTITUTO_TEST_SQLSERVER="Server=127.0.0.1,14339;Database=master;User Id=sa;Password=CLAVE;TrustServerCertificate=True"
dotnet test InstitutoBlazor.sln -c Release
```

Sin esa variable, las pruebas usan SQLite en memoria. Con la variable, cada test crea una base `InstitutoBlazorTest_<guid>` en SQL Server, aplica migraciones y la elimina al terminar.

## 3. SQL Server local con contenedor

```bash
docker volume create instituto-sql
docker run -d --name instituto-sql \
  -e ACCEPT_EULA=Y -e MSSQL_PID=Express \
  -e MSSQL_SA_PASSWORD='ClaveLocal1!' \
  -p 127.0.0.1:1433:1433 \
  -v instituto-sql:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

Con Podman, reemplazar `docker` por `podman`. La contraseña de `sa` debe tener al menos 8 caracteres, mayúscula, minúscula, dígito y símbolo.

## 4. Migraciones

El proyecto incluye `dotnet-ef` fijado en `.config/dotnet-tools.json` y una factoría de diseño (`Data/AppDbContextDesignFactory.cs`) que lee la cadena de conexión o user secrets.

```bash
dotnet tool restore

# Crear una migración después de cambiar el modelo
dotnet ef migrations add NombreDelCambio --output-dir Data/Migrations

# Aplicar en desarrollo
dotnet ef database update

# Verificar que no queden cambios sin migrar (lo usa CI)
dotnet ef migrations has-pending-model-changes

# Generar script idempotente para producción
dotnet ef migrations script --idempotent -o migraciones.sql
```

En producción hay dos caminos:

1. **Recomendado:** ejecutar `migraciones.sql` con un usuario administrador y dejar `Database__ApplyMigrations=false`.
2. **Primer despliegue simple:** `Database__ApplyMigrations=true`. La aplicación ejecuta `MigrateAsync()` al arrancar. El login de la aplicación necesita permisos DDL mientras dure esa ejecución.

La aplicación **no** ejecuta migraciones si `ApplyMigrations` está en `false` o ausente. El servicio de salud `/health` consulta la tabla `empleados`, por lo que también detecta un esquema faltante.

## 5. Usuario de base de datos para la aplicación

Ejecutar en el contenedor como `sa`:

```sql
CREATE DATABASE InstitutoBlazorDemo;
GO
CREATE LOGIN instituto_app WITH PASSWORD = N'Cambiar_Esta_Clave_1!';
GO
USE InstitutoBlazorDemo;
GO
CREATE USER instituto_app FOR LOGIN instituto_app;
GO
ALTER ROLE db_datareader ADD MEMBER instituto_app;
ALTER ROLE db_datawriter ADD MEMBER instituto_app;
GO
```

Con ese usuario de mínimo privilegio, aplicá el esquema con `migraciones.sql` usando `sa` y mantené `Database__ApplyMigrations=false`.

Para la entrega del integrador se acepta usar `db_owner` durante el primer arranque y luego volver a `db_datareader` + `db_datawriter`:

```sql
ALTER ROLE db_owner ADD MEMBER instituto_app;
-- Primer arranque con Database__ApplyMigrations=true
ALTER ROLE db_owner DROP MEMBER instituto_app;
```

## 6. Copias de seguridad

Backup dentro del contenedor:

```bash
docker exec -u mssql instituto-sql mkdir -p /var/opt/mssql/backup
docker exec -u mssql instituto-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'CLAVE_SA' -C \
  -Q "BACKUP DATABASE InstitutoBlazorDemo TO DISK = '/var/opt/mssql/backup/instituto_$(date +%F).bak' WITH CHECKSUM"
```

Restauración (base nueva o reemplazada):

```sql
RESTORE DATABASE InstitutoBlazorDemo
FROM DISK = '/var/opt/mssql/backup/instituto_2026-11-16.bak'
WITH REPLACE, RECOVERY;
```

También se puede respaldar el volumen completo con el contenedor detenido:

```bash
docker compose -f /opt/institutoblazor/compose.sqlserver.yml stop sqlserver
docker run --rm -v institutoblazor_sqlserver-data:/data -v "$PWD":/backup alpine \
  tar czf /backup/sqlserver-data.tgz -C /data .
docker compose -f /opt/institutoblazor/compose.sqlserver.yml start sqlserver
```

## 7. Verificación rápida

```bash
curl -fsS http://127.0.0.1:5000/health
```

Respuesta esperada: `Healthy`. Si devuelve `Unhealthy`, revisar:

```bash
docker logs institutoblazor-sqlserver --tail 100
journalctl -u institutoblazor -n 100 --no-pager
```
