# Despliegue en Ubuntu Server

Guía para publicar InstitutoBlazor en una VM con **Ubuntu Server 26.04 LTS**, SQL Server en Docker, Nginx como proxy inverso y despliegue automático desde GitHub Actions.

## 1. Arquitectura

```text
Internet
   │ 80/443
   ▼
Nginx (VM) ──proxy──► Kestrel 127.0.0.1:5000  (servicio systemd)
   │                        │
   │                        └──► SQL Server 127.0.0.1:1433 (contenedor Docker)
   │
GitHub Actions ──artefacto──► runner self-hosted ──► deploy-institutoblazor
```

- La aplicación escucha **solo en loopback**; el único puerto expuesto es Nginx.
- SQL Server también escucha solo en loopback y no se expone a Internet.
- El runner de GitHub no necesita Docker: solo ejecuta el script de despliegue y `sudo systemctl restart`.

## 2. Requisitos

| Recurso | Mínimo | Recomendado |
|---|---|---|
| vCPU | 2 | 4 |
| RAM | 4 GB | 6–8 GB (SQL Server consume ~2 GB) |
| Disco | 30 GB | 40 GB |
| Red | IP fija en la LAN | IP fija + puerto 80 redirigido |

En esta instalación la VM usa **NAT de QEMU** (IP `10.0.2.15`) con reenvío del host: `127.0.0.1:2222 → VM:22` y `0.0.0.0:8080 → VM:80`. La creación de la VM y el túnel público están en [`docs/VM-LOCAL.md`](VM-LOCAL.md). Si se usa una VM bridged con IP propia, el router puede redirigir el puerto 80 directamente a esa IP.

## 3. Instalar Ubuntu Server 26.04 LTS

1. Descargar la ISO desde `https://ubuntu.com/download/server`.
2. Crear la VM en VirtualBox, VMware o `virt-manager`.
3. Instalar con OpenSSH habilitado.
4. Actualizar el sistema:

```bash
sudo apt-get update && sudo apt-get upgrade -y
sudo timedatectl set-timezone America/Argentina/Buenos_Aires
```

Comprobar la IP de la VM:

```bash
ip -4 addr show
```

## 4. Paquetes base

```bash
sudo apt-get install -y curl ca-certificates gnupg lsb-release ufw

# Runtime ASP.NET Core 10 (script oficial, independiente de la versión de Ubuntu)
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
sudo bash /tmp/dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Nginx
sudo apt-get install -y nginx

# Docker Engine y Compose
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list >/dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

Verificar:

```bash
dotnet --list-runtimes | grep ASP.NETCore.App
docker --version
nginx -v
```

> No agregues el usuario de despliegue al grupo `docker`: ese grupo equivale a root. Los comandos de Docker se ejecutan con `sudo` y el runner no los necesita.

## 5. Usuarios y directorios

```bash
# Usuario sin login para ejecutar la aplicación
sudo useradd --system --home /opt/institutoblazor --shell /usr/sbin/nologin institutoblazor

# Usuario que recibe los despliegues y corre el runner de GitHub
sudo useradd --create-home --shell /bin/bash instituto-deploy

sudo mkdir -p /opt/institutoblazor/releases /etc/institutoblazor
sudo chown -R instituto-deploy:institutoblazor /opt/institutoblazor
sudo chmod 750 /opt/institutoblazor /opt/institutoblazor/releases

