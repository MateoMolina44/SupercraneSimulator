# Roadmap

Entrega: **viernes 23/10/2026**. Hoy es jueves 22/10, así que el plan está
pensado para un día y medio. Cada hito termina con commit y push.

Prioridad: primero que **cumpla la consigna y ande en WebGL**, después lo lindo.

## Hito 0 — Base (jueves)

- [x] Proyecto Unity 6.3 + URP + Input System
- [x] Documentación inicial (este archivo y los demás)
- [ ] Carpetas `Assets/_Project/` y escena `Main`
- [ ] Plataforma Web configurada (compresión para itch.io)
- [ ] **Build WebGL vacía subida a itch.io** (detecta problemas temprano)

## Hito 1 — Grúa física (jueves)

- [ ] Árbol de 7 Articulation Bodies con primitivas (cubos y cilindros)
- [ ] Escala (1,1,1), anclas en pivotes, masas iniciales
- [ ] Límites y drives configurados
- [ ] Probar en Play: la grúa se queda quieta, sin temblores

## Hito 2 — Control (jueves)

- [ ] `DrivenJoint` + `CraneController` (targets en `FixedUpdate`)
- [ ] Acciones en el Input System
- [ ] Telemetría en pantalla: target y valor real por joint
- [ ] Build WebGL de control

## Hito 3 — Tarea (viernes temprano)

- [ ] Cajas y contenedores (Rigidbody)
- [ ] Enganchar y soltar
- [ ] Zona de apilado marcada y contador

## Hito 4 — Cámaras y UI (viernes)

- [ ] Cámara de cabina y cámara externa
- [ ] Cambio con C y con botón
- [ ] Controles en pantalla (si se decide)

## Hito 5 — Entrega (viernes)

- [ ] Ajuste de masas, drives y estabilidad
- [ ] Build WebGL final en itch.io
- [ ] README con link, controles y captura
- [ ] Repo público y limpio

## Si sobra tiempo

- Mallas más lindas para torre, pluma y cabina
- Sonido de motores
- Cronómetro y puntaje
