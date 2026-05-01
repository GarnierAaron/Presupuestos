# Capacitor — Android (APK)

La carpeta `android/` es el proyecto nativo. El flujo es: **build web → copiar a Android → abrir Android Studio / Gradle → APK**.

## Requisitos en tu PC

1. **Node.js** (versión acorde al proyecto).
2. **JDK 17** (Android Studio suele traer el JDK embebido).
3. **Android Studio** (SDK Platform Tools, plataforma Android, build-tools).
4. Variable de entorno **`ANDROID_HOME`** apuntando al SDK (Android Studio → SDK Manager muestra la ruta).

## Configuración ya aplicada en este repo

| Elemento | Valor |
|----------|--------|
| Paquetes | `@capacitor/core`, `@capacitor/cli`, `@capacitor/android` |
| `capacitor.config.ts` | `appId`: `com.presupuestos.app`, `webDir`: `dist` |
| Vite | `base: './'` (recomendado para assets dentro del APK) |
| Scripts npm | `cap:sync`, `android:open`, `android:build` |

Puedes cambiar `appId` y `appName` en `capacitor.config.ts` antes de publicar en Play Store (debe ser único).

## API desde la app (importante)

En el navegador, la API puede ir por **proxy** (`VITE_API_BASE` vacío). En el APK **no hay proxy**: Axios necesita la URL real del backend.

1. Crea `client/.env.production` (no lo subas con secretos si no aplica):

```env
VITE_API_BASE=https://tu-servidor.com
VITE_APP_VERSION=1.0.0
```

2. Usa **HTTPS** en producción. Si probás HTTP local (emulador `http://10.0.2.2:5279`), Android puede bloquear tráfico no cifrado; en ese caso hay que permitir cleartext solo en debug (consulta documentación de Android `networkSecurityConfig`).

**Emulador:** `http://10.0.2.2:PUERTO` apunta al `localhost` de tu PC.

**Dispositivo físico:** la IP LAN de tu PC, ej. `http://192.168.1.10:5279`, y firewall abierto.

## Pasos exactos — después de cambiar código web

```bash
cd client
npm install
```

### 1) Build de Vite + sincronizar con Android

```bash
npm run android:build
```

Equivale a `npm run build` + `npx cap sync android` (copia `dist/` al proyecto nativo).

### 2) Abrir en Android Studio

```bash
npm run android:open
```

O abre manualmente la carpeta `client/android` en Android Studio.

### 3) Generar APK

**Desde Android Studio**

1. Menú **Build → Build Bundle(s) / APK(s) → Build APK(s)** (debug) o **Generate Signed Bundle / APK** (release firmado para tienda).
2. El APK debug suele quedar en:
   `android/app/build/outputs/apk/debug/app-debug.apk`

**Desde terminal (debug)**

En Windows (Git Bash / CMD):

```bash
cd android
gradlew.bat assembleDebug
```

En macOS / Linux:

```bash
cd android
./gradlew assembleDebug
```

Salida típica: `android/app/build/outputs/apk/debug/app-debug.apk`.

### 4) Solo sincronizar (sin rebuild web)

Si ya corriste `npm run build` y solo cambió la config nativa:

```bash
npm run cap:sync
```

## Live reload en dispositivo (opcional)

1. En `capacitor.config.ts`, dentro de `server`, descomenta y ajusta:

   ```ts
   url: 'http://TU_IP_LAN:5999',
   cleartext: true,
   ```

2. Arranca Vite: `npm run dev` (accesible en esa IP/puerto).

3. `npx cap sync` y vuelve a ejecutar la app desde Android Studio.

Después desactiva `server.url` para builds de producción.

## Compatibilidad con lo existente

- **Web + PWA:** sin cambios obligatorios; `npm run dev` / `npm run build` siguen igual.
- **Proxy de Vite:** solo aplica en desarrollo web; en APK usa `VITE_API_BASE`.
- **`.npmrc`:** `legacy-peer-deps` se mantiene para `vite-plugin-pwa` + Vite 8.

## Publicación (release)

Para Play Store necesitás **APK o AAB firmado** (keystore propio). Usa **Build → Generate Signed Bundle / APK** en Android Studio y sigue el asistente; no commitees el keystore ni contraseñas.
