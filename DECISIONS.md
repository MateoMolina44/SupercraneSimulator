# Decisiones

Registro corto de decisiones de diseño y técnicas. La más nueva va arriba.

| Fecha | Tema | Decisión | Motivo |
|---|---|---|---|
| 2026-11-18 | Seguridad | Se saca `com.unity.pipeline` (servidor HTTP que ejecuta comandos en el editor, sin uso) y los valores guardados descartan NaN/infinito; MCP for Unity queda hasta terminar el trabajo con Claude | Revisión de vulnerabilidades: el juego no tenía fallas explotables, el riesgo estaba en la máquina de desarrollo |
| 2026-10-23 | Pack de barcos | Se borra `25 Boats And Ships` | No se usaba (ver la decisión de abajo) |
| 2026-10-23 | Texturas | Del LowpolyStreetPack se copian a `_Project/Textures` solo el adoquinado (piso del muelle), el hormigón oscuro (paredón) y el barro (lecho); el piso es una losa fina aparte | El resto del pack (calles, carteles, árboles) no encaja en un puerto y el atlas pesa 62 MB; la losa evita que la textura se estire en las paredes |
| 2026-10-23 | Agua | Simple Water Shader en bloques de 50 m, con lecho a −8 m, paredón del muelle hasta el lecho y depth texture activada en el asset Mobile (WebGL); sin la cámara de reflejo | Pedido de Mateo; el shader necesita algo debajo para teñir por profundidad y la cámara de reflejo sería un segundo render por cuadro en WebGL |
| 2026-10-23 | Escenografía | 61 contenedores en la cancha, 20 en cubierta y 33 pallets/cajas, en tablas de pilas con semilla fija | Puerto con actividad sin tocar la escena a mano; se verificó que no chocan con el pórtico, el spreader ni la cámara inicial |
| 2026-10-23 | Cajas jugables | En x = −11 (z 13 a 19,6) | Dejan lugar a la bahía de contenedores de popa y quedan fuera de la huella del spreader sobre los contenedores |
| 2026-10-23 | Barco | Portacontenedores `low-poly-cargo-ship` corrido 10 m hacia −X y con MeshCollider; cubierta en y = 1,3 | El castillo (hasta 16,7 m) queda fuera del paso del pórtico y las cargas apoyan en la cubierta real |
| 2026-10-23 | Modelos de terceros | Se hornean (rotación y escala en los vértices) con `BuilderAssets.BakeModel`, con material URP propio si el del pack es Built-in | Mantiene la escala (1,1,1) que pide la consigna y evita el magenta en URP |
| 2026-10-23 | Escenografía | Cancha de contenedores y pila de pallets en las posiciones que armó Mateo a mano, pasadas a datos en el builder | La escena se sigue generando por script; se verificó que no chocan con el pórtico |
| 2026-10-23 | Pack de barcos | `25 Boats And Ships` sin usar | Son buques genéricos con torretas, sin cubierta plana para apilar |
| 2026-10-22 | Idioma | Tabla en código (`Strings`) en vez de ScriptableObject | Más simple, sin asset aparte que se desincronice con la escena generada |
| 2026-10-22 | Números | Coma en español y punto en inglés, con formato propio | En WebGL la cultura del sistema es siempre la invariante |
| 2026-10-22 | Pausa | `timeScale` 0 + `CraneInput.Locked`; botón de pausa en el HUD además de la tecla P | Que no se mueva nada ni se enganche con el menú abierto; jugable sin teclado |
| 2026-10-22 | Cámara orbital | Sigue al pórtico con suavizado; la rueda usa solo el signo | La rueda devuelve valores muy distintos en Windows y en WebGL |
| 2026-10-22 | Zona de apilado | Volumen lógico de 14 × 5 m en (−5, 0, 4,7), sin trigger | Más simple que un trigger y no interfiere con la física |
| 2026-10-22 | Alineación | Contenedores módulo 180°, cajas módulo 90° | Una caja cuadrada se ve igual girada 90° |
| 2026-10-22 | Cabina | Del lado tierra del izaje, girada para mirar al mar; joysticks animados con la entrada | Corrección de Mateo: miraba al muelle; los controles tienen que reflejar lo que hace el usuario |
| 2026-10-22 | Balanceo | Masas más altas, rampas de 1 a 2,5 s, damping 15 en cables, frenado antes de los topes | Pedido de Mateo: la grúa se movía demasiado; ahora frena con 4-6° |
| 2026-10-22 | Cámaras | Tres cámaras: externa, cabina POV y vertical en el carro | Pedido de Mateo: POV real del operador y vista para alinear el spreader |
| 2026-10-22 | Cabina | Interior modelado (vidrios, consolas, asiento) con la cámara en la cabeza | Referencia de una cabina STS real |
| 2026-10-22 | Pórtico | Vigas transversales arriba del carro | La cabina colgada chocaba con las vigas a la altura de los rieles del carro |
| 2026-10-22 | Inercia del cable | Eslabones con inercia (150, 800, 150) | Con la real el spreader gira flojo y oscila (relación 600:1) |
| 2026-10-22 | Arranque de joints | Ningún joint arranca en su límite; offset en DrivenJoint | En el límite PhysX da un impulso enorme en el primer paso |
| 2026-10-22 | Drives | Modo Acceleration, con rampa de 0,8 s en el target | Independiente de la carga; menos latigazo en el cable |
| 2026-10-22 | Input | Acciones creadas en código en CraneInput, sin tocar el .inputactions | Más simple y sin código generado |
| 2026-10-22 | Escala | Mallas generadas con el tamaño final, todo transform en (1,1,1) | Cumple la consigna al pie de la letra |
| 2026-10-22 | Herramientas | MCP for Unity (OpenUPM) para que Claude maneje el editor | Probar en Play y leer la consola sin pasos manuales |
| 2026-10-22 | Itch.io | Mateo crea la página; Claude prepara el build y los pasos | Claude no crea cuentas ni páginas |
| 2026-10-22 | Visual | Primitivas URP, paleta de puerto (grúa roja/blanca) | Rápido y liviano para WebGL |
| 2026-10-22 | Física | 50 Hz; subir solverIterations antes que el timestep | Lo más barato en WebGL |
| 2026-10-22 | Masas | Escaladas: 3000/2000/1500/800/800/2000 kg (antes 2000/1000/500/200/200/500); contenedor 800, caja 150 | Masas parecidas entre vecinos; el conjunto que sostiene la carga pesa más |
| 2026-10-22 | Tests | Sin tests automáticos; checklist manual | Entrega al día siguiente |
| 2026-10-22 | Escena | Generada por script de editor (`Supercrane → Build Scene`) | Reproducible; evita editar YAML y conflictos con el editor |
| 2026-10-22 | Código | Un asmdef de runtime y uno de Editor | Prolijo sin complicar |
| 2026-10-22 | Recortes | Primero idioma y config avanzada | Lo obligatorio va primero |
| 2026-10-22 | Conteo | Cuenta si está en la zona, suelta y quieta 1 s | Evita contar cargas en tránsito |
| 2026-10-22 | Cargas | 4 contenedores + 4 cajas | Demuestra la tarea sin cargar WebGL |
| 2026-10-22 | Recorridos | Pórtico ±15 m, carro 2-20 m, izaje 0-12 m | Alcanza 2-3 bahías del barco |
| 2026-10-22 | Enganche | FixedJoint en runtime con Espacio; cerca + alineado ±15° | No suma Articulation Bodies; da sentido al giro del spreader |
| 2026-10-22 | Idioma | ES/EN con tabla propia (ScriptableObject; después pasó a código, ver arriba) | Sin paquete Localization, liviano |
| 2026-10-22 | Config | Persistente con PlayerPrefs | Se guarda en el navegador en WebGL |
| 2026-10-22 | Pausa | Tecla P (no Esc) con Reanudar/Config/Controles/Reiniciar | Esc sale de pantalla completa en WebGL |
| 2026-10-22 | UI | uGUI; telemetría en tabla; contador de apiladas | Simple y conocido |
| 2026-10-22 | Controles UI | Botones mantener-presionado, ocultables desde config | Cumple "entrada por UI" |
| 2026-10-22 | Teclado | A/D pórtico, W/S carro, R/F izaje, Q/E spreader, Espacio, C, P | Esquema WASD conocido |
| 2026-10-22 | Cámaras | Cabina colgada del carro + externa orbital | Fiel a una STS; buena vista del apilado |
| 2026-10-22 | Escena | Muelle + barco estático | Tarea clara: descargar el barco |
| 2026-10-22 | Accesorio | Spreader (Revolute Y ±180°) en vez de gancho | Fiel a una STS |
| 2026-10-22 | Control | Target incremental en FixedUpdate | Estable y encaja con target vs real |
| 2026-10-22 | Tipo de grúa | **Grúa pórtico STS**, no grúa torre | Aclaración de Mateo: el proyecto es un pórtico |
| 2026-10-22 | Árbol de joints | Base → Pórtico (P) → Carro (P) → Izaje (P) → Cable ×2 (S) → Spreader (R) | 7 cuerpos, 3 tipos, 4 drives, 2 pasivos |
| 2026-10-22 | Plataforma | WebGL en itch.io, compresión Gzip | Brotli suele fallar en itch.io |
| 2026-10-22 | Repo | Rama única `main`, commits por tarea | Proyecto individual con entrega corta |
