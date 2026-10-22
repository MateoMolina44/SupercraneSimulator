# SupercraneSimulator

TP de Simuladores (UAI, 2.º año): prototipo con Articulation Body.
Unity 6.3 LTS. Entrega: viernes 23/10. Debe funcionar en build WebGL (itch.io).

## Qué simula
Grúa torre. Tarea: tomar cajas/contenedores (Rigidbodies comunes) con el
gancho y apilarlos en una zona marcada. Dos cámaras (cabina y grúa),
con cambio entre ellas por tecla C y botón de UI.

## Árbol de Articulation Bodies (7 cuerpos, NO agregar más)
1. Torre: raíz, immovable
2. Pluma+cabina: Revolute, eje Y, giro, con drive
3. Carro: Prismatic, a lo largo de la pluma (2 a 20 m), con drive
4. Izaje: Prismatic vertical (0 a 12 m), con drive
5-6. Cable: 2 eslabones Spherical, pasivos (gravedad)
7. Gancho: Revolute, eje Y (±180°), con drive

La cabina es una malla hija de la pluma, sin Articulation Body.
Todo detalle visual es hijo de un cuerpo existente, sin ArticulationBody propio.

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
- Hacer commits chicos y frecuentes
- Build WebGL temprano, hoy si es posible