# El directorio de claves y datos lo crea systemd con StateDirectory,
# pero aseguramos permisos correctos
sudo install -d -o institutoblazor -g institutoblazor -m 750 /var/lib/institutoblazor
```

## 6. SQL Server en Docker

Crear el archivo de contraseña (solo root puede leerlo):

```bash
sudo tee /etc/institutoblazor/sqlserver.env >/dev/null <<'EOF'
MSSQL_SA_PASSWORD=CAMBIAR_CLAVE_UNICA_1!
EOF
sudo chmod 600 /etc/institutoblazor/sqlserver.env
```

Copiar el compose del repositorio:

```bash
sudo cp deploy/ubuntu/compose.sqlserver.yml /opt/institutoblazor/compose.sqlserver.yml
sudo chown root:root /opt/institutoblazor/compose.sqlserver.yml
```

Levantar y esperar el healthcheck:

```bash
sudo docker compose -f /opt/institutoblazor/compose.sqlserver.yml up -d
sudo docker compose -f /opt/institutoblazor/compose.sqlserver.yml ps
```

En esta instalación el contenedor se llama `institutoblazor-sqlserver` y guarda los datos en el volumen `institutoblazor-sqlserver-data`. Si se usa el compose, el nombre agrega el sufijo `-1`.

## 7. Base de datos y usuario de la aplicación

```bash
sudo docker exec -it institutoblazor-sqlserver \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C
```

Dentro de `sqlcmd`, reemplazar la clave y ejecutar:

```sql
CREATE DATABASE InstitutoBlazorDemo;
GO
CREATE LOGIN instituto_app WITH PASSWORD = N'CAMBIAR_CLAVE_APP_1!';
GO
USE InstitutoBlazorDemo;
GO
CREATE USER instituto_app FOR LOGIN instituto_app;
GO
ALTER ROLE db_owner ADD MEMBER instituto_app; -- primer arranque con migraciones
GO
```

Después del primer arranque, reducir privilegios (ver `docs/BASE-DE-DATOS.md`):

```sql
ALTER ROLE db_owner DROP MEMBER instituto_app;
ALTER ROLE db_datareader ADD MEMBER instituto_app;
ALTER ROLE db_datawriter ADD MEMBER instituto_app;
```

## 8. Variables de entorno de la aplicación

```bash
sudo cp deploy/ubuntu/institutoblazor.env.example /etc/institutoblazor/institutoblazor.env
sudo chown root:institutoblazor /etc/institutoblazor/institutoblazor.env
sudo chmod 640 /etc/institutoblazor/institutoblazor.env
sudo nano /etc/institutoblazor/institutoblazor.env
```

Completar:

- `ConnectionStrings__DefaultConnection`: usuario `instituto_app` y su clave.
- `AllowedHosts`: dominio, IP pública y `localhost` separados por `;`.
- Dejar `Database__ApplyMigrations=true` **solo** si el usuario tiene permisos DDL en el primer arranque o en una base nueva.

Verificar sin mostrar la clave:

```bash
sudo grep -v -i password /etc/institutoblazor/institutoblazor.env
```

## 9. Servicio systemd y script de despliegue

Clonar el repositorio en la VM (solo se usan los archivos de `deploy/` y la documentación; el artefacto llega por CI):

```bash
sudo -u instituto-deploy git clone https://github.com/LautaroDabrowski/NotaDeRoble.git \
  /home/instituto-deploy/InstitutoBlazor
cd /home/instituto-deploy/InstitutoBlazor
```

Instalar el servicio y el script:

```bash
sudo cp deploy/ubuntu/institutoblazor.service /etc/systemd/system/institutoblazor.service
sudo systemctl daemon-reload
sudo systemctl enable institutoblazor.service

sudo cp deploy/ubuntu/deploy.sh /usr/local/bin/deploy-institutoblazor
sudo chown root:root /usr/local/bin/deploy-institutoblazor
sudo chmod 755 /usr/local/bin/deploy-institutoblazor

sudo install -m 0440 -o root -g root \
  deploy/ubuntu/institutoblazor.sudoers /etc/sudoers.d/institutoblazor-deploy
sudo visudo -c
```

El sudoers solo permite reiniciar **este** servicio sin contraseña. El script crea una release nueva, cambia el symlink `current` de forma atómica, reinicia el servicio y espera a que `/health` responda.

## 10. Nginx

```bash
sudo cp deploy/ubuntu/nginx.conf /etc/nginx/sites-available/institutoblazor
sudo nano /etc/nginx/sites-available/institutoblazor   # cambiar server_name
sudo ln -sf /etc/nginx/sites-available/institutoblazor /etc/nginx/sites-enabled/institutoblazor
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

La configuración incluye `Upgrade`/`Connection` para WebSockets (circuito de Blazor) y desactiva el buffering del proxy.

## 11. Firewall y URL pública

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw enable
sudo ufw status
```

Opciones para la URL pública:

1. **Cloudflare Tunnel (usada en esta demo):** no requiere router ni IP pública. `cloudflared` se conecta desde el host a `http://127.0.0.1:8080` y entrega una URL HTTPS `*.trycloudflare.com`. Es temporal y cambia al reiniciar el túnel. Pasos completos en [`docs/VM-LOCAL.md`](VM-LOCAL.md).
2. **IP pública con redirección de puerto:** en el router, redirigir el puerto 80 externo a la IP de la VM o al host:8080. La URL queda `http://IP_PUBLICA`. Actualizar `AllowedHosts` y `server_name`.
3. **Dominio + TLS:** apuntar un registro A a la IP pública y ejecutar:

```bash
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d instituto.example.com
```

Certbot configura HTTPS y renovación automática. Usar HTTPS si se dispone de dominio.

Si el router está detrás de CGNAT (la IP WAN no coincide con la IP pública visible desde Internet), la redirección de puertos no funcionará. Alternativas: pedir IP pública al proveedor o usar un túnel (por ejemplo Cloudflare Tunnel) apuntando a `http://127.0.0.1:80`.

Comprobar desde otra red:

```bash
curl -I http://IP_PUBLICA/
curl -fsS http://IP_PUBLICA/health
```

> La sesión de Blazor usa `ProtectedSessionStorage` y claves de Data Protection persistidas en `/var/lib/institutoblazor/keys`. No borrar ese directorio entre despliegues: invalidaría las sesiones activas.

## 12. GitHub: repositorio, ramas y runner

### 12.1 Crear el repositorio y subir el código

En la máquina de desarrollo:

