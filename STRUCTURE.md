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
│  │  └─ Localization/ Tabla ES/EN y textos traducibles
│  ├─ Editor/
│  │  ├─ SupercraneSimulator.Editor.asmdef   (solo Editor)
│  │  └─ SceneBuilder/ Menú "Supercrane → Build Scene"
│  ├─ Data/            ScriptableObjects (tabla de idioma)
│  ├─ Prefabs/         Crane, Container20, Crate, StackZone, Ship
│  ├─ Models/Generated Mallas con el tamaño aplicado (las crea el builder)
│  ├─ Materials/       Paleta de puerto (URP Lit, los crea el builder)
│  └─ Physics/         PhysicsMaterials
├─ Settings/           Assets de URP (de la plantilla)
└─ InputSystem_Actions.inputactions
```

`TutorialInfo/` y `Scenes/SampleScene.unity` son de la plantilla y se borran
cuando exista la escena propia.

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
│  ├─ Quay            (muelle)
│  ├─ Water
│  ├─ Ship            (estático, con colliders)
│  └─ StackZone
├─ --- Crane ---
│  └─ Base → Gantry → Trolley → Hoist → CableUpper → CableLower → Spreader
│     (CabinCamera es hija de Trolley)
├─ --- Systems ---
│  ├─ CraneInput
│  └─ CraneController
├─ --- Loads ---
│  ├─ Container_01 … _04
│  └─ Crate_01 … _04
├─ --- Cameras ---
│  └─ OrbitCamera
└─ --- UI ---
   ├─ HUD Canvas      (telemetría, contador, botón cámara, controles)
   ├─ Pause Canvas    (pausa y configuración)
   └─ EventSystem
```

Los objetos `--- X ---` son separadores vacíos, sin componentes.

Las mallas no se escalan con el transform: `BuilderAssets` genera una malla
con el tamaño final y la guarda en `Models/Generated/`. Así toda la jerarquía
queda con escala (1,1,1).

## Scripts

✅ = ya existe.

| Script | Carpeta | Responsabilidad |
|---|---|---|
| ✅ `DrivenJoint` | Crane | Envuelve un ArticulationBody con drive: rango, rampa, target, valor real, unidad y offset |
| ✅ `CraneController` | Crane | Lee los ejes en `Update` y los aplica a cada `DrivenJoint` en `FixedUpdate` |
| ✅ `RopeLine` | Crane | Dibuja los cables del carro al izaje con un LineRenderer |
| ✅ `CraneAxis` | Input | Enum de los 4 ejes |
| ✅ `CraneInput` | Input | Acciones del Input System creadas en código, combinadas con los botones de UI |
| `HoldButton` | Input | Botón de UI que informa si está presionado |
| ✅ `CameraSwitcher` | Cameras | Alterna cabina/externa con C y con botón |
| `OrbitCamera` | Cameras | Órbita con clic derecho y zoom con rueda |
| `Load` | Gameplay | Marca una carga (contenedor o caja) y su estado |
| `SpreaderGrabber` | Gameplay | Detecta carga válida, crea y destruye el FixedJoint |
| `StackZone` | Gameplay | Cuenta cargas sueltas y quietas dentro de la zona |
| ✅ `JointTelemetryView` | UI | Tabla de target y valor real por joint |
| `StackCounterView` | UI | Muestra `Apiladas: N / total` |
| `PauseMenu` | UI | Pausa con P, paneles de configuración y controles |
| `SettingsView` | UI | Toggles y sliders de configuración |
| `GameSettings` | Settings | Valores de config, carga y guarda con PlayerPrefs, avisa cambios |
| `LocalizationTable` | Localization | ScriptableObject clave → ES/EN |
| `LocalizedText` | Localization | Actualiza un texto al cambiar el idioma |
| ✅ `SceneBuilder` | Editor | Genera la escena completa con valores de DESIGN.md |
| ✅ `BuilderAssets` | Editor | Mallas con tamaño aplicado y materiales URP |

Esta tabla se actualiza cuando aparece o desaparece un script.
