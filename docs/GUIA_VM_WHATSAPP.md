# Guía rápida: proyecto INDOTEL por WhatsApp

## En Ubuntu

Envíe el archivo final como **Documento** por WhatsApp:

```text
INDOTEL_PROYECTO_COMPLETO_VM_WHATSAPP_FINAL.zip
```

No necesita ejecutar comandos de transferencia, servidor Python ni volver a construir imágenes Docker.

## En Windows VirtualBox

1. Descargue el ZIP desde WhatsApp Web.
2. En Descargas, pulse clic derecho → **Extraer todo**.
3. Entre a `INDOTEL-Proyecto-Completo`.
4. Ejecute:

```text
PREPARAR_VM_WINDOWS_COMPLETA.bat
```

5. Cuando indique que la VM está preparada, ejecute:

```text
INICIAR_APLICACIONES_VM_WINDOWS.bat
```

## Aplicaciones que abrirá

- Portal Web en el navegador.
- Core administrativo WinForms.
- Caja WPF.

## Credenciales académicas

```text
Administrador: admin@indotel.test / Admin123*
Cajero:       cajero@indotel.test / Caja123*
```

## Importante

- Ubuntu debe permanecer encendido.
- Los contenedores deben permanecer `healthy`.
- VirtualBox debe estar en Adaptador puente.
- La Caja usa el Gateway; el Core administrativo usa el Core API.
