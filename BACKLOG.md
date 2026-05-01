# Backlog y documentación viva — Presupuestos

**Hoja de ruta del proyecto.** Agentes y personas: al arrancar trabajo en este repo, leer este archivo primero; aquí está el orden sugerido de tareas y el resumen de qué hace la app. Tras cada entrega, actualizar checkboxes y la sección *Documentación* si cambió el comportamiento público.

**Convención**

- `- [ ]` pendiente · `- [x]` hecho
- Orden: arriba lo más próximo o bloqueante
- Rutas API: `/api/…` · Cliente: `/…`

---

## Documentación — qué hace la app

### Visión

App **multi-tenant** para gestionar **insumos**, **servicios** (recetas de insumos + márgenes), **presupuestos** y (opcional) **reglas de precio flexibles**. Los datos están acotados por **tenant** salvo el **super administrador global**, que opera usuarios y estadísticas sin tenant en JWT.

### Backend (.NET)

| Área | Rol |
|------|-----|
| **Auth** | Registro (crea tenant + usuario admin), login, refresh, logout. JWT: `sub`, `email`, `role`, `tenant_id` o `super_admin` sin tenant. Respuesta de tokens incluye `tenantName` (nombre de organización) para la UI. |
| **Tenant** | Resolución por JWT o cabeceras `X-Tenant-Id` / `X-User-Id` en integraciones. Middleware valida cuenta activa y no vencida. |
| **Insumos (Items)** | CRUD por tenant. |
| **Servicios** | Catálogo por tenant; líneas `ServiceItem` (insumo + cantidad). |
| **Presupuestos** | Alta con líneas por servicio; totales según margen / precio flexible si el tenant lo tiene activo. |
| **Pricing rules** | Reglas por tenant cuando *flexible pricing* está habilitado. |
| **Admin global** | Rutas bajo `/api/admin/…`: listado/detalle usuarios, toggle `IsActive`, estadísticas. Política `SuperAdmin` + claim `super_admin`. |
| **App config / devices** | Config remota y control de dispositivos (según opciones). |
| **Suscripciones + MP** | Planes `Plans` (Free/Pro/Premium), `Subscriptions` por **tenant** (B2B); `POST /api/Subscriptions/create`, `GET /api/Subscriptions/me` usan contexto de organización (`tenant_id` o cabeceras de integración); webhook `POST /api/webhooks/mercadopago` (validación del pago vía API MP). Middleware `SubscriptionAccess` (`SubscriptionAccess:Enforce`) tras tenant; bypass Auth, admin, webhooks, create/me. |

Migraciones EF: al arranque de la API se ejecuta `Database.MigrateAsync()` (revisar política en prod).

### Frontend (Vite + React)

| Ruta | Uso |
|------|-----|
| `/login` | Sesión; en registro, campo *Organización* con ayuda contextual. |
| `/` | Panel tenant o panel reducido super admin. |
| `/items`, `/services`, `/services/:id`, `/budgets`, `/budgets/new` | Flujo operativo tenant. Navegación: **sidebar** (fijo en escritorio ≥900px; en móvil menú ☰ + drawer). |
| `/admin` | Solo super admin: usuarios, stats, activar/desactivar, detalle (incluye `expirationDate`, dispositivos, tenant). |

Autorización en cliente: detección de super admin leyendo el claim `super_admin` del JWT (la API sigue siendo la fuente de verdad).

---

## Hecho recientemente (referencia)

- [x] **Suscripciones v1 (backend)**: `Plan` + `Subscription` por **tenant**, preferencia Mercado Pago, webhook, middleware de acceso; migraciones EF incl. `SubscriptionTenantId`; config `MercadoPago` + `SubscriptionAccess`
- [x] API multi-tenant + JWT + refresh
- [x] Super admin en modelo + endpoints `/api/admin/users`, `/api/admin/stats`, toggle activo
- [x] Cliente: pantalla `/admin`, navegación condicional, panel inicio super admin
- [x] Admin UI: buscador por email u organización, filtros (Todos/Activos/Inactivos), confirmación accesible (ConfirmDialog), componentes `StatsCards` / `UsersTable` / `UserRow`, capa de servicio `src/services/adminService.ts`; columna Organización en tabla y `TenantName` en `AdminUserListItemDto`
- [x] Arranque API con `MigrateAsync` (revisar política en prod)
- [x] Tokens con `tenantName`; barra superior muestra organización; ayuda bajo “Organización” en registro

---

## SaaS vendible — análisis y pasos pequeños

Objetivo: poder **cobrar**, **dar soporte** y **generar confianza** sin un big-bang. Priorizar lo que desbloquea ventas o reduce riesgo legal/operativo.

### A. Confianza y claridad (barato, alto impacto)

- [ ] Páginas legales enlazadas desde login/footer: **Términos**, **Privacidad**, contacto soporte
- [ ] Checkbox registro: “Acepto términos” + guardar `TermsAcceptedAt` (usuario o tenant)
- [ ] **Página pública** mínima: qué es la app, para quién, 3 bullets de valor (sin pricing aún si no hay cobro)
- [ ] **Health**: `GET /health` (vivo) y `GET /ready` (BD) para hosting/monitoreo

