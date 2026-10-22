# Flujo de trabajo

## Git

- Rama única: `main`. Proyecto individual con entrega corta, no hace falta
  más.
- Commits chicos y frecuentes: uno por tarea terminada, con push enseguida.
- Formato del mensaje: `tipo: descripción en presente`, en español.

| Tipo | Uso |
|---|---|
| `feat` | Funcionalidad nueva (joint, controlador, cámara) |
| `fix` | Corrección de un bug |
| `docs` | Documentación |
| `scene` | Cambios de escena o prefabs sin código |
| `build` | Configuración de build o de plataforma |
| `chore` | Mantenimiento (ajustes que reescribe Unity, limpieza) |

- **Git LFS** está activo vía `.gitattributes` para binarios (texturas,
  modelos, audio). Verificar con `git lfs ls-files` antes de subir archivos
  grandes.
- Nunca se suben `Library/`, `Temp/`, `Logs/`, `Build/` ni `UserSettings/`
  (ya están en `.gitignore`).

## Unity y Claude Code

- **No editar la escena en Unity y en Claude Code al mismo tiempo.** Antes de
  que Claude toque un `.unity` o `.prefab`, guardar y cerrar la escena en
  Unity (o cerrar el editor).
- Después de cambios hechos por Claude en escena o prefabs, volver a abrir
  Unity y revisar la consola antes de commitear.
- Los cambios que Unity hace solo al abrir (assets de URP, ProjectSettings) se
  commitean aparte con `chore:`.

## Escena

- La escena `Main` se genera con **Supercrane → Build Scene**. Cualquier
  cambio de la grúa o del entorno se hace en `SceneBuilder.cs` y se vuelve a
  generar; no se edita la escena a mano.
- Claude maneja el editor con **MCP for Unity** (paquete
  `com.coplaydev.unity-mcp` de OpenUPM). El aviso "Missing Signature" del
  Package Manager es esperable: el paquete no está firmado por Unity.

## Build WebGL

0. Requiere el módulo **Web Build Support** (Unity Hub → Installs →
   6000.3.14f1 → Add modules).
1. **File → Build Profiles → Web**, cambiar plataforma.
2. **Player Settings → Publishing Settings:**
   - Compression Format: **Gzip** (o Disabled). Brotli suele fallar en
     itch.io.
   - Decompression Fallback: **desactivado**.
3. Build en `Build/Web/` (ignorado por git). Tarda ~9 minutos.
4. Empaquetar con el script del repo:

   ```bash
   python Tools/package_web.py Build/Web Build/SupercraneSimulator-Web.zip
   ```

   Deja `index.html` en la raíz y las rutas con `/`. **No usar
   `Compress-Archive` de PowerShell 5.1**: guarda las rutas con `\` y en
   itch.io (Linux) las carpetas se rompen.

> Probarlo local no es confiable en esta máquina: algo intercepta el tráfico
> (el mismo problema que obliga a usar `schannel` con git) y corta la
> descarga del `.wasm`. La prueba real es en itch.io.

Si después de cambiar de plataforma el build falla con `Build target 'WebGL'
not supported`, es porque el módulo se instaló con Unity abierto: reiniciar
Unity.

## Publicación en itch.io

1. Crear el proyecto como **HTML** y subir el `.zip`.
2. Marcar "This file will be played in the browser".
3. Tamaño del viewport igual a la resolución del build (por ejemplo 1280×720)
   y activar el botón de pantalla completa.
4. Probar en una ventana privada antes de pasar el link.

## Checklist antes de cada push

- [ ] La escena abre sin errores en la consola
- [ ] Play funciona y la grúa no tiembla en reposo
- [ ] No hay archivos de `Library/` ni `Build/` en el commit
- [ ] Si cambió un script o una carpeta, actualizar [STRUCTURE.md](STRUCTURE.md)
