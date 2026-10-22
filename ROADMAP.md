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
- [ ] Página de itch.io creada (Mateo)
- [ ] **Subir el build a itch.io y probarlo** (Mateo)

## Hito 1 — Grúa física (jueves)

- [x] `SceneBuilder`: muelle, agua, rieles y árbol de 7 Articulation Bodies
- [x] Escala (1,1,1), anclas en pivotes, masas y límites de DESIGN.md
- [x] Probar en Play: la grúa se queda quieta, sin temblores

## Hito 2 — Control (jueves)

- [x] `DrivenJoint` + `CraneController` (targets en `FixedUpdate`)
- [x] Acciones del Input System (WASD, QE, RF, Espacio, C, P)
- [x] Telemetría: target y valor real por joint
- [ ] Build WebGL de control

## Hito 3 — Tarea (viernes temprano)

- [ ] Barco estático con 4 contenedores y 4 cajas
- [ ] `SpreaderGrabber` (cerca + alineado ±15°) e indicador de color
- [ ] `StackZone` y contador `Apiladas: N / 8`

## Hito 4 — Cámaras y UI (viernes)

- [x] Cámara de cabina en el carro (POV con interior)
- [x] Cámara vertical de la grúa
- [ ] Cámara orbital
- [x] Cambio con C y con botón
- [ ] Controles en pantalla (botones mantener-presionado)
- [ ] Pausa con P: reanudar, controles, reiniciar

## Hito 5 — Configuración (viernes, recortable)

- [ ] `GameSettings` con PlayerPrefs
- [ ] Toggles: controles en pantalla, telemetría
- [ ] Sliders: velocidades y sensibilidad de cámara
- [ ] Idioma ES/EN

## Hito 6 — Entrega (viernes)

- [ ] Ajuste de masas, drives y estabilidad
- [ ] Checklist de prueba (WORKFLOW.md) en Play y en WebGL
- [ ] Build WebGL final en itch.io
- [ ] README con link, controles y captura
- [ ] Repo público y limpio
