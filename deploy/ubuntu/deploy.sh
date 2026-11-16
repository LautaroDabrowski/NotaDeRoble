#!/usr/bin/env bash
# Instalar como /usr/local/bin/deploy-institutoblazor (propiedad de root).
# Se ejecuta como instituto-deploy; sudo solo se usa para reiniciar este servicio.
set -euo pipefail

if [[ $# -ne 1 ]]; then
    printf 'Uso: deploy-institutoblazor /ruta/al/artefacto\n' >&2
    exit 2
fi

source_dir="$(realpath -- "$1")"
base=/opt/institutoblazor
if [[ ! -f "$source_dir/InstitutoBlazor.dll" || ! -d "$source_dir/wwwroot" ]]; then
    printf 'El directorio no contiene una publicación válida.\n' >&2
    exit 2
fi

# Serializa también los despliegues manuales.
exec 9>"$base/.deploy.lock"
flock -n 9 || { printf 'Ya hay otro despliegue en curso.\n' >&2; exit 1; }
release="$(mktemp -d "$base/releases/release.XXXXXXXX")"
cp -a "$source_dir/." "$release/"
chmod -R u=rwX,go=rX "$release"
previous="$(readlink "$base/current" || true)"
ln -s "$release" "$base/.current-$$"
mv -Tf "$base/.current-$$" "$base/current"
sudo -n /usr/bin/systemctl restart institutoblazor.service

for attempt in {1..45}; do
    if curl --fail --silent --max-time 3 http://127.0.0.1:5000/health >/dev/null; then
        printf 'Despliegue saludable: %s\n' "$release"
        exit 0
    fi
    sleep 2
done

printf 'Falló /health. Revisar journalctl -u institutoblazor. Versión anterior: %s\n' "$previous" >&2
# No se revierte automáticamente una migración de datos.
exit 1
