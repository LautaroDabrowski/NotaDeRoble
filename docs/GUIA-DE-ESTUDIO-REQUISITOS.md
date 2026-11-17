# Guía de estudio — Requisitos de la exposición

Fuente: `EDIWEB/U4.pdf`, secciones **Proyecto Integrador – Aplicación Blazor Completa**, **Checklist de Entrega Pre-Presentación** y **Criterios de Evaluación** (Clase 24).

Esta guía responde una sola pregunta: **para cada requisito, dónde está en el proyecto y qué muestro o digo en la exposición.**

## 1. Tablero de puntaje

| Criterio de la consigna | Puntos | Estado | Dónde se evidencia |
|---|---:|---|---|
| Componentes Blazor: estructura, parámetros, eventos | 20 | Hecho | `Components/Shared/`, `Components/Pages/` |
| Formularios y validaciones avanzadas | 15 | Hecho | `EmployeeForm.razor`, `EmpleadoValidator.cs`, `Empleado.cs` |
| Interfaz visual: HTML5, CSS, Bootstrap responsive | 15 | Hecho | `MainLayout.razor`, `Header/Footer`, `app.css`, capturas |
| JS Interop implementado correctamente | 10 | Hecho | `ThemeToggle.razor`, `CopyLink.razor`, `wwwroot/js/browser.js` |
| Persistencia EF Core: CRUD, paginación, búsqueda | 20 | Hecho | `AppDbContext`, `EmpleadoService`, migraciones, pruebas |
| Git: historial limpio, ramas, CI/CD | 10 | **Parcial** | Repo público y build verde; faltan ramas/PR/issues y runner |
| Aplicación desplegada y accesible | 5 | Hecho (URL temporal) | VM QEMU + Nginx + Cloudflare Tunnel |
| README y documentación | 5 | Hecho | `README.md`, `CHANGELOG.md`, `docs/` |

**Total: 100 puntos.** El repo y el build ya están. La prioridad antes de exponer es crear los 5 issues, el tablero, un PR de ejemplo y registrar el runner para que el deploy se ejecute.

## 2. Mapa rápido: requisito → archivo → qué mostrar

| Requisito de la consigna | Archivo principal | Evidencia concreta |
|---|---|---|
| Mínimo 6 componentes reutilizables | `Components/Shared/` | Hay **11** componentes; parámetros y eventos en `EmployeeForm`, `Paginacion`, `ConfirmDialog`, `AlertMessage`, `ThemeToggle`, `DetailItem` |
| Formularios con `EditForm` y validación avanzada | `Components/Shared/EmployeeForm.razor` | `EditForm` línea 5, `DataAnnotationsValidator` línea 6, `ServerValidation` línea 7, FluentValidation líneas 57-59 |
| Routing con parámetros | `EmpleadoEditar.razor`, `EmpleadoVer.razor` | `@page "/empleados/editar/{Id:int}"` y `[Parameter] public int Id` |
| Navegación programática | `Login.razor`, `EmpleadoNuevo.razor`, `Index.razor` | `Nav.NavigateTo(...)` |
| State Service con notificación reactiva | `SessionService.cs`, `EmpleadosState.cs` | `event Action? OnChange` y suscripciones en `MainLayout` y `Header` |
| HTML5 semántico | `MainLayout.razor`, `Header.razor`, `Footer.razor` | `header`, `nav`, `main`, `article`, `aside`, `footer`, `section`, `table` |
| CSS Isolation en al menos 3 componentes | `*.razor.css` | **6** archivos de aislamiento |
| Bootstrap 5 responsive mobile-first | `App.razor`, `Empleados.razor` | Bootstrap 5.3.3, `col-md-*`, `table-responsive`, prueba a 375 px |
| JS Interop | `ThemeToggle.razor`, `browser.js` | `IJSRuntime` + `localStorage` + `data-bs-theme` |
| EF Core con `DbContextFactory` | `Program.cs`, `EmpleadoService.cs` | `AddDbContextFactory` y `CreateDbContextAsync()` |
| Al menos 2 entidades relacionadas | `Models/`, `AppDbContext.cs` | `Empleado` → `Departamento` y `Empleado` → `Localidad` (1:N) |
| Migraciones versionadas y seed | `Data/Migrations/`, `DemoData.cs` | 2 migraciones; 5 departamentos, 5 localidades, 12 empleados |
| CRUD completo | `EmpleadoService.cs`, páginas de empleados | Crear, listar, ver, editar, eliminar |
| Paginación y búsqueda | `EmpleadoService.ObtenerPaginadoAsync` | `Where`, `CountAsync`, `Skip/Take` en SQL Server |
| Repositorio GitHub limpio | https://github.com/LautaroDabrowski/NotaDeRoble | **Hecho parcial**: repo público y 6 commits; faltan ramas/PR/issues |
| CI/CD con GitHub Actions | `.github/workflows/ci-deploy.yml` | Build verde (run 43978670037); deploy skipped hasta runner |
| README profesional | `README.md` | Descripción, stack, arquitectura Mermaid, instalación, pruebas, despliegue |
| Aplicación en URL pública | Cloudflare Tunnel | `https://chances-mtv-labour-depends.trycloudflare.com` (temporal) |
| CHANGELOG con 3 versiones | `CHANGELOG.md` | `0.3.0`, `0.2.0`, `0.1.0` |
| Sin credenciales en el código | `appsettings.json`, `.gitignore` | Cadena vacía, user secrets y variables de entorno |
| Datos de seed para la demo | `Data/DemoData.cs` | 12 empleados y catálogos |

