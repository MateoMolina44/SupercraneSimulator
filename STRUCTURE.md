# Estructura del proyecto

## Carpetas

Todo lo propio del proyecto vive en `Assets/_Project/` para separarlo de lo
que generan Unity y los paquetes.

```
Assets/
├─ _Project/
│  ├─ Scenes/          Main.unity (escena única, generada por el builder)
│  ├─ Scripts/
│  │  ├─ SupercraneSimulator.asmdef
│  │  ├─ Crane/        Controlador de la grúa y wrappers de joints
│  │  ├─ Input/        Lectura de Input System y botones de UI
│  │  ├─ Cameras/      Cámara orbital y cambio de cámaras
│  │  ├─ Gameplay/     Cargas, enganche, zona de apilado
│  │  ├─ UI/           Telemetría, controles en pantalla, pausa, config
│  │  ├─ Settings/     Configuración persistente (PlayerPrefs)
│  │  └─ Localization/ Strings (tabla ES/EN) y LocalizedText
│  ├─ Editor/
│  │  ├─ SupercraneSimulator.Editor.asmdef   (solo Editor)
│  │  ├─ ConsignaValidator.cs  Menú "Supercrane → Validate Consigna"
│  │  └─ SceneBuilder/ Menú "Supercrane → Build Scene"
│  ├─ Models/Generated Mallas con el tamaño aplicado (las crea el builder)
│  ├─ Models/Baked     Modelos de terceros horneados a escala real (los crea el builder)
│  ├─ Materials/       Paleta de puerto (URP Lit, los crea el builder)
│  ├─ Textures/        Adoquinado, paredón y lecho (sacadas de LowpolyStreetPack)
│  └─ Physics/         PhysicsMaterials
├─ Megapoly.Art/                    Contenedores (pack de terceros)
├─ Free Stylized Cargo Props Pack/  Cajas y pallets (pack de terceros)
├─ low-poly-cargo-ship/             Barco portacontenedores (pack de terceros)
├─ IgniteCoders/Simple Water Shader/  Shader de agua (pack de terceros)
├─ Generated/Imported/              Casco modelado en Blender (respaldo si falta el barco)
├─ Settings/           Assets de URP (de la plantilla)
└─ InputSystem_Actions.inputactions   (de la plantilla, sin uso: las acciones se crean en CraneInput)
Docs/Screenshots/      Capturas para el README y la portada de itch.io
Tools/package_web.py   Empaqueta el build Web para itch.io
```

## Assemblies

| Assembly | Carpeta | Referencias |
|---|---|---|
| `SupercraneSimulator` | `_Project/Scripts` | Unity.InputSystem, UnityEngine.UI, Unity.TextMeshPro (si se usa) |
| `SupercraneSimulator.Editor` | `_Project/Editor` | `SupercraneSimulator`; plataforma: solo Editor |

## Escena principal

La genera el menú **Supercrane → Build Scene**. No se edita el YAML a mano.

```
Main
├─ --- Environment ---
│  ├─ Directional Light
│  ├─ Global Volume
│  ├─ Quay            (muelle: Wall con el paredón y Paving con el adoquinado)
│  ├─ Seabed          (lecho a −8 m, para la mezcla por profundidad del agua)
│  ├─ Water           (48 bloques del shader de agua, sin collider)
│  ├─ Ship            (estático, MeshCollider con la forma del barco; DeckCargo: 20 contenedores fijos)
│  ├─ DockProps       (bolardos, postes, CargoStaging, ContainerYard; decorado)
│  └─ StackZone       (volumen lógico + pintura en el piso)
├─ --- Crane ---
│  └─ Base → Gantry → Trolley → Hoist → CableUpper → CableLower → Spreader
│     (Cabin con CabinCamera y CraneCamera son hijas de Trolley)
├─ --- Systems ---
│  ├─ CraneInput
│  └─ CraneController
├─ --- Loads ---
│  ├─ Container_01 … _04
│  └─ Crate_01 … _04
├─ --- Cameras ---
│  ├─ ExternalCamera   (con OrbitCamera)
│  └─ CameraSwitcher
└─ --- UI ---
   ├─ HUD             (telemetría, contador, botones de cámara y pausa, controles)
   ├─ GameSettings
   ├─ PauseCanvas     (PauseMenu; Overlay con MainPanel, ControlsPanel y SettingsPanel)
   └─ EventSystem
```

Los objetos `--- X ---` son separadores vacíos, sin componentes.

Las mallas no se escalan con el transform: `BuilderAssets` genera una malla
con el tamaño final y la guarda en `Models/Generated/`. Así toda la jerarquía
queda con escala (1,1,1). Los modelos de terceros que no vienen a escala real
se hornean igual con `BuilderAssets.BakeModel` (rotación y escala aplicadas a
los vértices) y quedan en `Models/Baked/`.

## Scripts

✅ = ya existe.

| Script | Carpeta | Responsabilidad |
|---|---|---|
| ✅ `DrivenJoint` | Crane | Envuelve un ArticulationBody con drive: rango, rampa, target, valor real, unidad y offset |
| ✅ `CraneController` | Crane | Lee los ejes en `Update` y los aplica a cada `DrivenJoint` en `FixedUpdate` |
| ✅ `CabinJoystick` | Crane | Inclina y gira los joysticks de la cabina según la entrada |
| ✅ `RopeLine` | Crane | Dibuja los cables del carro al izaje con un LineRenderer |
| ✅ `CraneAxis` | Input | Enum de los 4 ejes |
| ✅ `CraneInput` | Input | Acciones del Input System creadas en código, combinadas con los botones de UI |
| ✅ `HoldButton` | Input | Botón de UI que informa si está presionado |
| ✅ `CameraSwitcher` | Cameras | Recorre externa → cabina → grúa con C y con botón; muestra el nombre |
| ✅ `OrbitCamera` | Cameras | Órbita con clic derecho y zoom con rueda |
| ✅ `Load` | Gameplay | Marca una carga (contenedor o caja) y su estado |
| ✅ `SpreaderGrabber` | Gameplay | Detecta carga válida, crea y destruye el FixedJoint |
| ✅ `StackZone` | Gameplay | Cuenta cargas sueltas y quietas dentro de la zona |
| ✅ `JointTelemetryView` | UI | Tabla de target y valor real por joint |
| ✅ `StackCounterView` | UI | Muestra `Apiladas: N / total` y el estado del spreader |
| ✅ `PauseMenu` | UI | Pausa con P o botón; paneles principal, controles y configuración |
| ✅ `SettingsView` | UI | Toggles, sliders e idioma del panel de configuración |
| ✅ `GameSettings` | Settings | Carga y guarda con PlayerPrefs y aplica: paneles, velocidades, sensibilidad, idioma |
| ✅ `Strings` | Localization | Tabla clave → ES/EN, idioma actual y formato de números |
| ✅ `LocalizedText` | Localization | Actualiza un texto al cambiar el idioma |
| ✅ `SceneBuilder` | Editor | Genera la escena completa con valores de DESIGN.md |
| ✅ `BuilderAssets` | Editor | Mallas con tamaño aplicado y materiales URP; borra las mallas viejas al regenerar |
| ✅ `ConsignaValidator` | Editor | Revisa en la escena los requisitos de la consigna |

Esta tabla se actualiza cuando aparece o desaparece un script.