```bash
git init -b main
git add .
git commit -m "feat: versión integradora con validaciones, paginación, pruebas y despliegue"
git remote add origin https://github.com/LautaroDabrowski/NotaDeRoble.git
git branch develop
git push -u origin main develop
```

Crear las ramas de trabajo desde `develop` (`feature/…`) y abrir Pull Requests hacia `develop`; para publicar, PR de `develop` a `main`.

### 12.2 Proteger `main` y `develop`

En GitHub: **Settings → Branches → Add branch ruleset**:

- Requerir Pull Request antes de fusionar.
- Requerir que pasen los checks de CI.
- Bloquear force push y borrado.

### 12.3 Registrar el runner self-hosted

En **Settings → Actions → Runners → New self-hosted runner** (Linux x64), copiar los comandos. En la VM, como `instituto-deploy`:

```bash
su - instituto-deploy
mkdir -p ~/actions-runner && cd ~/actions-runner
curl -o actions-runner-linux-x64.tar.gz -L URL_QUE_MUESTRA_GITHUB
tar xzf actions-runner-linux-x64.tar.gz
./config.sh --url https://github.com/LautaroDabrowski/NotaDeRoble \
  --token TOKEN_QUE_MUESTRA_GITHUB \
  --name instituto-vm --labels instituto-production --unattended
exit

cd /home/instituto-deploy/actions-runner
sudo ./svc.sh install instituto-deploy
sudo ./svc.sh start
sudo ./svc.sh status
```

El label `instituto-production` debe coincidir con `runs-on` del workflow.

### 12.4 Habilitar el despliegue

En **Settings → Secrets and variables → Actions → Variables**, crear:

```text
DEPLOY_ENABLED = true
```

Sin esa variable, el job de despliegue queda omitido y CI sigue funcionando.

## 13. Primer despliegue

Opción A, manual desde la máquina de desarrollo:

```bash
dotnet publish InstitutoBlazor.csproj -c Release -o /tmp/instituto-app
scp -r /tmp/instituto-app USUARIO@IP_VM:/tmp/
ssh USUARIO@IP_VM
sudo -u instituto-deploy /usr/local/bin/deploy-institutoblazor /tmp/instituto-app
```

Opción B, automática: hacer merge a `main` y esperar el workflow **CI y despliegue Ubuntu**. El job `build` publica el artefacto y el job `deploy` lo activa en la VM.

Verificar:

```bash
systemctl status institutoblazor --no-pager
curl -fsS http://127.0.0.1:5000/health
curl -I http://127.0.0.1/
journalctl -u institutoblazor -n 50 --no-pager
```

## 14. Rollback y mantenimiento

Si `/health` falla después de un despliegue, el script devuelve error y deja la versión nueva activa para diagnóstico. Para volver a la anterior:

```bash
ls -1 /opt/institutoblazor/releases
sudo -u instituto-deploy ln -sfn /opt/institutoblazor/releases/RELEASE_ANTERIOR \
  /opt/institutoblazor/current
sudo systemctl restart institutoblazor
```

No se revierten migraciones automáticamente. Si el cambio incluyó una migración destructiva, restaurar el backup correspondiente.

Limpieza de releases antiguas (conservar las últimas 5):

```bash
ls -1dt /opt/institutoblazor/releases/release.* | tail -n +6 | sudo xargs -r rm -rf
```

Actualizar el runner:

```bash
sudo /home/instituto-deploy/actions-runner/bin/runner.listener status || true
# Seguir el aviso de GitHub cuando publique una versión nueva.
```

## 15. Diagnóstico rápido

| Síntoma | Revisar |
|---|---|
| 502 Bad Gateway | `systemctl status institutoblazor`; ¿Kestrel escucha en 5000? |
| `/health` Unhealthy | logs del servicio, credenciales y `docker logs institutoblazor-sqlserver` |
| Login no persiste tras reinicio | `/var/lib/institutoblazor/keys` existe y es escribible por `institutoblazor` |
| Circuito Blazor se desconecta | `proxy_read_timeout`, WebSocket y `X-Forwarded-*` en Nginx |
| Deploy falla por permisos | `realpath` del artefacto, dueño de `/opt/institutoblazor/releases`, `sudo -n systemctl restart` |
| SQL Server no inicia | contraseña de `sa` que cumpla la política; RAM suficiente |

## 16. Checklist de puesta en marcha

- [ ] Ubuntu actualizado y hora correcta.
- [ ] Runtime ASP.NET Core 10 instalado.
- [ ] SQL Server healthy y volumen persistente.
- [ ] Login `instituto_app` probado.
- [ ] `/etc/institutoblazor/institutoblazor.env` con permisos `640`.
- [ ] Servicio `institutoblazor` habilitado (`systemctl enable`).
- [ ] Nginx con `nginx -t` correcto.
- [ ] Firewall con solo 22/80/443.
- [ ] URL pública responde y `/health` devuelve `Healthy`.
- [ ] Runner conectado y variable `DEPLOY_ENABLED=true`.
- [ ] Push a `main` genera despliegue verde.
- [ ] Backup probado y restauración documentada.