## 3. Unidad 1 — Componentes Blazor (20 puntos)

### 3.1 Mínimo 6 componentes reutilizables y encapsulados

Hay **11 componentes compartidos** en `Components/Shared/`:

| Componente | Rol | Dónde se ve la encapsulación |
|---|---|---|
| `EmployeeForm.razor` | Formulario compartido por alta y edición | `[Parameter]` líneas 41-45; `EventCallback<Empleado>` línea 45 |
| `Paginacion.razor` | Barra de paginación reutilizable | Parámetros líneas 10-14; `EventCallback<int>` línea 14 |
| `ConfirmDialog.razor` | Modal de confirmación | Parámetros y eventos líneas 18-23 |
| `AlertMessage.razor` | Mensaje de éxito/error | Parámetros líneas 10-13 |
| `ThemeToggle.razor` | Botón de tema autocontenido | `IJSRuntime` línea 1; ciclo de vida y `DisposeAsync` líneas 13-40 |
| `DetailItem.razor` | Ítem etiqueta/valor | Parámetros línea 2 |
| `ServerValidation.razor` | Puente entre FluentValidation y `EditContext` | `ValidationMessageStore` líneas 5-19 |
| `Header.razor` | Cabecera, navegación y sesión | `header`, `nav`, `aside`; suscripción a eventos líneas 36-47 |
| `Footer.razor` | Pie semántico | `footer` y `section` líneas 1-14 |
| `CopyLink.razor` | Copiar enlace con JS | `IJSRuntime` línea 1; invocación línea 32 |
| `RedirectToLogin.razor` | Guard de navegación | `Nav.NavigateTo("/login")` línea 3 |

Para la exposición alcanza con abrir **`EmployeeForm.razor`** y **`Paginacion.razor`**: se ven parámetros de entrada, eventos de salida y el componente se usa desde dos páginas distintas.

### 3.2 Formularios con `EditForm` y validación avanzada

- `Components/Shared/EmployeeForm.razor:5` → `<EditForm Model="Model" OnValidSubmit="HandleSubmit" FormName="empleado">`.
- Línea 6 → `<DataAnnotationsValidator />` (reglas del modelo).
- Línea 7 → `<ServerValidation @ref="ServerValidator" />` (errores de FluentValidation dentro del `EditContext`).
- Línea 8 → `<ValidationSummary>` (resumen visible).
- Líneas 13-31 → `ValidationMessage` por campo.
- Línea 57 → `await Validator.ValidateAsync(Model)` antes de guardar.
- `Validators/EmpleadoValidator.cs:13-17` → DNI único consultando la base.
- `Validators/EmpleadoValidator.cs:19-23` → departamento existente.
- `Validators/EmpleadoValidator.cs:25-29` → localidad existente.
- `Models/Empleado.cs:8-26` → `Required`, `StringLength`, `Range` y expresiones regulares de DNI, nombre, apellido y teléfono.
- `Models/RegistroUsuario.cs:7-23` → usuario, email, contraseña y confirmación.
- `Components/Pages/Register.razor:13-21` y `Login.razor:14-20` → otros dos `EditForm` reales.

**Qué decir:** “La validación tiene tres capas: DataAnnotations valida formato y obligatoriedad; FluentValidation valida reglas de negocio que necesitan la base, como DNI único o departamento existente; y el índice único de SQL Server protege aunque alguien omita la capa anterior.”

### 3.3 Routing con parámetros y navegación programática

