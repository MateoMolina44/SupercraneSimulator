# SupercraneSimulator

TP de Simuladores (UAI, 2.º año): prototipo con Articulation Body.
Unity 6.3 LTS. Entrega: viernes 23/10. Debe funcionar en build WebGL (itch.io).

## Qué simula
Grúa pórtico portuaria (STS, ship-to-shore) sobre rieles en un muelle.
Tarea: descargar cajas y contenedores (Rigidbodies comunes) de un barco
estático con el spreader y apilarlos en una zona marcada del muelle.
Tres cámaras (externa, cabina POV y vertical de la grúa), con cambio
entre ellas por tecla C y botón de UI.

## Árbol de Articulation Bodies (7 cuerpos, NO agregar más)
1. Base: raíz, immovable (rieles del muelle)
2. Pórtico: Prismatic, a lo largo de los rieles (±15 m), con drive
3. Carro: Prismatic, a lo largo de la viga/pluma (2 a 20 m), con drive
4. Izaje: Prismatic vertical (0 a 12 m), con drive
5-6. Cable: 2 eslabones Spherical, pasivos (gravedad)
7. Spreader: Revolute, eje Y (±180°), con drive

La cabina es una malla hija del carro, sin Articulation Body.
Todo detalle visual (patas, pluma, barco) es hijo de un cuerpo existente
o escenografía estática, sin ArticulationBody propio.
El enganche de cargas es un FixedJoint creado en runtime (no suma cuerpos).

## Requisitos de la consigna
- Entre 6 y 10 Articulation Bodies, al menos 3 tipos de joint
- Al menos 4 joints con drive y 1 parte pasiva
- Escala (1,1,1) en toda la jerarquía, anclas en los pivotes
- Entrada por UI o Input System
- Pantalla con target y valor real por joint
- Controlador: entrada -> target del drive en FixedUpdate,
  lectura de jointPosition hacia la UI
- Masas parecidas entre cuerpos conectados (evita temblores)

## Reglas de trabajo
- Repo público, sin Library, Temp, Logs ni Build
- No editar la escena en Unity y en Claude Code al mismo tiempo
- La escena se genera con un script de editor (menú Supercrane), no
  editando el YAML a mano
- Hacer commits chicos y frecuentes, con push al terminar cada tarea
- Build WebGL temprano, hoy si es posible
- Prioridad: consigna + WebGL > tarea > UI > idioma/config avanzada

## Documentación
DESIGN.md (diseño), STRUCTURE.md (carpetas y scripts), CODESTYLE.md,
ROADMAP.md, WORKFLOW.md (git, build, itch.io), DECISIONS.md.
