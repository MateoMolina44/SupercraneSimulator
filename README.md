# SupercraneSimulator

Simulador de grúa pórtico portuaria (STS) hecho en Unity 6.3 con
**Articulation Bodies**. Trabajo práctico de Simuladores (UAI, 2.º año).

El objetivo es descargar cajas y contenedores de un barco con el spreader y
apilarlos dentro de una zona marcada del muelle.

- **Jugar:** _(link de itch.io pendiente)_
- **Motor:** Unity 6000.3.14f1 (6.3 LTS), URP
- **Plataforma objetivo:** Web (WebGL) en itch.io

## Controles

| Acción | Tecla |
|---|---|
| Mover el pórtico sobre los rieles | A / D |
| Mover el carro (muelle ↔ barco) | W / S |
| Subir / bajar el spreader | R / F |
| Girar el spreader | Q / E |
| Enganchar / soltar | Espacio |
| Cambiar cámara | C |
| Pausa y configuración | P |
| Orbitar / zoom (cámara externa) | Clic derecho + mouse / rueda |

También hay botones en pantalla, que se pueden ocultar desde Configuración.

## Documentación

| Archivo | Contenido |
|---|---|
| [CLAUDE.md](CLAUDE.md) | Consigna, restricciones y reglas de trabajo (fuente de verdad) |
| [DESIGN.md](DESIGN.md) | Diseño del simulador: grúa, joints, cámaras, UI, tarea |
| [STRUCTURE.md](STRUCTURE.md) | Organización de carpetas, escena y scripts |
| [CODESTYLE.md](CODESTYLE.md) | Convenciones de código C# y de Unity |
| [ROADMAP.md](ROADMAP.md) | Plan de trabajo hasta la entrega |
| [WORKFLOW.md](WORKFLOW.md) | Git, commits, builds WebGL y publicación en itch.io |
| [DECISIONS.md](DECISIONS.md) | Registro de decisiones tomadas |

## Abrir el proyecto

1. Clonar el repo (requiere [Git LFS](https://git-lfs.com/)).
2. Abrir la carpeta con Unity Hub usando el editor **6000.3.14f1**.
3. Abrir la escena principal (ver [STRUCTURE.md](STRUCTURE.md)).