- `Components/Pages/EmpleadoEditar.razor:1` → `@page "/empleados/editar/{Id:int}"`.
- `Components/Pages/EmpleadoEditar.razor:20` → `[Parameter] public int Id { get; set; }`.
- `Components/Pages/EmpleadoEditar.razor:47` → `Nav.NavigateTo("/empleados")` después de actualizar.
- `Components/Pages/EmpleadoVer.razor:1` y `:19` → ruta con parámetro y recepción del `Id`.
- `Components/Pages/EmpleadoNuevo.razor:39` → navegación programática tras crear.
- `Components/Pages/Login.razor:40` → navegación al listado tras autenticar.
- `Components/Pages/Index.razor:7` → redirección según haya sesión o no.
- `Components/Shared/RedirectToLogin.razor:3` → redirección a `/login` si se intenta entrar sin sesión.

**Qué demostrar:** entrar a `/empleados/editar/1`, ver que el `Id` de la URL carga el empleado correcto y que al guardar vuelve al listado.

### 3.4 State Service con notificación reactiva

- `Services/SessionService.cs:18` → `public event Action? OnChange;`.
- `Services/SessionService.cs:22-31` → login, persistencia y `OnChange?.Invoke()`.
- `Services/SessionService.cs:33-41` → logout y notificación.
- `Services/SessionService.cs:43-61` → restauración desde `ProtectedSessionStorage` al iniciar.
- `Services/EmpleadosState.cs:4-11` → estado de filtros/página y evento `OnChange`.
- `Services/EmpleadosState.cs:13-28` → actualización del resumen y limpieza con notificación.
- `Components/Layout/MainLayout.razor:22-30` → se suscribe a `Session.OnChange` y llama `StateHasChanged`.
- `Components/Shared/Header.razor:36-47` → se suscribe a `Session` y `EmpleadosState`.
- `Components/Pages/Empleados.razor:89-92` → publica el total y los departamentos en el estado compartido.

**Qué demostrar:** iniciar sesión, hacer F5 y ver que la sesión y el listado siguen; cerrar sesión y ver que el header cambia sin recargar manualmente.

## 4. Unidad 2 — Interfaz visual (15 + 10 puntos)

### 4.1 HTML5 semántico en los layouts

- `Components/Layout/MainLayout.razor:11-18` → `div.app-shell`, skip link, `header`, `main`, `article`, `footer`.
- `Components/Shared/Header.razor:6-33` → `header`, `nav`, `aside`.
- `Components/Shared/Footer.razor:1-14` → `footer`, `section`.
- `Components/Pages/Empleados.razor:15-60` → `section`, `header`, `form`, `fieldset`, `legend`, `table`, `caption`, `thead`, `tbody`.
- `Components/Shared/EmployeeForm.razor:11-32` → `fieldset`, `legend`, `label` asociado por `for`, `ValidationMessage`.

**Qué decir:** “No uso `div` para todo: los landmarks (`header`, `nav`, `main`, `footer`) permiten navegar con lector de pantalla y el skip link lleva directo al contenido.”

### 4.2 CSS Isolation en al menos 3 componentes

Hay **6** archivos:

1. `Components/Layout/MainLayout.razor.css`
2. `Components/Shared/Header.razor.css`
3. `Components/Shared/EmployeeForm.razor.css`
4. `Components/Shared/Paginacion.razor.css`
5. `Components/Shared/ConfirmDialog.razor.css`
6. `Components/Shared/DetailItem.razor.css`

`EmployeeForm.razor.css:2-4` usa `::deep` para estilizar los mensajes de validación que genera un componente hijo. El CSS se empaqueta como `InstitutoBlazor.styles.css` y se referencia en `Components/App.razor:12`.

### 4.3 Bootstrap 5 responsive mobile-first

- `Components/App.razor:7` → Bootstrap **5.3.3** desde CDN.
- `Components/Pages/Empleados.razor:23-26` → grilla `row g-2`, `col-md-5`, `col-md-3`, `col-auto`.
- `Components/Pages/Empleados.razor:38` → `table-responsive` para tablas en pantallas chicas.
- `Components/Shared/EmployeeForm.razor:13-31` → `col-md-6` y `col-12` (una columna en móvil, dos en escritorio).
- `wwwroot/css/app.css:17` → `clamp(1.5rem,4vw,2.25rem)` para títulos fluidos.
- `wwwroot/css/app.css:18` → media query de escritorio.
- `tests/browser/integrador.spec.js:69-72` → viewport de **375 px** y verificación de que no hay scroll horizontal.
- Capturas: `docs/capturas/empleados-claro.png` y `docs/capturas/empleados-oscuro-movil.png`.

