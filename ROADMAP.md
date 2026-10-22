# Roadmap

Entrega: **viernes 23/10/2026**. Hoy es jueves 22/10, así que el plan está
pensado para un día y medio. Cada tarea termina con commit y push.

Prioridad: consigna + WebGL > tarea > UI > idioma y config avanzada.
Orden de recortes en [DESIGN.md](DESIGN.md#11-recortes-si-falta-tiempo).

## Hito 0 — Base (jueves)

- [x] Proyecto Unity 6.3 + URP + Input System
- [x] Documentación y decisiones de diseño
- [ ] Carpetas `Assets/_Project/` y assemblies
- [ ] Plataforma Web configurada (Gzip, sin fallback)
- [ ] Página de itch.io creada (Mateo)
- [ ] **Build WebGL vacía subida a itch.io**

## Hito 1 — Grúa física (jueves)

- [ ] `SceneBuilder`: muelle, agua, rieles y árbol de 7 Articulation Bodies
- [ ] Escala (1,1,1), anclas en pivotes, masas y límites de DESIGN.md
- [ ] Probar en Play: la grúa se queda quieta, sin temblores

## Hito 2 — Control (jueves)

- [ ] `DrivenJoint` + `CraneController` (targets en `FixedUpdate`)
- [ ] Acciones del Input System (WASD, QE, RF, Espacio, C, P)
- [ ] Telemetría: target y valor real por joint
- [ ] Build WebGL de control

## Hito 3 — Tarea (viernes temprano)

- [ ] Barco estático con 4 contenedores y 4 cajas
- [ ] `SpreaderGrabber` (cerca + alineado ±15°) e indicador de color
- [ ] `StackZone` y contador `Apiladas: N / 8`

## Hito 4 — Cámaras y UI (viernes)

- [ ] Cámara de cabina en el carro y cámara orbital
- [ ] Cambio con C y con botón
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