### B. Producto comercial (sin pasarela o con trial manual)

- [ ] **Planes** documentados (nombre, precio objetivo, límites) aunque se cobre manual al inicio
- [ ] **Trial**: flag tenant `trialEndsAt` + banner en app + bloqueo suave al vencer (antes de PSP)
- [ ] **Invitaciones**: admin tenant invita por email en lugar de solo registro abierto (reduce abuso)

### C. Monetización y acceso (cuando el flujo de negocio esté claro)

- [x] Modelo **`Subscription`** por **tenant** (`TenantId`), alineado a B2B habitual
- [x] **Webhook** PSP con validación remota del pago + idempotencia básica por `ExternalPaymentId` (sin tabla de eventos aún)
- [x] **Checkout** vía preferencia MP (pago puntual por plan); **pendiente**: recurrente / preapproval / suscripción nativa MP
- [ ] **Facturación mínima**: PDF o enlace a comprobante; datos fiscales tenant (opcional país)
- [ ] **Email** post-pago / recordatorio fin de periodo

### D. Seguridad y cumplimiento

- [ ] **Migraciones en prod**: CI/CD o job explícito; no depender solo de `MigrateAsync` en arranque
- [ ] **Secretos** solo por variables de entorno / vault; rotación documentada
- [ ] **2FA** opcional para admins tenant; **revisión** rate limits en `/api/Auth/*`
- [ ] **Export / borrado** datos tenant (GDPR-style) proceso documentado

### E. Operación y crecimiento

- [ ] **Logs** estructurados + `X-Request-Id` para soporte
- [ ] **Sentry** (o similar) solo producción, sin PII en extra
- [ ] **Auditoría** admin: quién cambió `IsActive` / `ExpirationDate` / plan
- [ ] **Límites por plan**: usuarios del tenant, presupuestos/mes (soft enforce + mensaje claro)

### F. Calidad y ventas internas

- [ ] **Demo** seed o video corto para prospectos
- [ ] **Tests e2e** críticos (login + refresh + una ruta tenant) antes de escalar tráfico

---

## Próximo / en curso (sugerido)

Priorizar según la sección **SaaS vendible** arriba; estos son los más acoplados al código actual:

- [x] **Cliente suscripciones**: UI planes, llamada a `POST /api/Subscriptions/create`, redirección a `checkoutUrl`, banner si 403 por `SubscriptionAccess`, pantalla “Mi plan” con `GET /api/Subscriptions/me`
- [ ] **Suscripciones — próximos avances** (ver sección dedicada abajo)
- [ ] **Admin**: editar `ExpirationDate` desde UI + endpoint `PATCH` acotado a super admin
- [x] **Admin**: filtros (activo, texto email) en UI — paginación backend pendiente
- [ ] **Prod**: desactivar migración automática en arranque o gatear por entorno (`Development` only)

---

## Suscripciones — próximos avances (post v1 backend)

Orden sugerido: producto visible → cobro recurrente → operación.

- [x] **Modelo de cobro B2B**: suscripción ligada a **tenant** (migración `SubscriptionTenantId` repuebla `TenantId` desde el usuario dueño de la fila previa)
- [ ] **Registro / onboarding**: opción “activar Free automático” al crear **tenant** para no depender de que el cliente llame a `create` manualmente
- [ ] **Alineación con `User.ExpirationDate`**: una sola fuente de verdad (o documentar cuándo usa cada una: soporte manual vs plan pagado)
- [ ] **Mercado Pago recurrente**: preapproval / plan de suscripción MP o debito automático; hoy es checkout por periodo vía preferencia
- [ ] **Webhook endurecido**: tabla `ProcessedWebhookEvents` (id evento + payment id) para idempotencia total; rate limit / IP allowlist si MP lo documenta; revisar secret/signature si aplica a tu integración
- [ ] **Back URLs + UX**: pantalla “Pago en proceso / falló” usando `BackUrls*` y deep link a la app
- [ ] **Admin / soporte**: super admin ve plan y `EndDate` por **tenant**; acciones de soporte (extender periodo, cancelar)
- [ ] **Límites por plan**: cantidad de usuarios por tenant, presupuestos/mes, etc., leídos de `Plan` o tabla `PlanFeature`
- [ ] **Tests**: integración webhook (mock HTTP MP) + test flujo Free + test middleware con `Enforce` on/off
- [ ] **Producción**: `MercadoPago:NotificationUrl` HTTPS estable; `SubscriptionAccess:Enforce` y comunicación a usuarios existentes antes del cutover

---

## Backlog / ideas

- [ ] Export CSV presupuestos / reportes

*(Invitaciones, límites por plan, auditoría, tests e2e, legales, health, etc. están desglosados en **SaaS vendible**.)*

---

## Cómo “cerrar” una tarea

1. Implementar y mergear.
2. Marcar `- [x]` aquí (y en PR si usás checklist).
3. Si cambia comportamiento visible, actualizar la tabla **Documentación** arriba en la misma PR.

Si el archivo crece mucho, valorar partir en `BACKLOG.md` (solo tareas) y `docs/APP.md` (solo documentación); mantener en raíz al menos un índice que apunte a ambos.