**Qué demostrar:** abrir DevTools, activar el modo dispositivo de 375 px y mostrar que la tabla se desplaza dentro de su contenedor, sin romper la página.

### 4.4 JS Interop

- `Components/Shared/ThemeToggle.razor:1` → `@inject IJSRuntime JS`.
- `ThemeToggle.razor:18` → importa `./js/browser.js`.
- `ThemeToggle.razor:21` → lee el tema guardado.
- `ThemeToggle.razor:31` → aplica el tema con JS.
- `wwwroot/js/browser.js:3-10` → `readTheme()`: lee `localStorage`, usa `prefers-color-scheme` y escribe `data-bs-theme`.
- `wwwroot/js/browser.js:12-16` → `setTheme()`: aplica y persiste el tema.
- `Components/Shared/CopyLink.razor:19` y `:32` → segundo uso de JS Interop para copiar el enlace con `navigator.clipboard` (`browser.js:18-24`).
- `tests/browser/integrador.spec.js:60-68` → cambia a oscuro, recarga y verifica que el tema persiste.

**Qué decir:** “El modo oscuro no se guarda en el servidor: se guarda en `localStorage` y se aplica con JS antes de que el usuario vea el parpadeo. El servidor no necesita saber el tema.”

## 5. Unidad 3 — Persistencia (20 puntos)

### 5.1 EF Core con `DbContextFactory`

- `Program.cs:18-19` → `AddDbContextFactory<AppDbContext>(...)` con SQL Server.
- `Services/EmpleadoService.cs:9-12` → recibe `IDbContextFactory<AppDbContext>`.
- `Services/EmpleadoService.cs:17`, `:39`, `:71`, `:99` → crea un contexto corto por operación con `await using var db = await dbFactory.CreateDbContextAsync();`.
- `Services/UsuarioService.cs:9`, `:16`, `:37` → el mismo patrón para usuarios.
- `Services/CatalogoService.cs:7`, `:11`, `:17` → el mismo patrón para catálogos.
- `Data/DatabaseHealthCheck.cs:7-14` → también usa la fábrica para verificar la base.
- `tests/InstitutoBlazor.Tests/PersistenciaTests.cs:21` → la prueba implementa `IDbContextFactory` para inyectar SQLite o SQL Server.

**Por qué:** en Blazor Server el circuito vive mucho tiempo; un `DbContext` scoped quedaría vivo demasiado y no es thread-safe. La fábrica permite contextos cortos y aislados por operación.

### 5.2 Al menos 2 entidades con relación

- `Models/Empleado.cs:27-28` → propiedades de navegación `Departamento` y `Localidad`.
- `Models/Departamento.cs:7` → `ICollection<Empleado> Empleados`.
- `Models/Localidad.cs:8` → `ICollection<Empleado> Empleados`.
- `Data/AppDbContext.cs:28-29` → `Empleado` 1:N `Departamento`, FK `IdDepartamento`, borrado `Restrict`.
- `Data/AppDbContext.cs:30-32` → `Empleado` 1:N `Localidad`, FK `CodPostal`.
- `Data/AppDbContext.cs:8-11` → cuatro `DbSet`: `Empleados`, `Departamentos`, `Localidades`, `Usuarios`.
- `Data/AppDbContext.cs:27` → índice único en `Dni`.

**Qué mostrar:** el diagrama de relaciones en `docs/BASE-DE-DATOS.md` y el listado con las columnas Departamento/Localidad.

### 5.3 Migraciones versionadas y datos de seed

- `Data/Migrations/20261116205233_CreacionInicial.cs:13-55` → crea `departamento`, `localidad`, `usuario` y `empleados`.
- `Data/Migrations/20261116205233_CreacionInicial.cs:72-82` → claves foráneas.
- `Data/Migrations/20261116205233_CreacionInicial.cs:85-108` → índices únicos y de búsqueda.
- `Data/Migrations/20261116205436_DatosDemostracion.cs:15-59` → `InsertData` del seed.
- `Data/DemoData.cs:11-16` → 5 departamentos.
- `Data/DemoData.cs:17-22` → 5 localidades.
- `Data/DemoData.cs:24-34` → 12 empleados deterministas.
- `Program.cs:45-49` → aplicación opcional de migraciones al arrancar (`Database:ApplyMigrations`).
- `tests/InstitutoBlazor.Tests/PersistenciaTests.cs:152-162` → el seed no resucita empleados eliminados al reiniciar migraciones.

**Qué decir:** “El seed va dentro de una migración, no en un `INSERT` manual. Se aplica una sola vez; si borro un empleado y vuelvo a arrancar, la migración ya está registrada y no lo vuelve a insertar.”

