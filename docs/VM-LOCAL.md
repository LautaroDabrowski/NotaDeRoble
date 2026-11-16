# VM local con QEMU/KVM

Guía de la VM usada para la demo: Ubuntu Server 26.04 LTS sobre QEMU/KVM en Fedora 44, con NAT y reenvío de puertos al host.

## 1. Requisitos del host

| Recurso | Valor usado |
|---|---|
| RAM de la VM | 6 GB |
| vCPU | 4 |
| Disco | 25 GB dinámicos (`raw` disperso) |
| Aceleración | KVM (`/dev/kvm`) |
| Red | NAT de QEMU + reenvío de puertos |

Comprobar KVM:

```bash
ls -l /dev/kvm
qemu-system-x86_64 --version
```

La ISO usada está en `~/Downloads/ubuntu-26.04.1-live-server-amd64.iso`.

## 2. Crear el disco y arrancar el instalador

Los scripts están en `deploy/qemu/`:

```bash
mkdir -p ~/VMs/InstitutoBlazor
truncate -s 25G ~/VMs/InstitutoBlazor/instituto.raw

# Instalador (ISO adjunta, arranque desde DVD)
./deploy/qemu/instalar.sh
```

Si la ISO o el disco están en otra ruta:

```bash
ISO=/ruta/ubuntu.iso DISK=/ruta/instituto.raw ./deploy/qemu/instalar.sh
```

### Instalación de Ubuntu

1. Idioma y teclado.
2. Red: DHCP (QEMU entrega `10.0.2.15`).
3. Almacenamiento: usar todo el disco virtual.
4. Perfil de referencia: usuario `lauty`, servidor `instituto-vm`.
5. Marcar **Install OpenSSH server**.
6. No instalar snaps adicionales.
7. Cuando pida reiniciar, cerrar la ventana de QEMU.

## 3. Arrancar la VM ya instalada

```bash
./deploy/qemu/iniciar.sh
```

La VM sigue usando NAT de QEMU. El host reenvía:

| Host | VM | Uso |
|---|---|---|
| `127.0.0.1:2222` | `22` | SSH |
| `0.0.0.0:8080` | `80` | HTTP público local |

Acceso:

```bash
ssh -p 2222 lauty@127.0.0.1
```

## 4. Instalación automática del stack

Copiar el paquete desde el host:

```bash
scp -P 2222 /tmp/opencode/instituto-bundle.tar.gz lauty@127.0.0.1:/tmp/
```

Dentro de la VM:

```bash
sudo rm -rf /tmp/instituto-bundle
sudo mkdir -p /tmp/instituto-bundle
sudo tar xzf /tmp/instituto-bundle.tar.gz -C /tmp/instituto-bundle
sudo bash /tmp/instituto-bundle/deploy/ubuntu/setup-vm.sh /tmp/instituto-bundle/app
```

El script instala .NET 10, Docker, SQL Server, Nginx, systemd y UFW; genera las contraseñas, crea la base y publica la app. Log: `/var/log/instituto-setup.log`.

## 5. URL pública

### Opción A: Cloudflare Tunnel (usada en la demo)

No requiere tocar el router ni el ISP. La URL es temporal y cambia si se reinicia el túnel.

```bash
mkdir -p ~/.local/bin
curl -fL -o ~/.local/bin/cloudflared \
  https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
chmod +x ~/.local/bin/cloudflared

systemd-run --user --unit=instituto-tunnel \
  --property=Restart=on-failure --property=RestartSec=5 \
  ~/.local/bin/cloudflared tunnel --url http://127.0.0.1:8080 --no-autoupdate
```

Obtener la URL:

```bash
journalctl --user -u instituto-tunnel -n 60 --no-pager \
  | grep -oE 'https://[a-z0-9-]+\.trycloudflare\.com' | tail -1
```

Estado y detención:

```bash
systemctl --user status instituto-tunnel
systemctl --user stop instituto-tunnel
```

### Opción B: redirección en el router

Si el router permite administración y redirección de puertos:

- Puerto externo **80** → `192.168.1.41:8080` (IP del host Fedora), TCP.
- Si el ISP bloquea el 80: externo **8080** → `192.168.1.41:8080` y URL con `:8080`.
- Verificar que la IP WAN no sea CGNAT (`100.64.0.0/10`).

En esta instalación el router no respondió en `192.168.1.1` (HTTP/HTTPS) y UPnP estaba deshabilitado, por eso se usó el túnel.

## 6. Durante la presentación

- Mantener abierta la ventana de QEMU o el proceso `iniciar.sh`.
- Mantener el host Fedora despierto (Configuración de energía → nunca suspender).
- No detener `instituto-tunnel.service`; si se reinicia, cambia la URL.
- La app queda accesible en `http://127.0.0.1:8080` desde el host y en la URL de Cloudflare desde Internet.

## 7. Apagado y copia de seguridad

Apagar Ubuntu con `sudo poweroff` o cerrar la ventana de QEMU. Con la VM detenida, el disco completo se puede copiar:

```bash
cp --sparse=always ~/VMs/InstitutoBlazor/instituto.raw \
  ~/VMs/InstitutoBlazor/instituto-$(date +%F).raw
```

## 8. Nota sobre VirtualBox

VirtualBox 7.2.22 está instalado, pero en este equipo el kernel en ejecución (7.2.5) no tiene `kernel-devel` disponible en los repos; compilar `vboxdrv` requiere actualizar a 7.3.3 y reiniciar. Se optó por QEMU/KVM porque ya funcionaba sin esos pasos.
