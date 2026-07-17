# Correccion de instalacion de Visual Studio en la VM

Fecha: 17 de julio de 2026.

## Problema observado

El preparador encontraba `Visual Studio Installer`, pero no una instalacion terminada de Visual Studio 2022. Esto ocurre cuando se descarga el instalador y aun no se completa la instalacion del producto.

## Correccion

`INSTALAR_COMPONENTES_VISUAL_STUDIO_WINDOWS.ps1` ahora:

1. Detecta una instalacion existente de Visual Studio 2022.
2. Busca un instalador ya descargado en `Downloads` o `Descargas`.
3. Cuando no encuentra uno, instala Visual Studio 2022 Community mediante WinGet.
4. Como ultimo recurso descarga el instalador oficial de Microsoft.
5. Instala las cargas `Microsoft.VisualStudio.Workload.ManagedDesktop` y `Microsoft.VisualStudio.Workload.NetWeb`.
6. Verifica el SDK .NET 8 y lo instala mediante WinGet cuando falta.
7. Continua con la configuracion, compilacion y validacion de INDOTEL.

## Ejecucion

Ejecutar como administrador:

```text
PREPARAR_VM_WINDOWS_COMPLETA.bat
```

La instalacion puede tardar y requiere conexion a Internet. No se debe cerrar la ventana durante la instalacion.

## Artefactos

- `INDOTEL_PROYECTO_COMPLETO_VM_WHATSAPP_FINAL_V2.zip`
- SHA-256: `198ed582568bcfaf987bb824c4c718e11894cbc1b81566ef8289bedb7bd3c5fe`
- `PARCHE_VISUAL_STUDIO_VM_V2.zip`
- SHA-256: `c90fe07ab4fc8b3c599e3c0fe336de9eeedc541c5573e687493ac5d486894cf9`