### 5.4 CRUD completo con paginación y búsqueda

- `Services/IEmpleadoService.cs:14-38` → contrato del servicio: paginado, obtener, crear, actualizar, eliminar, estadísticas.
- `Services/EmpleadoService.cs:15-34` → paginación + búsqueda + filtro + `Include` de relaciones.
- `Services/EmpleadoService.cs:19-23` → busca por DNI, nombre o apellido.
- `Services/EmpleadoService.cs:24` → filtra por departamento.
- `Services/EmpleadoService.cs:27-31` → `CountAsync`, `Skip`, `Take` y orden estable.
- `Services/EmpleadoService.cs:57-94` → validación y guardado (alta/edición).
- `Services/EmpleadoService.cs:97-114` → eliminación con control de errores.
- `Components/Pages/Empleados.razor:15-66` → listado, búsqueda, paginación y diálogo de borrado.
- `Components/Pages/Empleados.razor:83-106` → carga, búsqueda, limpieza y cambio de página.
- `Components/Pages/Empleados.razor:108-128` → eliminación.
- `Components/Pages/EmpleadoNuevo.razor:13-41` → alta.
- `Components/Pages/EmpleadoEditar.razor:1-49` → edición con parámetro de ruta.
- `Components/Pages/EmpleadoVer.razor:1-30` → detalle.
- `tests/browser/integrador.spec.js:28-58` → E2E de alta, validación, búsqueda, edición, detalle y borrado.
- `tests/InstitutoBlazor.Tests/PersistenciaTests.cs:65-134` → CRUD, DNI duplicado, índice único, paginación y validaciones.

**Números para memorizar:** 12 empleados de seed, 10 por página, 2 páginas, búsqueda por DNI/nombre/apellido y filtro por departamento.

## 6. Unidad 4 — Prácticas profesionales (20 puntos)

### 6.1 Repositorio GitHub con historial limpio — HECHO PARCIAL

Lo que ya existe:

- Repo público: https://github.com/LautaroDabrowski/NotaDeRoble
- Rama `main` con 6 commits convencionales y sin secretos.
- `.github/workflows/ci-deploy.yml` (CI/CD).
- `.github/pull_request_template.md`.
- `.github/ISSUE_TEMPLATE/historia.md`, `bug.md`, `documentacion.md`.
- `docs/GIT-Y-GITHUB.md` con el flujo de ramas, issues, tablero y PRs.
- `.gitignore` de .NET (líneas 1-23), que excluye `bin/`, `obj/`, `.env`, `*.env`, `secrets.json`, `appsettings.*.local.json`, `publish/`, `node_modules/`, etc.
- Workflow ejecutado: build **verde** en el run `43978670037`; el job `deploy` quedó `skipped` porque falta el runner y `DEPLOY_ENABLED=true`.

Lo que falta para la nota:

1. Crear la rama `develop` y una rama `feature/...` o `docs/...` para una mejora.
2. Abrir un PR, comentar una línea y hacer merge.
3. Crear **5 issues** con etiquetas `bug`, `enhancement`, `documentation`.
4. Crear el tablero Kanban: `Backlog`, `In Progress`, `Done`.
5. Registrar el runner self-hosted en la VM y activar `DEPLOY_ENABLED=true` para que el deploy deje de estar omitido.

**Commits ya realizados:**

```text
96ce8d0 feat: implementar aplicación Blazor con CRUD, autenticación y persistencia
099f1d2 chore: configurar SDK, herramientas y perfiles de ejecución
052e696 ci: agregar workflow de build y despliegue con GitHub Actions
537cc50 chore: agregar scripts de despliegue para Ubuntu y QEMU
c2e5ffd docs: documentar arquitectura, despliegue y guía de presentación
f428244 docs: agregar guía de estudio en PDF
```

**Orden de prioridad:** 5 issues + tablero + PR de ejemplo (10 puntos y checklist) → runner + deploy → ensayo. Si el tiempo aprieta, el PR puede ser un cambio pequeño de documentación, como el de esta guía.

### 6.2 CI/CD con GitHub Actions

`.github/workflows/ci-deploy.yml`:

