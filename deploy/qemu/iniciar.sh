#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
DISK="${DISK:-$HOME/VMs/InstitutoBlazor/instituto.raw}"
URI="${LIBVIRT_URI:-qemu:///session}"
DOMAIN="${DOMAIN:-InstitutoBlazor}"

[[ -f "$DISK" ]] || { printf 'No existe el disco %s\n' "$DISK" >&2; exit 1; }
command -v virsh >/dev/null 2>&1 || { printf 'Falta virsh (libvirt).\n' >&2; exit 1; }
command -v passt >/dev/null 2>&1 || { printf 'Falta passt: sudo dnf install passt\n' >&2; exit 1; }

if ! virsh -c "$URI" dominfo "$DOMAIN" >/dev/null 2>&1; then
    tmp="$(mktemp)"
    sed "s|/ruta/al/disco/instituto.raw|$DISK|g" "$SCRIPT_DIR/InstitutoBlazor.xml" >"$tmp"
    virsh -c "$URI" define "$tmp"
    rm -f "$tmp"
fi

if ! virsh -c "$URI" domid "$DOMAIN" >/dev/null 2>&1; then
    virsh -c "$URI" start "$DOMAIN"
fi

if command -v virt-manager >/dev/null 2>&1; then
    exec virt-manager --connect "$URI" --show-domain-console "$DOMAIN"
fi

printf 'VM %s iniciada en %s. Abrí virt-manager y conectate a esa URI.\n' "$DOMAIN" "$URI"
