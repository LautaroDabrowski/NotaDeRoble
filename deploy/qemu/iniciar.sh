#!/usr/bin/env bash
set -euo pipefail
DISK="${DISK:-$HOME/VMs/InstitutoBlazor/instituto.raw}"
exec qemu-system-x86_64 \
  -name InstitutoBlazor \
  -enable-kvm -machine q35 -cpu host -smp 4 -m 6144 \
  -drive file="$DISK",if=virtio,format=raw,cache=writeback,discard=unmap \
  -boot order=c \
  -netdev user,id=net0,hostfwd=tcp:127.0.0.1:2222-:22,hostfwd=tcp:0.0.0.0:8080-:80 \
  -device virtio-net-pci,netdev=net0 \
  -device virtio-vga \
  -display gtk \
  -usb -device usb-tablet