- Líneas 3-8 → se ejecuta en `push` a `main`, `develop`, `feature/**`, `release/**`, `hotfix/**`, en PR a `main`/`develop` y manualmente.
- Líneas 13-29 → job `build` en `ubuntu-24.04`, .NET 10, restore y build Release con `--warnaserror`.
- Líneas 30-31 → pruebas con SQLite.
- Líneas 32-49 → levanta SQL Server 2022 temporal en el puerto 14339 con contraseña aleatoria enmascarada.
- Líneas 50-51 → repite las pruebas contra SQL Server.
- Líneas 52-56 → `has-pending-model-changes`, publicación y script SQL idempotente.
- Líneas 57-66 → artefactos `instituto-app` y `migraciones-sql`.
- Líneas 71-90 → job `deploy`: solo en `main`, con `vars.DEPLOY_ENABLED == 'true'`, en un runner **self-hosted** con etiquetas `[self-hosted, linux, x64, instituto-production]`; descarga el artefacto y ejecuta `/usr/local/bin/deploy-institutoblazor`.

**Estado:** build verde en el run `43978670037`; el job `deploy` quedó `skipped` porque `vars.DEPLOY_ENABLED` no está definido y el runner todavía no está registrado.

**Qué decir:** “El CI no solo compila: prueba con SQLite y con SQL Server real, verifica que no falte ninguna migración, publica la app y el script SQL, y solo despliega en `main` si la variable `DEPLOY_ENABLED` está en true.”

### 6.3 README profesional

`README.md` incluye:

- Descripción y capturas (líneas 1-7).
- Funcionalidades (líneas 9-20).
- Stack (líneas 22-30).
- Arquitectura con diagrama **Mermaid** (sección `Arquitectura`).
- Estructura del proyecto.
- Requisitos y puesta en marcha local (restore, SQL Server, user secrets, migraciones, `dotnet run`).
- Pruebas unitarias, SQL Server y E2E.
- Configuración y despliegue.
- CI/CD, seguridad, migración desde PHP y solución de problemas.

### 6.4 CHANGELOG con al menos 3 versiones

`CHANGELOG.md` tiene:

- `0.3.0` — validaciones, paginación, modo oscuro, pruebas, CI/CD y despliegue.
- `0.2.0` — migración inicial PHP/MySQL → Blazor + SQL Server.
- `0.1.0` — estructura inicial, modelo y primeras pantallas.

### 6.5 Aplicación desplegada y accesible

- VM Ubuntu Server 26.04.1 sobre QEMU/KVM: 4 vCPU, 6 GB RAM, disco de 25 GB.
- Nginx en el puerto 80 → Kestrel en `127.0.0.1:5000`.
- SQL Server 2022 en Docker, solo loopback.
- Servicio `institutoblazor.service` con `Restart=on-failure`.
- Túnel Cloudflare desde el host: `http://127.0.0.1:8080` → URL HTTPS pública.
- URL actual: **https://chances-mtv-labour-depends.trycloudflare.com**
- Verificación: `/health` devuelve `Healthy`; home y `/registro` devuelven 200.
- La prueba E2E completa pasó contra la URL pública (registro, F5, validaciones, CRUD, búsqueda, paginación, modo oscuro y 375 px).

**Aclaración honesta para la defensa:** es un **Cloudflare Quick Tunnel** temporal; no requiere abrir puertos en el router, pero la URL cambia si se reinicia el túnel. Para una URL permanente habría que usar un túnel nombrado con dominio o configurar el router. La app y la base siguen funcionando en la VM aunque el túnel se reinicie; solo cambia el enlace público.

## 7. Checklist de entrega pre-presentación

| Checklist de la consigna | Estado | Evidencia / acción |
|---|---|---|
| Aplicación desplegada con URL funcionando | Hecho (temporal) | URL de Cloudflare + `/health` |
| Repositorio GitHub público con historial limpio | **Hecho parcial** | Repo público y 6 commits; faltan issues/tablero/PR |
| README completo: descripción, stack, instalación, capturas | Hecho | `README.md` |
| CHANGELOG con al menos 3 versiones | Hecho | `0.3.0`, `0.2.0`, `0.1.0` |
| GitHub Actions: workflow CI/CD verde | **Build verde; deploy pendiente** | Run `43978670037`; deploy skipped hasta runner |
| Base de datos con datos de seed | Hecho | 12 empleados, 5 departamentos, 5 localidades |
| Sin credenciales ni secrets en el código | Hecho | `appsettings.json` vacío, `.gitignore`, variables de entorno |

## 8. Guion de exposición (10 minutos + 5 de preguntas)

### 0:00–1:00 — Introducción

“InstitutoBlazor es la migración de un CRUD de empleados en PHP/MySQL a Blazor Web App con .NET 10, EF Core y SQL Server. Resuelve el alta, búsqueda y administración de empleados con validaciones de servidor y sesión persistente, desplegado en una VM Ubuntu con URL pública.”

