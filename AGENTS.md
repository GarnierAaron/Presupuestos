# Instrucciones para agentes (Cursor / IA)

1. **Hoja de ruta y verdad funcional:** leer primero [`BACKLOG.md`](BACKLOG.md) en la raíz del repo. Ahí están prioridades, checkboxes y el resumen de módulos API/UI.
2. Al terminar una tarea: actualizar `BACKLOG.md` (marcar hecho y, si aplica, la sección *Documentación*).
3. No asumir rutas o comportamiento que no figure ahí o en el código; si falta algo en el backlog, proponer una línea nueva antes de implementar cambios grandes.

Stack principal: **.NET 8 API** (`src/Presupuestos.Api`), **EF Core** (`Presupuestos.Infrastructure`), **React + Vite** (`client/`).
