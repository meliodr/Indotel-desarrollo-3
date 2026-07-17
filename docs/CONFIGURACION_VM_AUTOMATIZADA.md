# Configuración automatizada Ubuntu + VirtualBox Windows

La entrega está preparada para enviarse por WhatsApp como **Documento ZIP**, descargarse en Windows y extraerse directamente dentro de **Descargas**. No es obligatorio mover el proyecto a `C:\INDOTEL`.

## Distribución de componentes

- Ubuntu físico con Docker: SQL Server, Core API, API Gateway, Portal Web y proveedor externo de pagos.
- Windows 11 en VirtualBox: Caja WPF, GUI administrativa WinForms y navegador.
- Caja consume únicamente el Gateway.
- GUI administrativa consume el Core API.
- Ninguna aplicación de escritorio se conecta directamente a SQL Server.

## Transferencia por WhatsApp

1. En Ubuntu, envíe `INDOTEL_PROYECTO_COMPLETO_VM_WHATSAPP_FINAL.zip` como **Documento**.
2. En Windows, descargue el ZIP en `Descargas`.
3. Pulse **Extraer todo**.
4. Abra la carpeta extraída `INDOTEL-Proyecto-Completo`.
5. Ejecute `PREPARAR_VM_WINDOWS_COMPLETA.bat`.
6. Después ejecute `INICIAR_APLICACIONES_VM_WINDOWS.bat`.

Todos los scripts usan su propia ubicación mediante `%~dp0` o `$PSScriptRoot`; por eso funcionan desde Descargas, Escritorio u otra carpeta.

## Automatización incluida

`PREPARAR_VM_WINDOWS_COMPLETA.bat`:

- solicita permisos administrativos;
- comprueba Visual Studio 2022;
- instala o verifica las cargas **Desarrollo de escritorio de .NET** y **ASP.NET y desarrollo web**;
- comprueba el SDK .NET 8;
- detecta el host Ubuntu automáticamente;
- configura `INDOTEL_GATEWAY_URL`, `INDOTEL_CORE_URL` e `INDOTEL_WEB_URL`;
- actualiza los `appsettings.json` locales de respaldo;
- prueba Core, Gateway y Web;
- restaura y compila Caja WPF;
- restaura y compila Core WinForms;
- crea accesos directos en el escritorio;
- deja evidencia en `CONFIGURACION_VM_APLICADA.txt`.

La configuración inicial incluida apunta a:

```text
Ubuntu:  192.168.1.124
Core:    http://192.168.1.124:5085
Gateway: http://192.168.1.124:5185
Web:     http://192.168.1.124:5234
```

Si cambia la IP, el script prueba primero la dirección guardada y luego busca automáticamente un host INDOTEL en las redes privadas de Windows.

## Requisito de red

VirtualBox debe utilizar **Adaptador puente**. Los servicios de Ubuntu deben mostrar estado `healthy` y publicar:

```text
5085 Core API
5185 API Gateway
5234 Portal Web
```

## Diagnóstico

En caso de error revise:

```text
CONFIGURACION_VM_DIAGNOSTICO.txt
CONFIGURACION_VM_APLICADA.txt
```

No ejecute Docker Desktop dentro de la máquina virtual. Docker permanece en Ubuntu.