### 1:00–5:00 — Demo en vivo (orden exacto)

1. Abrir la URL pública → `/registro` → crear usuario → login.
2. Mostrar el listado: **Página 1 de 2**, 12 registros.
3. **F5** y verificar que la sesión sigue.
4. “Nuevo empleado” → guardar vacío → mostrar errores por campo.
5. DNI `90000001` (ya existe) → mostrar “El DNI ya está registrado en otro empleado.”
6. DNI `12345678`, nombre y apellido → guardar.
7. Buscar “Integrador” → 1 registro; editar; ver detalle; volver.
8. Eliminar → cancelar → eliminar → confirmar.
9. Limpiar búsqueda → vuelven los 12.
10. Modo oscuro → recargar → sigue oscuro.
11. DevTools a 375 px → sin scroll horizontal.
12. Cerrar sesión → intentar `/empleados/nuevo` → redirige a `/login`.

### 5:00–8:00 — Recorrido por el código

Abrir en este orden y decir una frase por archivo:

1. `Components/Shared/EmployeeForm.razor` → `EditForm`, validación y componente reutilizado por alta y edición.
2. `Validators/EmpleadoValidator.cs` → reglas asíncronas contra la base.
3. `Services/EmpleadoService.cs:15-34` → paginación y búsqueda en SQL con `Skip/Take`.
4. `Data/AppDbContext.cs:28-32` y una migración → relaciones, FK e índices.
5. `Services/SessionService.cs:18-30` → estado reactivo y sesión persistente.
6. `Program.cs:18-19` → `DbContextFactory` y arquitectura.

### 8:00–10:00 — Repositorio, CI/CD y reflexión

- Mostrar el historial de commits (`git log --oneline --graph`).
- Mostrar el workflow en Actions y el artefacto.
- Mostrar README, CHANGELOG y capturas.
- Reflexión: “Lo más difícil fue sostener la sesión después de F5 en Blazor Server sin cookie de autenticación y hacer que la paginación y la búsqueda se ejecuten en SQL, no en memoria. Lo resolvimos con `ProtectedSessionStorage`, `IDbContextFactory` y `Skip/Take`.”

## 9. Preguntas probables y respuestas cortas

**¿Por qué Blazor Server y no WebAssembly?**
Porque el CRUD depende de SQL Server y servicios de servidor; Interactive Server simplifica autenticación, acceso a datos y despliegue, y funciona bien en una VM chica.

**¿Por qué `IDbContextFactory` y no un `DbContext` scoped?**
En Blazor Server el circuito dura toda la sesión; un contexto scoped quedaría vivo demasiado y no es thread-safe. La fábrica crea contextos cortos por operación.

**¿Cómo funciona la paginación?**
`ObtenerPaginadoAsync` filtra con `Where`, cuenta con `CountAsync`, ajusta la página y trae solo 10 filas con `Skip((pagina-1)*tamanio).Take(tamanio)`. Todo se ejecuta en SQL Server.

**¿Cómo se evita SQL injection?**
EF Core parametriza las consultas LINQ. No se concatenan strings con datos del usuario.

**¿Cómo se valida un DNI duplicado?**
FluentValidation consulta si existe otro empleado con ese DNI; además hay un índice único en la base y se captura `DbUpdateException` como última defensa.

**¿Cómo se guarda la contraseña?**
Con `PasswordHasher<Usuario>` (`UsuarioService.cs:11` y `:44`): hash con salt, nunca texto plano. En login se verifica y se rehashea si hace falta.

**¿Cómo sobrevive la sesión a F5?**
`SessionService` guarda el usuario en `ProtectedSessionStorage` (cifrado y firmado) con vencimiento de 8 horas; `MainLayout` la restaura después del primer render (`MainLayout.razor:24-28`).

**¿Eso es autenticación de producción?**
No. Es una sesión liviana para el alcance académico. En producción usaría ASP.NET Core Identity, cookie de autenticación, roles y autorización por servidor.

**¿Cómo se protegen las rutas?**
Las páginas verifican `Session.IsAuthenticated` y muestran `RedirectToLogin`; además el header cambia. Es una guarda de UI, suficiente para el TP.

**¿Qué pasa si la base se cae?**
`/health` devuelve `Unhealthy`; el servicio tiene `Restart=on-failure`, SQL Server `restart: unless-stopped` y las páginas muestran un mensaje de error comprensible.

**¿Cómo se aplican las migraciones en producción?**
Con `Database__ApplyMigrations=true` en el primer arranque sobre una base nueva, o con el script idempotente que genera el CI. El despliegue no se considera exitoso si `/health` no responde.

