#!/usr/bin/env bash
set -euo pipefail
export DEBIAN_FRONTEND=noninteractive
export NEEDRESTART_MODE=a

if [[ $EUID -ne 0 ]]; then
    echo "Ejecutar con sudo: sudo bash $0 /ruta/a/app" >&2
    exit 1
fi

APP_DIR="${1:-}"
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
LOG=/var/log/instituto-setup.log
exec > >(tee -a "$LOG") 2>&1

log() { printf '\n==> %s\n' "$*"; }

if [[ -z "$APP_DIR" || ! -f "$APP_DIR/InstitutoBlazor.dll" ]]; then
    echo "Falta la carpeta publicada. Uso: sudo bash $0 /tmp/instituto-bundle/app" >&2
    exit 2
fi

log "Ampliando la raíz con el espacio libre del volumen (si existe)"
ROOT_FS="$(findmnt -no FSTYPE /)"
ROOT_SRC="$(findmnt -no SOURCE /)"
if command -v vgs >/dev/null 2>&1 && command -v lvextend >/dev/null 2>&1 && [[ "$ROOT_SRC" == /dev/mapper/* ]]; then
    VG="$(vgs --noheadings -o vg_name | awk 'NR==1{print $1}')"
    FREE="$(vgs --noheadings -o vg_free_count "$VG" 2>/dev/null | awk 'NR==1{print $1}')"
    if [[ -n "${VG:-}" && -n "${FREE:-}" && "$FREE" -gt 0 ]]; then
        lvextend -l +100%FREE "$ROOT_SRC"
        case "$ROOT_FS" in
            ext4) resize2fs "$ROOT_SRC" ;;
            xfs) xfs_growfs / ;;
        esac
    fi
fi
df -h /

log "Actualizando Ubuntu"
apt-get update
apt-get -y -o Dpkg::Options::=--force-confdef -o Dpkg::Options::=--force-confold upgrade
timedatectl set-timezone America/Argentina/Buenos_Aires

log "Instalando paquetes base"
apt-get install -y curl ca-certificates gnupg lsb-release ufw nginx git tar openssl

log "Instalando runtime ASP.NET Core 10"
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-runtimes 2>/dev/null | grep -q 'Microsoft.AspNetCore.App 10\.'; then
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    bash /tmp/dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet
    ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
fi
dotnet --list-runtimes | grep 'Microsoft.AspNetCore.App 10\.'

log "Instalando Docker"
if ! command -v docker >/dev/null 2>&1; then
    apt-get install -y docker.io
fi
systemctl enable --now docker

log "Creando usuarios y directorios"
id institutoblazor >/dev/null 2>&1 || useradd --system --home /opt/institutoblazor --shell /usr/sbin/nologin institutoblazor
id instituto-deploy >/dev/null 2>&1 || useradd --create-home --shell /bin/bash instituto-deploy
install -d -m 755 /opt/institutoblazor /etc/institutoblazor
install -d -o instituto-deploy -g institutoblazor -m 750 /opt/institutoblazor/releases
install -d -o institutoblazor -g institutoblazor -m 750 /var/lib/institutoblazor /var/lib/institutoblazor/keys

log "Generando secretos"
umask 077
if [[ ! -s /etc/institutoblazor/sqlserver.env ]]; then
    SA_PASS="$(openssl rand -base64 48 | tr -dc 'A-Za-z0-9' | head -c 24)Aa1!"
    printf 'MSSQL_SA_PASSWORD=%s\n' "$SA_PASS" > /etc/institutoblazor/sqlserver.env
fi
if [[ ! -s /etc/institutoblazor/app-db-password ]]; then
    APP_PASS="$(openssl rand -base64 48 | tr -dc 'A-Za-z0-9' | head -c 24)Aa1!"
    printf '%s\n' "$APP_PASS" > /etc/institutoblazor/app-db-password
fi
APP_PASS="$(cat /etc/institutoblazor/app-db-password)"
if [[ ! -s /etc/institutoblazor/institutoblazor.env ]]; then
    cat > /etc/institutoblazor/institutoblazor.env <<ENV
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:5000
ConnectionStrings__DefaultConnection=Server=127.0.0.1,1433;Database=InstitutoBlazorDemo;User Id=instituto_app;Password=${APP_PASS};Encrypt=True;TrustServerCertificate=True;Connect Timeout=10
Database__ApplyMigrations=true
DataProtection__KeysPath=/var/lib/institutoblazor/keys
AllowedHosts=*
ENV
fi
chown root:institutoblazor /etc/institutoblazor/institutoblazor.env
chmod 640 /etc/institutoblazor/institutoblazor.env
chmod 600 /etc/institutoblazor/sqlserver.env /etc/institutoblazor/app-db-password
source /etc/institutoblazor/sqlserver.env

log "Levantando SQL Server"
if ! docker ps -a --format '{{.Names}}' | grep -qx institutoblazor-sqlserver; then
    docker run -d --name institutoblazor-sqlserver \
        --restart unless-stopped \
        -e ACCEPT_EULA=Y -e MSSQL_PID=Express \
        -e MSSQL_SA_PASSWORD="$MSSQL_SA_PASSWORD" \
        -p 127.0.0.1:1433:1433 \
        -v institutoblazor-sqlserver-data:/var/opt/mssql \
        mcr.microsoft.com/mssql/server:2022-latest
fi

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
READY=0
for attempt in {1..120}; do
    if docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver test -x "$SQLCMD" 2>/dev/null \
        && docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver \
            "$SQLCMD" -S localhost -U sa -C -Q 'SELECT 1' -b -o /dev/null 2>/dev/null; then
        READY=1
        break
    fi
    sleep 2
done
if [[ $READY -ne 1 ]]; then
    echo "SQL Server no inició a tiempo" >&2
    docker logs institutoblazor-sqlserver --tail 80 >&2 || true
    exit 1
fi

log "Creando base y usuario de la aplicación"
docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver "$SQLCMD" \
    -S localhost -U sa -C -b \
    -Q "IF DB_ID(N'InstitutoBlazorDemo') IS NULL CREATE DATABASE InstitutoBlazorDemo;"
docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver "$SQLCMD" \
    -S localhost -U sa -C -b \
    -Q "IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name=N'instituto_app') CREATE LOGIN instituto_app WITH PASSWORD=N'${APP_PASS}';"
docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver "$SQLCMD" \
    -S localhost -U sa -C -b -d InstitutoBlazorDemo \
    -Q "IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'instituto_app') CREATE USER instituto_app FOR LOGIN instituto_app;"
docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" institutoblazor-sqlserver "$SQLCMD" \
    -S localhost -U sa -C -b -d InstitutoBlazorDemo \
    -Q "ALTER ROLE db_owner ADD MEMBER instituto_app;"

log "Instalando la aplicación"
release=/opt/institutoblazor/releases/release.initial
rm -rf "$release"
cp -a "$APP_DIR" "$release"
chown -R instituto-deploy:institutoblazor "$release" /opt/institutoblazor
chmod -R u=rwX,g=rX,o= "$release"
chmod 750 /opt/institutoblazor /opt/institutoblazor/releases
ln -sfn "$release" /opt/institutoblazor/current

log "Instalando servicio y script de despliegue"
install -m 644 "$SCRIPT_DIR/institutoblazor.service" /etc/systemd/system/institutoblazor.service
install -m 755 -o root -g root "$SCRIPT_DIR/deploy.sh" /usr/local/bin/deploy-institutoblazor
install -m 440 -o root -g root "$SCRIPT_DIR/institutoblazor.sudoers" /etc/sudoers.d/institutoblazor-deploy
visudo -cf /etc/sudoers.d/institutoblazor-deploy
systemctl daemon-reload
systemctl enable institutoblazor
systemctl restart institutoblazor

log "Configurando Nginx"
cat > /etc/nginx/sites-available/institutoblazor <<'NGINX'
map $http_upgrade $instituto_connection_upgrade {
    default upgrade;
    '' close;
}

server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $instituto_connection_upgrade;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 3600s;
        proxy_send_timeout 3600s;
        proxy_buffering off;
    }
}
NGINX
ln -sfn /etc/nginx/sites-available/institutoblazor /etc/nginx/sites-enabled/institutoblazor
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl enable nginx
systemctl reload nginx

log "Configurando firewall"
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

log "Esperando /health"
HEALTHY=0
for attempt in {1..60}; do
    if curl -fsS --max-time 3 http://127.0.0.1:5000/health >/dev/null; then
        HEALTHY=1
        break
    fi
    sleep 2
done
if [[ $HEALTHY -ne 1 ]]; then
    echo "La aplicación no respondió. Logs:" >&2
    journalctl -u institutoblazor -n 100 --no-pager >&2 || true
    exit 1
fi

log "Verificación final"
curl -fsS http://127.0.0.1:5000/health; echo
curl -fsS http://127.0.0.1/health; echo
log "Listo. Log completo: $LOG"
