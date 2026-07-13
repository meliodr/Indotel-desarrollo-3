# Mapa de auditoria y alcance del Core INDOTEL

Este documento sirve para pensar antes de seguir programando. Su objetivo es evitar que el Core quede incompleto, duplicado o desconectado de lo que necesitan Web, Caja y Gateway.

## 1. Regla principal

Antes de tocar codigo nuevo, se debe responder:

```text
1. Que modulo del Core estamos completando?
2. Ya existe algo parecido?
3. Que endpoint necesita Web?
4. Que endpoint necesita Caja?
5. Que tabla o modelo toca?
6. Requiere migracion?
7. Requiere rol o permiso?
8. Requiere auditoria?
9. Requiere documentacion?
10. Como se prueba en Swagger?
```

## 2. Estado general del Core

```text
Core funcional para demostracion academica: SI
Core completo para produccion real: NO TODAVIA
```

El Core ya cubre el flujo principal:

```text
Login -> Token -> Catalogos -> Ciudadano -> Reclamacion -> Estado -> Respuesta -> Historial -> Reportes
```

Pero todavia faltan piezas de control institucional:

```text
Usuarios completos, permisos por rol, maquina de estados estricta, auditoria automatica, archivos, filtros, SLA y notificaciones.
```

## 3. Inventario por modulo

| Modulo | Estado | Que tenemos | Que falta |
|---|---|---|---|
| Configuracion base | Completo | ASP.NET Core, Swagger, Docker, SQL Server | Revisar nombres y estandarizar rutas |
| Base de datos | Parcial alto | Migracion inicial, tablas principales | Relaciones mas fuertes y migraciones futuras |
| Auth/Login | Completo para MVP | Login JWT, admin semilla, /me | Registro de usuarios, refresh/logout opcional |
| Roles | Parcial | Tabla Roles y claims en token | RBAC estricto por endpoint |
| Usuarios | Parcial bajo | Usuario admin inicial | CRUD usuarios, activar/desactivar, cambiar password |
| Ciudadanos | Parcial alto | Crear, listar, ver por id | Buscar por cedula, editar, validar JCE simulada |
| Prestadoras | Parcial | Seed y catalogo | CRUD completo, usuarios vinculados a prestadora |
| ServiciosTelecom | Parcial | Seed y catalogo | CRUD completo si Caja lo necesita |
| Reclamaciones | Parcial alto | Crear, listar, detalle, estado, respuesta | Flujo estricto, filtros, cancelar/cerrar/resolver separados |
| Historial | Completo MVP | Historial por reclamacion | Agregar mas detalle de usuario/IP |
| Auditoria | Parcial bajo | Modelo creado | Servicio automatico y endpoints de consulta |
| Documentos | Parcial bajo | Modelo DocumentoReclamacion | Carga real, validacion, descarga/listado |
| Reportes | Parcial medio | Resumen, por estado, por prestadora | Reportes por fecha, vencidas, tiempos, servicios |
| Integraciones | Faltante | Ninguna real | JCE simulada, validaciones externas simuladas |
| SLA | Faltante | Ninguno | Fecha limite, vencimiento, estados automaticos |
| Notificaciones | Faltante | Ninguna | Email simulado, notificacion por cambio de estado |
| Middleware/Gateway | Fuera del Core | Documentado como integracion | Contrato final cuando exista Gateway |
| Pruebas | Faltante | Pruebas manuales en Swagger | Tests automaticos |

## 4. Contrato actual con Web

La Web se enfoca en el ciudadano.

Debe cubrir:

- Inicio publico.
- Registro ciudadano.
- Login.
- Panel ciudadano.
- Crear reclamacion.
- Mis reclamaciones.
- Detalle de reclamacion.
- Subir documento.
- Consultar expediente.
- Informacion institucional.

### Endpoints que Web necesita y estado actual