**¿Por qué el seed no vuelve a insertar empleados borrados?**
Porque `HasData` se aplica dentro de una migración y EF registra la migración como aplicada; al reiniciar no la ejecuta otra vez. Hay una prueba específica: `Seed_NoResucitaEmpleadosEliminados_AlReiniciarMigraciones`.

**¿Por qué `AsNoTracking`?**
Para consultas de solo lectura, evita que EF guarde cambios y mejora el rendimiento del listado.

**¿Cómo se hace el modo oscuro?**
`ThemeToggle` llama a JS Interop; `browser.js` guarda el tema en `localStorage` y escribe `data-bs-theme` en `<html>`. Al recargar, se lee antes de mostrar la página.

**¿Cómo se probó el responsive?**
Playwright cambia el viewport a 375×812 y verifica que `scrollWidth <= innerWidth`; las capturas están en `docs/capturas/`.

**¿Por qué un túnel y no una IP pública?**
El router de la red no permitía administración ni redirección de puertos; Cloudflare Tunnel da una URL HTTPS sin tocar el router. Es temporal; para una URL fija usaría un túnel nombrado con dominio.

**¿Qué falta?**
El repo ya está público y el build de Actions está verde. Faltan los 5 issues, el tablero, un PR de ejemplo y el runner self-hosted para que el deploy se ejecute; el código y el despliegue manual están terminados.

## 10. Cinco historias de usuario para los issues

Se pueden copiar tal cual en GitHub Issues:

| # | Historia | Puntos | Criterios de aceptación |
|---|---|---:|---|
| 1 | Como visitante, quiero registrarme con usuario, email y contraseña para acceder al sistema. | 3 | Valida usuario de 3-20 caracteres, email válido y contraseña con mayúscula y número; no permite duplicados; guarda hash. |
| 2 | Como usuario registrado, quiero iniciar sesión y que la sesión sobreviva a F5 para no volver a ingresar. | 5 | Login por usuario o email; sesión de 8 horas; F5 mantiene la sesión; logout la borra y redirige. |
| 3 | Como usuario, quiero dar de alta, editar, ver y eliminar empleados para administrar la nómina. | 8 | DNI de 7-8 dígitos; DNI único; departamento y localidad existentes; confirmación antes de eliminar; mensajes de error visibles. |
| 4 | Como usuario, quiero buscar por DNI, nombre o apellido, filtrar por departamento y paginar para encontrar empleados rápido. | 5 | Búsqueda combinable con filtro; 10 resultados por página; total y página visibles; la paginación se calcula en SQL. |
| 5 | Como usuario, quiero un modo oscuro persistente y una interfaz usable en el celular para trabajar cómodo. | 3 | Tema claro/oscuro con `localStorage`; persiste al recargar; a 375 px no hay scroll horizontal. |

Tablero Kanban: **Backlog → In Progress → Done**. Etiquetas: `enhancement` para las historias 1-5, `documentation` para README/CHANGELOG y `bug` para un defecto real o simulado.

## 11. Riesgos antes de exponer

1. **GitHub parcial:** el repo público y el build ya están; faltan los 5 issues, el tablero y un PR. Crearlos antes de la demo.
2. **Deploy omitido:** el build está verde, pero el job `deploy` queda `skipped` hasta registrar el runner y definir `DEPLOY_ENABLED=true`. Para la demo alcanza con mostrar el build verde y el despliegue manual; si se configura el runner, mejor.
3. **URL temporal:** si se reinicia el túnel, cambia la URL. No apagar la VM ni el túnel; probar la URL 30 minutos antes y tener a mano el comando para obtener la nueva.
4. **PC suspendida:** configurar energía en “nunca suspender” durante la demo.
5. **No mostrar secretos:** no abrir `/etc/institutoblazor/*.env`, ni la consola con la cadena de conexión, ni logs con contraseñas.

## 12. Checklist de pantallas para la demo

- [ ] URL pública abierta en `/login`.
- [ ] `/health` abierto y en `Healthy`.
- [ ] Pestaña del repositorio GitHub (commits, issues, tablero, PRs).
- [ ] Pestaña de Actions con el workflow.
- [ ] VS Code con `EmployeeForm.razor`, `EmpleadoService.cs`, `AppDbContext.cs` y una migración.
- [ ] `docs/capturas/empleados-claro.png` y `empleados-oscuro-movil.png`.
- [ ] `README.md` y `CHANGELOG.md`.
- [ ] Terminal con `git log --oneline --graph` (el repo ya está creado).
