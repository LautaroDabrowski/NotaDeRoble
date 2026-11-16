#!/usr/bin/env bash
set -euo pipefail
ISO="${ISO:-$HOME/Downloads/ubuntu-26.04.1-live-server-amd64.iso}"
DISK="${DISK:-$HOME/VMs/InstitutoBlazor/instituto.raw}"
mkdir -p "$(dirname "$DISK")"
[[ -f "$DISK" ]] || truncate -s 25G "$DISK"
exec qemu-system-x86_64 \
  -name InstitutoBlazor \
  -enable-kvm -machine q35 -cpu host -smp 4 -m 6144 \
  -drive file="$DISK",if=virtio,format=raw,cache=writeback,discard=unmap \
  -cdrom "$ISO" \
  -boot order=d \
  -netdev user,id=net0,hostfwd=tcp:127.0.0.1:2222-:22,hostfwd=tcp:0.0.0.0:8080-:80 \
  -device virtio-net-pci,netdev=net0 \
  -device virtio-vga \
  -display gtk \
  -usb -device usb-tablet
