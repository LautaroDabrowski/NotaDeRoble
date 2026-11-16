# Guion de presentación y evidencias

## 1. Estado de la entrega

| Requisito | Estado | Dónde se evidencia |
|---|---|---|
| Migración PHP/MySQL → C#/Blazor/SQL Server | Hecho | `README.md`, código, migraciones |
| CRUD de empleados con relaciones | Hecho | `/empleados`, `docs/capturas/` |
| Registro y login con hash | Hecho | `/registro`, `/login`, pruebas |
| Sesión persistente tras F5 | Hecho | `SessionService`, prueba E2E |
| Validaciones de servidor y mensajes | Hecho | `EmpleadoValidator`, `EmployeeForm` |
| Expresiones regulares (DNI, teléfono) | Hecho | `Models/Empleado.cs`, anexo |
| Búsqueda, filtro y paginación en servidor | Hecho | `EmpleadoService.ObtenerPaginadoAsync` |
| Diseño responsive y modo oscuro | Hecho | capturas claro/oscuro 375 px |
| Pruebas automatizadas | Hecho | 7 pruebas SQLite + SQL Server, Playwright |
| CI/CD con GitHub Actions | Configurado | `.github/workflows/ci-deploy.yml` |
| Documentación de despliegue | Hecho | `docs/DESPLIEGUE.md`, `docs/BASE-DE-DATOS.md` |
| Repositorio GitHub, issues, tablero y PRs | **Pendiente externo** | requiere crear el repo y subir el código |
| VM Ubuntu y URL pública | Hecho | QEMU/KVM + Cloudflare Tunnel |
| Prueba E2E contra la URL pública | Hecho | Playwright sobre la URL de Cloudflare |

### URL pública de la demo

```text
https://plugins-necessary-forms-tba.trycloudflare.com
```

Es un **Cloudflare Quick Tunnel** temporal: no requiere router ni IP pública, pero la URL cambia si se reinicia el túnel. Ver [`docs/VM-LOCAL.md`](VM-LOCAL.md) para obtener la URL actual o recrearlo.

## 2. Antes de presentar

0. Repasar [`docs/GUIA-DE-ESTUDIO-REQUISITOS.md`](GUIA-DE-ESTUDIO-REQUISITOS.md) o su [versión PDF](GUIA-DE-ESTUDIO-REQUISITOS.pdf): tiene el mapa de cada requisito con archivo y línea, el guion de demo y las preguntas probables.

1. Confirmar que la URL pública responde:

```bash
curl -fsS https://plugins-necessary-forms-tba.trycloudflare.com/health
```

Si no responde, comprobar que QEMU y el túnel estén activos:

```bash
systemctl --user status instituto-tunnel
journalctl --user -u instituto-tunnel -n 60 --no-pager | grep trycloudflare
```

La ventana de QEMU debe seguir abierta y el host no debe suspenderse.

2. Verificar que el workflow de `main` está verde.
3. Tener abiertas estas pestañas:
   - Aplicación desplegada (`/login`).
   - Repositorio: tablero, issues y Pull Requests.
   - Actions: última ejecución de CI/CD.
   - `docs/capturas/empleados-claro.png` y `empleados-oscuro-movil.png`.
4. Reiniciar la base solo si hace falta una demo desde cero (perder usuarios y empleados creados).
5. No mostrar archivos con contraseñas ni la consola del servidor con variables de entorno.

## 3. Guion de demo (8–10 minutos)

### 3.1 Arquitectura (1 minuto)

Mostrar `README.md` y explicar:

- Blazor Web App con Interactive Server.
- Separación Página → Servicio → EF Core → SQL Server.
- Nginx, systemd y SQL Server en Docker sobre Ubuntu Server.
- Pipeline: PR → CI con SQL Server real → artefacto → runner → `/health`.

### 3.2 Registro y sesión (1 minuto)

1. Abrir `/registro` y crear un usuario nuevo.
2. Probar una confirmación de contraseña distinta: debe mostrar error sin guardar.
3. Registrarse correctamente e iniciar sesión.
4. Recargar con **F5**: la sesión se mantiene.
5. Cerrar sesión e intentar entrar directo a `/empleados/nuevo`: redirige a `/login`.

