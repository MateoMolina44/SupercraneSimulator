# Roadmap

Entrega: **viernes 23/10/2026**. Hoy es jueves 22/10, así que el plan está
pensado para un día y medio. Cada tarea termina con commit y push.

Prioridad: consigna + WebGL > tarea > UI > idioma y config avanzada.
Orden de recortes en [DESIGN.md](DESIGN.md#11-recortes-si-falta-tiempo).

## Hito 0 — Base (jueves)

- [x] Proyecto Unity 6.3 + URP + Input System
- [x] Documentación y decisiones de diseño
- [x] Carpetas `Assets/_Project/` y assemblies
- [x] Plataforma Web configurada (Gzip, sin fallback)
- [x] Instalar el módulo **Web Build Support** en Unity Hub (Mateo)
- [x] Primer build Web (`Build/SupercraneSimulator-Web.zip`)
- [x] Página de itch.io creada (Mateo)
- [x] **Subir el build a itch.io** (Mateo): <https://midir44.itch.io/supercrane-simulator>

## Hito 1 — Grúa física (jueves)

- [x] `SceneBuilder`: muelle, agua, rieles y árbol de 7 Articulation Bodies
- [x] Escala (1,1,1), anclas en pivotes, masas y límites de DESIGN.md
- [x] Probar en Play: la grúa se queda quieta, sin temblores

## Hito 2 — Control (jueves)

- [x] `DrivenJoint` + `CraneController` (targets en `FixedUpdate`)
- [x] Acciones del Input System (WASD, QE, RF, Espacio, C, P)
- [x] Telemetría: target y valor real por joint
- [x] Build WebGL de control

## Hito 3 — Tarea (viernes temprano)

- [x] Barco estático con 4 contenedores y 4 cajas
- [x] `SpreaderGrabber` (cerca + alineado ±15°) e indicador de color
- [x] `StackZone` y contador `Apiladas: N / 8`

## Hito 4 — Cámaras y UI (viernes)

- [x] Cámara de cabina en el carro (POV con interior)
- [x] Cámara vertical de la grúa
- [x] Cámara orbital
- [x] Cambio con C y con botón
- [x] Controles en pantalla (botones mantener-presionado)
- [x] Pausa con P: reanudar, controles, reiniciar

## Hito 5 — Configuración (viernes, recortable)

- [x] `GameSettings` con PlayerPrefs
- [x] Toggles: controles en pantalla, telemetría
- [x] Sliders: velocidades y sensibilidad de cámara
- [x] Idioma ES/EN

## Hito 6 — Entrega (viernes)

- [x] Ajuste de masas, drives y estabilidad
- [x] Validador de la consigna (Supercrane → Validate Consigna)
- [x] Checklist de prueba (WORKFLOW.md) en Play
- [ ] Checklist de prueba en WebGL (cuando esté en itch.io)
- [x] Build WebGL final (`Build/SupercraneSimulator-Web.zip`)
- [x] Subirlo a itch.io (Mateo)
- [x] README con controles, consigna y capturas
- [x] Link de itch.io en el README
- [x] Repo limpio: sin restos de la plantilla ni carpetas generadas
