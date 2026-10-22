# Estructura del proyecto

## Carpetas

Todo lo propio del proyecto vive en `Assets/_Project/` para separarlo de lo
que generan Unity y los paquetes.

```
Assets/
├─ _Project/
│  ├─ Scenes/          Main.unity (escena única de juego)
│  ├─ Scripts/
│  │  ├─ Crane/        Controlador de la grúa y wrappers de joints
│  │  ├─ Input/        Lectura de Input System / botones de UI
│  │  ├─ Cameras/      Cambio y control de cámaras
│  │  ├─ Gameplay/     Cargas, enganche, zona de apilado
│  │  └─ UI/           Telemetría y controles en pantalla
│  ├─ Prefabs/         Crane, Crate, Container, StackZone
│  ├─ Materials/
│  ├─ Models/          Mallas importadas (si hay)
│  └─ Physics/         PhysicsMaterials
├─ Settings/           Assets de URP (de la plantilla)
└─ InputSystem_Actions.inputactions
```

`TutorialInfo/` y `Scenes/SampleScene.unity` son de la plantilla y se borran
cuando exista la escena propia.

## Escena principal

```
Main
├─ --- Environment ---
│  ├─ Directional Light
│  ├─ Global Volume
│  ├─ Ground
│  └─ StackZone
├─ --- Crane ---
│  └─ Crane (prefab)  → árbol de Articulation Bodies, ver DESIGN.md
├─ --- Loads ---
│  └─ Crate_01 … / Container_01 …
├─ --- Cameras ---
│  └─ ExternalCamera   (la de cabina es hija de Jib)
└─ --- UI ---
   ├─ Canvas
   └─ EventSystem
```

Los objetos `--- X ---` son separadores vacíos, sin componentes.

## Scripts previstos

| Script | Carpeta | Responsabilidad |
|---|---|---|
| `CraneController` | Crane | Recibe intenciones, escribe targets en `FixedUpdate`, expone lecturas |
| `DrivenJoint` | Crane | Envuelve un ArticulationBody con drive: límites, velocidad, target, valor real |
| `CraneInput` | Input | Lee Input System y botones; entrega intenciones al controlador |
| `CameraSwitcher` | Cameras | Alterna cámaras con C y botón |
| `HookGrabber` | Gameplay | Engancha y suelta cargas |
| `StackZone` | Gameplay | Detecta y cuenta cargas apiladas |
| `JointTelemetryView` | UI | Muestra target y valor real por joint |

Esta tabla se actualiza cuando aparece o desaparece un script.