### 3.3 Validaciones (2 minutos)

1. En `/empleados/nuevo`, enviar vacío: mensajes por campo.
2. DNI con puntos o letras: error de formato (expresión regular).
3. Teléfono inválido: error de formato.
4. DNI `90000001` (ya existe en los datos de demostración): mensaje de duplicado.
5. Mostrar que el índice único de SQL Server protege aunque se omita el validador (prueba automática).

### 3.4 ABM y consultas (2 minutos)

1. Crear un empleado con departamento y localidad.
2. Ver el detalle y comprobar las relaciones.
3. Editar un campo y guardar; verificar el cambio en el listado.
4. Eliminar con el diálogo de confirmación.
5. Buscar por DNI o apellido.
6. Filtrar por departamento.
7. Avanzar de página y comprobar que la paginación se calcula en la base (12 registros de demo, 10 por página).

### 3.5 Interfaz (1 minuto)

1. Alternar modo claro/oscuro; recargar y comprobar que se conserva.
2. Abrir las herramientas del navegador en modo responsive a 375 px.
3. Mostrar navegación por teclado, skip link y etiquetas asociadas a los campos.

### 3.6 Pruebas y CI/CD (2 minutos)

1. Mostrar `dotnet test` con 7 pruebas verdes.
2. Mostrar la ejecución de Playwright (registro, sesión, validación, CRUD, búsqueda, paginación y tema móvil).
3. Abrir Actions y señalar: build con `--warnaserror`, pruebas contra SQL Server, artefacto y job de despliegue.
4. Mostrar `/health` en producción.
5. Hacer un cambio menor, abrir PR, ver CI verde y merge a `main`; el runner despliega solo.

### 3.7 Cierre (1 minuto)

- Mencionar seguridad: contraseñas con hash, cadena de conexión fuera del repositorio, SQL solo en loopback, firewall 22/80/443.
- Mencionar límites conocidos: la URL del túnel es temporal y cambia al reiniciarlo; una URL estable requiere dominio + túnel nombrado o certificado en el router; el rollback de datos es manual; SQL Server Express tiene límite de 10 GB.

## 4. Evidencias a adjuntar

- Capturas de la app en escritorio y 375 px, modo claro y oscuro.
- Captura del tablero de GitHub con historias en distintas columnas.
- Enlaces a 2–3 issues y sus PRs fusionados.
- Captura del workflow verde.
- Captura de `/health` con `Healthy`.
- Captura de SQL Server con las tablas y los 12 registros de demostración.
- Enlace público de la aplicación: `https://plugins-necessary-forms-tba.trycloudflare.com` (temporal).

## 5. Preguntas frecuentes

**¿Por qué Blazor Server y no WebAssembly?**
Porque el CRUD depende de SQL Server y de servicios de servidor; Interactive Server simplifica autenticación, acceso a datos y despliegue, y el circuito funciona bien en una VM pequeña.

**¿Dónde está la lógica de negocio?**
En `Services/EmpleadoService` y `Services/UsuarioService`. Las páginas Razor solo orquestan estado y presentación.

**¿Cómo se evita SQL injection?**
EF Core parametriza las consultas LINQ. No se concatenan strings con datos del usuario.

**¿Cómo se validan los datos en servidor?**
FluentValidation en `EmpleadoValidator`, validaciones de modelo en `RegistroUsuario` y restricciones reales en SQL Server (índices únicos y claves foráneas).

**¿Qué pasa si la base se cae?**
`/health` devuelve `Unhealthy`; el servicio tiene `Restart=on-failure` y SQL Server `restart: unless-stopped`. El despliegue no se considera exitoso si `/health` no responde.

**¿Cómo se actualiza el esquema?**
Con migraciones EF. CI genera un script idempotente; en producción se aplica con un usuario administrador o se habilita `ApplyMigrations` en el primer arranque.

**¿Por qué no hay contraseña en el repositorio?**
Porque se usan user secrets en desarrollo y un archivo de entorno con permisos `640` en producción.