| Necesidad Web | Endpoint pedido | Estado actual del Core | Decision |
|---|---|---|---|
| Login | POST /api/auth/login | Existe | Mantener |
| Registro ciudadano/cuenta | POST /api/auth/register-ciudadano | No existe | Definir si sera Auth o Ciudadanos |
| Perfil ciudadano | GET /api/ciudadanos/{id} | Existe | Mantener |
| Actualizar perfil | PUT /api/ciudadanos/{id} | No existe | Agregar |
| Catalogo prestadoras | GET /api/prestadoras | No exacto; existe GET /api/catalogos/prestadoras | Ajustar documento o crear alias |
| Catalogo servicios | GET /api/servicios | No exacto; existe GET /api/catalogos/servicios | Ajustar documento o crear alias |
| Crear reclamacion | POST /api/reclamaciones | Existe | Mantener |
| Mis reclamaciones | GET /api/ciudadanos/{id}/reclamaciones | No existe | Agregar o filtrar /api/reclamaciones |
| Detalle reclamacion | GET /api/reclamaciones/{id} | Existe | Mantener |
| Historial | GET /api/reclamaciones/{id}/historial | Existe | Mantener |
| Subir documento | POST /api/reclamaciones/{id}/documentos | No existe | Agregar |
| Consulta por expediente | GET /api/reclamaciones/expediente/{numero} | No existe | Agregar |

## 5. Contrato actual con Caja

Caja se enfoca en personal interno/ventanilla.

Debe cubrir:

- Login de personal interno.
- Panel principal.
- Buscar ciudadano.
- Registrar reclamacion desde ventanilla.
- Bandeja de reclamaciones.
- Detalle de reclamacion.
- Cambiar estado.
- Respuesta de prestadora.
- Auditoria del caso.
- Reportes basicos.

### Endpoints que Caja necesita y estado actual

| Necesidad Caja | Endpoint pedido | Estado actual del Core | Decision |
|---|---|---|---|
| Health | GET /health | Existe | Mantener |
| Login | POST /api/auth/login | Existe | Mantener |
| Usuarios | GET /api/usuarios | No existe | Agregar |
| Usuario por id | GET /api/usuarios/{id} | No existe | Agregar |
| Activar/desactivar usuario | PATCH /api/usuarios/{id}/estado | No existe | Agregar |
| Listar ciudadanos | GET /api/ciudadanos | Existe | Mantener |
| Ciudadano por id | GET /api/ciudadanos/{id} | Existe | Mantener |
| Buscar por cedula | GET /api/ciudadanos/cedula/{cedula} | No existe | Agregar |
| Registrar ciudadano | POST /api/ciudadanos | Existe | Mantener |
| Actualizar ciudadano | PUT /api/ciudadanos/{id} | No existe | Agregar |
| Prestadoras | GET /api/prestadoras | No exacto; existe /api/catalogos/prestadoras | Crear alias o ajustar contrato |
| Servicios | GET /api/servicios | No exacto; existe /api/catalogos/servicios | Crear alias o ajustar contrato |
| Reporte resumen | GET /api/reportes/resumen | Existe | Mantener |
| Reporte estado | GET /api/reportes/reclamaciones-por-estado | Existe | Mantener |
| Reporte prestadora | GET /api/reportes/reclamaciones-por-prestadora | Existe | Mantener |
| Crear reclamacion | POST /api/reclamaciones | Existe | Mantener |
| Listar reclamaciones | GET /api/reclamaciones | Existe | Agregar filtros |
| Detalle reclamacion | GET /api/reclamaciones/{id} | Existe | Mantener |
| Cambiar estado | PATCH/PUT /api/reclamaciones/{id}/estado | Existe como PUT | Documentar decision o aceptar ambos |
| Adjuntar documento | POST /api/reclamaciones/{id}/documentos | No existe | Agregar |
| Enviar a prestadora | POST /api/reclamaciones/{id}/enviar-prestadora | No existe | Puede resolverse con cambio de estado o endpoint dedicado |
| Resolver | POST /api/reclamaciones/{id}/resolver | No existe | Puede resolverse con cambio de estado o endpoint dedicado |
| Cerrar | POST /api/reclamaciones/{id}/cerrar | No existe | Puede resolverse con cambio de estado o endpoint dedicado |
| Auditoria por reclamacion | GET /api/auditoria/reclamacion/{id} | No existe | Agregar cuando exista AuditoriaService |

