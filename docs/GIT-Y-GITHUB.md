# Git, GitHub y organización del trabajo

Este documento resume cómo se organiza el repositorio y qué evidencias deja el flujo de trabajo para la entrega del integrador.

## 1. Ramas

| Rama | Uso |
|---|---|
| `main` | Versión desplegada y estable. Solo recibe merges desde `develop`. |
| `develop` | Integración de historias terminadas y probadas. |
| `feature/<tema>` | Una historia o cambio acotado. Sale de `develop` y vuelve por PR. |
| `hotfix/<tema>` | Corrección urgente sobre `main`; se fusiona también en `develop`. |

Convención de nombres: `feature/validacion-dni`, `feature/paginacion-server`, `hotfix/sesion-f5`.

## 2. Commits

Formato sugerido (Conventional Commits):

```text
feat: agregar paginación en servidor
fix: conservar sesión después de F5
test: cubrir DNI duplicado en SQL Server
docs: completar guía de despliegue
ci: publicar artefacto y aplicar migraciones
```

Cada commit debe compilar. No se suben secretos, contraseñas, `bin/`, `obj/`, `node_modules/` ni archivos `.env` reales.

## 3. Issues e historias

Usar las plantillas de `.github/ISSUE_TEMPLATE/`:

- **Historia de usuario:** rol, acción, beneficio, criterios de aceptación y estimación Fibonacci.
- **Error:** pasos para reproducir, esperado, actual y evidencia.
- **Documentación:** documento o evidencia faltante.

Cada PR debe referenciar su issue con `Closes #N`.

## 4. Tablero

Crear un Project (GitHub Projects) con columnas:

```text
Backlog → Por hacer → En curso → En revisión → Hecho
```

Mover cada issue a medida que avanza. El tablero es evidencia directa de la planificación solicitada.

Sugerencia de historias mínimas:

1. Registro e inicio de sesión con hash.
2. Sesión persistente tras recarga (F5).
3. ABM de empleados con validaciones y mensajes.
4. Búsqueda, filtro por departamento y paginación en servidor.
5. Modo oscuro y diseño responsive accesible.
6. Pruebas de persistencia en SQLite y SQL Server.
7. CI/CD y despliegue en Ubuntu Server.
8. Documentación, evidencias y guion de presentación.

## 5. Pull Requests

La plantilla `.github/pull_request_template.md` pide criterios, pruebas y evidencia. Flujo:

```bash
git switch develop
git pull
git switch -c feature/nombre-corto
# trabajo + pruebas
git add .
git commit -m "feat: ..."
git push -u origin feature/nombre-corto
```

Abrir PR hacia `develop`, esperar CI en verde, pedir revisión y fusionar. Para publicar:

```bash
git switch develop
git pull
git switch main
git pull
git merge --no-ff develop
git tag -a v0.3.0 -m "Versión integradora"
git push origin main --tags
```

La protección de rama debe exigir PR y checks de CI antes de fusionar.

## 6. Releases

Etiquetar cada entrega:

```bash
git tag -a v0.3.0 -m "Entrega integradora"
git push origin v0.3.0
```

En GitHub, crear un Release asociado al tag con notas tomadas de `CHANGELOG.md` y el enlace a la URL desplegada.

## 7. Qué revisa CI

`.github/workflows/ci-deploy.yml`:

1. Restaura herramientas y dependencias.
2. Compila en Release con warnings como errores.
3. Ejecuta pruebas con SQLite.
4. Levanta SQL Server temporal y repite las pruebas contra SQL Server.
5. Verifica que no haya cambios de modelo sin migración.
6. Publica la aplicación y el script SQL idempotente como artefactos.
7. En `main`, si `DEPLOY_ENABLED=true`, despliega con el runner self-hosted.

## 8. Evidencias para la entrega

Guardar capturas o enlaces de:

- Tablero con columnas e historias.
- Issues con criterios y estimación.
- PR con discusión y revisión.
- Workflow verde (build + SQL Server + deploy).
- Releases y tags.
- URL pública funcionando.
- Documentación y capturas en `docs/capturas/`.
