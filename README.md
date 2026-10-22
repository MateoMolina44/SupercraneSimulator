# Supercrane Simulator

Simulador de una **grúa pórtico portuaria (STS, ship-to-shore)** hecho en
Unity 6.3 con **Articulation Bodies**. Trabajo práctico de Simuladores
(UAI, 2.º año).

La tarea es descargar un barco: tomar los 4 contenedores y las 4 cajas con
el spreader y apilarlos en la zona amarilla del muelle.

- **Jugar en el navegador:** _(link de itch.io pendiente)_
- **Motor:** Unity 6000.3.14f1 (6.3 LTS), URP, Input System
- **Plataforma:** Web (WebGL)

![Grúa pórtico con el HUD: telemetría, contador y controles en pantalla](Docs/Screenshots/hud.png)

| Cabina (POV del operador) | Cámara vertical de la grúa |
|---|---|
| ![Vista desde la cabina](Docs/Screenshots/cabin.png) | ![Vista vertical desde el carro](Docs/Screenshots/crane.png) |

## Cómo se juega

1. Llevá el carro hacia el barco (**W**) y ubicá el spreader sobre una carga.
2. Bajá el spreader (**F**) y alinealo con la carga (**Q / E**). Las luces
   del spreader se ponen **amarillas** cuando la carga está lista.
3. Enganchá con **Espacio** (luces **verdes**), levantá (**R**) y llevá la
   carga a la zona amarilla del muelle.
4. Bajala, soltala con **Espacio** y repetí hasta ver **¡Barco descargado!**

### Controles

| Acción | Teclado | En pantalla |
|---|---|---|
| Mover el pórtico sobre los rieles | A / D | ◄ A / D ► |
| Mover el carro (tierra ↔ mar) | S / W | S Tierra / W Mar |
| Subir / bajar el spreader | R / F | R Subir / F Bajar |
| Girar el spreader | Q / E | Q Izq. / E Der. |
| Enganchar / soltar | Espacio | Espacio |
| Cambiar de cámara (externa, cabina, grúa) | C | Botón de cámara |
| Pausa, controles, configuración, reiniciar | P | Botón de pausa |
| Orbitar / zoom (cámara externa) | Clic derecho + mouse / rueda | – |

En **Configuración** (dentro de la pausa) se pueden ocultar los controles en
pantalla o la telemetría, cambiar la velocidad de cada eje y la sensibilidad
de la cámara, y elegir **español o inglés**. Las opciones se guardan en el
navegador.

## Cómo cumple la consigna

| Requisito | Cómo se cumple |
|---|---|
| Entre 6 y 10 Articulation Bodies | 7: Base, Pórtico, Carro, Izaje, 2 eslabones de cable, Spreader |
| Al menos 3 tipos de joint | Prismatic (pórtico, carro, izaje), Spherical (cable), Revolute (spreader) |
| Al menos 4 joints con drive | Pórtico, carro, izaje y spreader |
| Al menos 1 parte pasiva | Los 2 eslabones del cable cuelgan por gravedad, sin target |
| Escala (1,1,1) y anclas en los pivotes | Las mallas se generan con su tamaño final: ningún transform está escalado |
| Entrada por UI o Input System | Las dos: teclado con Input System y botones en pantalla |
| Target y valor real por joint | Tabla de telemetría arriba a la izquierda (`jointPosition`) |
| Controlador en FixedUpdate | `CraneController` lee la entrada en `Update` y escribe los targets en `FixedUpdate` |
| Masas parecidas entre cuerpos conectados | 3000 / 2000 / 1500 / 800 / 800 / 2000 kg (peor relación 2,5:1) |

Todo esto se puede verificar en el editor con **Supercrane → Validate
Consigna**, que revisa la escena y escribe el resultado en la consola.

```
Base (immovable)
└─ Gantry        Prismatic X, ±15 m, drive
   └─ Trolley    Prismatic Z, 2 a 20 m, drive (cabina colgada)
      └─ Hoist   Prismatic −Y, 0 a 12 m, drive
         └─ CableUpper / CableLower   Spherical, pasivos
            └─ Spreader               Revolute Y, ±180°, drive
```

El enganche es un `FixedJoint` creado en runtime entre la carga y el
spreader, así que no suma Articulation Bodies.

## Abrir el proyecto

1. Clonar el repo (usa [Git LFS](https://git-lfs.com/) para las imágenes).
2. Abrirlo con Unity Hub usando el editor **6000.3.14f1**.
3. Abrir `Assets/_Project/Scenes/Main.unity` y darle Play.

La escena **no se edita a mano**: se genera con **Supercrane → Build
Scene** a partir de `SceneBuilder.cs`. Para cambiar la grúa o el entorno, se
cambia el script y se vuelve a generar.

## Build Web

Plataforma Web, compresión Gzip y sin *Decompression Fallback* (ver
[WORKFLOW.md](WORKFLOW.md)). Después del build:

```bash
python Tools/package_web.py Build/Web Build/SupercraneSimulator-Web.zip
```

## Documentación

| Archivo | Contenido |
|---|---|
| [CLAUDE.md](CLAUDE.md) | Consigna, restricciones y reglas de trabajo |
| [DESIGN.md](DESIGN.md) | Diseño: grúa, física, controlador, cámaras, UI, configuración |
| [STRUCTURE.md](STRUCTURE.md) | Carpetas, escena y scripts |
| [CODESTYLE.md](CODESTYLE.md) | Convenciones de código |
| [ROADMAP.md](ROADMAP.md) | Plan de trabajo por hitos |
| [WORKFLOW.md](WORKFLOW.md) | Git, build Web, itch.io y checklist de prueba |
| [DECISIONS.md](DECISIONS.md) | Registro de decisiones y por qué |