## 6. Decisiones que debemos tomar antes de programar

### Decision 1: Rutas de catalogos

Actualmente existen:

```text
GET /api/catalogos/prestadoras
GET /api/catalogos/servicios
```

Los documentos de Web/Caja tambien mencionan:

```text
GET /api/prestadoras
GET /api/servicios
```

Decision pendiente:

- Opcion A: mantener solo `/api/catalogos/...` y actualizar documentos.
- Opcion B: crear alias `/api/prestadoras` y `/api/servicios` para facilitar a Web/Caja.

Recomendacion: crear alias simples para no confundir a los integrantes.

### Decision 2: PUT o PATCH para cambiar estado

Actualmente existe:

```text
PUT /api/reclamaciones/{id}/estado
```

Caja menciona:

```text
PATCH /api/reclamaciones/{id}/estado
```

Decision pendiente:

- Opcion A: documentar que se usa PUT.
- Opcion B: aceptar PUT y PATCH apuntando a la misma accion.

Recomendacion: aceptar ambos para evitar errores.

### Decision 3: Registro ciudadano

Web menciona:

```text
POST /api/auth/register-ciudadano
```

Actualmente existe:

```text
POST /api/ciudadanos
```

Decision pendiente:

- Opcion A: ciudadano se registra como ciudadano sin usuario de login.
- Opcion B: registro ciudadano crea Ciudadano + Usuario + Rol Ciudadano.

Recomendacion: implementar `register-ciudadano` cuando hagamos CRUD de usuarios.

### Decision 4: Endpoints especificos vs cambio de estado generico

Caja menciona:

```text
POST /api/reclamaciones/{id}/enviar-prestadora
POST /api/reclamaciones/{id}/resolver
POST /api/reclamaciones/{id}/cerrar
```

Actualmente se hace con:

```text
PUT /api/reclamaciones/{id}/estado
```

Decision pendiente:

- Opcion A: usar solo cambio de estado generico.
- Opcion B: crear endpoints especificos que internamente llamen al cambio de estado.

Recomendacion: primero maquina de estados estricta; luego crear endpoints especificos si hace falta para Caja.

## 7. Prioridad de documentacion antes de codigo

Antes de programar mejoras, crear o actualizar:

- [ ] `docs/CORE_ROUTE_DECISIONS.md`.
- [ ] `docs/CORE_ROLE_MATRIX.md`.
- [ ] `docs/CORE_STATE_MACHINE.md`.
- [ ] `docs/CORE_WEB_CAJA_CONTRACT.md`.
- [ ] `docs/CORE_NEXT_IMPLEMENTATION_PLAN.md`.

## 8. Prioridad de codigo despues de documentar

Orden recomendado:

1. Alias/rutas compatibles para Web y Caja.
2. CRUD de usuarios.
3. RBAC por roles.
4. Buscar ciudadano por cedula y editar ciudadano.
5. Maquina de estados estricta.
6. Endpoints por expediente y mis reclamaciones.
7. Documentos/evidencias.
8. Auditoria automatica.
9. Filtros y paginacion.
10. SLA y notificaciones.

## 9. Definicion de no olvidar nada

No se considera listo para seguir codigo hasta que:

- [ ] Web tenga contrato claro de endpoints actuales y faltantes.
- [ ] Caja tenga contrato claro de endpoints actuales y faltantes.
- [ ] Las rutas duplicadas o distintas esten decididas.
- [ ] Los roles y permisos esten definidos.
- [ ] Los estados y transiciones esten definidos.
- [ ] Cada mejora tenga prueba Swagger definida.
- [ ] Cada mejora tenga impacto de base de datos revisado.

## 10. Conclusion

El Core ya funciona como base central del sistema. Ahora la prioridad no es seguir agregando codigo sin pensar, sino cerrar decisiones de arquitectura y contrato. Despues de eso, cada mejora debe entrar con una razon clara y una prueba clara.
