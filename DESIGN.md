# Diseño

Este documento describe **qué** hace el simulador y **cómo** se arma la grúa.
Las restricciones duras están en [CLAUDE.md](CLAUDE.md). Si algo de acá las
contradice, manda CLAUDE.md. Las decisiones y su motivo están en
[DECISIONS.md](DECISIONS.md).

## 1. Experiencia

- El jugador opera una **grúa pórtico portuaria STS** sobre rieles en un
  muelle, desde la cabina (colgada del carro), desde una cámara vertical en
  el carro o desde una cámara externa.
- Junto al muelle hay un **barco estático** con 4 contenedores y 4 cajas en
  cubierta.
- La tarea es engancharlos con el **spreader** y apilarlos en la **zona
  marcada** del muelle.
- Un contador muestra `Apiladas: N / 8`.
- La pantalla muestra, para cada joint con drive, el **target** y el **valor
  real** leído de `jointPosition`.

## 2. Ejes y medidas

Ejes del mundo:

- **X:** a lo largo del muelle (dirección de los rieles).
- **Z:** perpendicular al muelle; Z+ va hacia el agua y el barco.
- **Y:** arriba.

| Elemento | Medida |
|---|---|
| Luz del pórtico (entre patas) | ~20 m |
| Altura de la viga | ~15 m (ajustar para que el spreader llegue al suelo) |
| Recorrido sobre rieles | ±15 m |
| Recorrido del carro | 2 a 20 m (≈2-10 m sobre muelle, ≈10-20 m sobre el barco) |
| Recorrido del izaje | 0 a 12 m |
| Eslabones de cable | ~1 m cada uno |
| Contenedor 20' | 6,1 × 2,6 × 2,4 m |
| Caja | ~1,5 m de lado |

## 3. Árbol de Articulation Bodies

7 cuerpos. **No se agregan más.**

```
Base             (raíz, immovable — rieles)
└─ Gantry        Prismatic, eje X, ±15 m, drive      — patas + viga + pluma
   └─ Trolley    Prismatic, eje Z, 2 a 20 m, drive    — cabina (malla hija)
      └─ Hoist   Prismatic, eje −Y, 0 a 12 m, drive
         └─ CableUpper   Spherical, pasivo
            └─ CableLower   Spherical, pasivo
               └─ Spreader  Revolute, eje Y, ±180°, drive
```

| # | Cuerpo | Joint | Eje | Límites | Drive | Masa |
|---|---|---|---|---|---|---|
| 1 | Base | raíz | – | – | – | immovable |
| 2 | Gantry | Prismatic | X | −15 a 15 m | Sí | 3000 kg |
| 3 | Trolley | Prismatic | Z | 2 a 20 m | Sí | 2000 kg |
| 4 | Hoist | Prismatic | −Y | 0 a 12 m | Sí | 1500 kg |
| 5 | CableUpper | Spherical | – | swing ±45° | No | 800 kg |
| 6 | CableLower | Spherical | – | swing ±45° | No | 800 kg |
| 7 | Spreader | Revolute | Y | ±180° | Sí | 2000 kg |

Cumple la consigna: 7 cuerpos (6 a 10), 3 tipos de joint (Prismatic,
Spherical, Revolute), 4 drives, 2 cuerpos pasivos.

Cargas (Rigidbody comunes): contenedor 800 kg, caja 150 kg.

### Reglas de armado

- Escala `(1,1,1)` en **toda** la jerarquía. El tamaño se da con mallas hijas
  escaladas, nunca con el transform de un cuerpo.
- Cada ancla en el pivote real del joint. Los Prismatic de Unity se mueven
  sobre el eje X del ancla: rotar el ancla para alinearlo con X, Z o −Y.
- Patas, viga, pluma y cabina son mallas hijas, sin ArticulationBody propio.
- El barco, el muelle y el agua son escenografía estática con colliders.

### Física

- Timestep 0,02 s (50 Hz). `solverIterations` 16 y `solverVelocityIterations`
  4 en todos los cuerpos.
- Drives en modo **Acceleration**: no dependen de la masa que cuelga, así que
  sirven igual con o sin carga.

| Joint | Stiffness | Damping | Velocidad | Rampa | Posición inicial |
|---|---|---|---|---|---|
| Pórtico | 200 | 30 | 1,5 m/s | 2,5 s | 0 m |
| Carro | 200 | 30 | 2,5 m/s | 2,5 s | 6 m |
| Izaje | 400 | 40 | 1,5 m/s | 1 s | 3 m |
| Spreader | 400 | 40 | 30 °/s | 0,6 s | 0° |
| Cables (pasivos) | 0 | 15 | – | – | – |

- La amortiguación de los cables hace de **anti-balanceo**, como los cables
  en trapecio de una STS real. Medido: al frenar a fondo el balanceo queda
  en 3,9° (pórtico) y 6° (carro) y a los 3 s casi desaparece.
- Cerca de un tope, `DrivenJoint` limita la velocidad a la que permite frenar
  justo ahí, así no se para en seco.

- **Ningún joint arranca en un límite.** Si un Prismatic se arma justo en su
  tope, PhysX lo empuja hacia adentro en el primer paso y el cable sale
  disparado. Cada cuerpo se arma en su posición inicial (jointPosition = 0) y
  `DrivenJoint` suma un offset para mostrar la medida real.
- El límite físico del spreader es ±185° para que el target (±180°) nunca
  quede sobre el tope.
- La inercia de los eslabones de cable está inflada a (600, 3200, 600) kg·m².
  Con la real (Iyy ≈ 10 contra ≈ 7000 del spreader) el solver no transmite el
  torque y el spreader gira flojo y oscila.
- El target llega a la velocidad máxima con la rampa de cada eje (tabla de
  arriba), para no darle latigazos al cable.

### Unidades (verificar en Unity 6.3)

- `ArticulationDrive.target`: grados en Revolute, metros en Prismatic.
- `jointPosition`: radianes en Revolute, metros en Prismatic. Hay que
  convertir con `Mathf.Rad2Deg` antes de mostrar.

## 4. Controlador

Modo **target incremental**:

1. `Update`: se lee la entrada (teclado o botones de UI) y se guarda un eje
   −1/0/+1 por joint.
2. `FixedUpdate`: la velocidad del target se acerca a `eje × velocidad` con
   una rampa, `target += velocidad × fixedDeltaTime`, se limita al rango del
   joint y se escribe en el drive.
3. Se lee `jointPosition` y se publica para la UI.

Al soltar la tecla, el target queda fijo y el joint se detiene ahí.
Las velocidades se pueden cambiar desde Configuración.

## 5. Enganche (spreader)

- Tecla **Espacio** (o botón de UI) alterna enganchar/soltar.
- Solo engancha si hay una carga **debajo y cerca** del spreader y su
  orientación está dentro de **±15°** de la del spreader (módulo 180°).
- Al enganchar se crea un `FixedJoint` entre el spreader y la carga; al
  soltar se destruye.
- Indicador visual en el spreader: rojo (nada), amarillo (carga válida
  debajo), verde (enganchado).

## 6. Zona de apilado

- Volumen marcado en el muelle (trigger + decal o quad de color).
- Una carga **cuenta** si su centro está dentro del volumen, no está
  enganchada y su velocidad es casi 0 durante 1 s.
- Contador `Apiladas: N / 8` en pantalla.

## 7. Cámaras

Tres cámaras, en este orden al apretar **C** o el botón de UI (el botón
muestra el nombre de la activa):

1. **Externa:** vista general del muelle. Pendiente: que sea orbital (clic
   derecho y arrastrar para orbitar, rueda para zoom).
2. **Cabina (POV):** el operador sentado en la cabina, a la altura de la
   cabeza, mirando hacia abajo por el piso de vidrio. Se ven las consolas
   con joysticks a los costados, el marco de la cabina y el spreader abajo.
3. **Grúa:** cámara colgada del carro mirando derecho hacia abajo, para
   alinear el spreader con la carga.

### Cabina

- Cuelga del carro del lado del mar (z +0,9 a +3,5 m respecto del carro),
  para no chocar con el bloque del izaje.
- Mide 2,4 × 2,4 × 2,6 m. Marco verde agua, vidrios transparentes en el piso
  y en las cuatro caras, tablero y armario detrás del operador.
- El asiento queda detrás de la cámara para no tapar el piso.
- Las vigas transversales del pórtico van por encima del carro (y ≈ 18,5 m)
  para que la cabina pase entre las patas.

## 8. UI (uGUI)

| Elemento | Ubicación | Notas |
|---|---|---|
| Telemetría | Esquina superior izquierda | Tabla: joint, target, real, unidad. Ocultable |
| Contador | Arriba al centro | `Apiladas: N / 8` |
| Botón de cámara | Esquina superior derecha | |
| Controles en pantalla | Abajo | Pares ◀ ▶ que actúan mientras se mantienen + botón enganchar. Ocultables |
| Pausa | Centro, con tecla **P** | Reanudar, Configuración, Controles, Reiniciar |

### Configuración (se guarda con PlayerPrefs)

- Mostrar controles en pantalla (sí/no)
- Mostrar telemetría (sí/no)
- Velocidad de pórtico, carro, izaje y spreader (sliders)
- Sensibilidad de cámara (órbita y zoom)
- Idioma: Español / English

### Idioma

Tabla propia (ScriptableObject con clave → texto ES/EN) y un componente
`LocalizedText` en cada texto. Sin el paquete Localization.

## 9. Controles

| Acción | Teclado | UI |
|---|---|---|
| Pórtico sobre rieles | A / D | ◀ ▶ |
| Carro (muelle ↔ barco) | W / S | ◀ ▶ |
| Subir / bajar spreader | R / F | ▲ ▼ |
| Girar spreader | Q / E | ⟲ ⟳ |
| Enganchar / soltar | Espacio | Botón |
| Cambiar cámara | C | Botón |
| Pausa | P | – |
| Orbitar / zoom (cámara externa) | Clic derecho + mouse / rueda | – |

**Esc no se usa:** en WebGL saca al navegador de pantalla completa.

## 10. Estilo visual

Primitivas de Unity con materiales URP, paleta de puerto clásico:

- Grúa: rojo y blanco.
- Contenedores: colores variados (azul, verde, naranja, gris).
- Cajas: madera clara.
- Muelle: gris hormigón. Agua: plano azul sin animar. Barco: casco oscuro.

## 11. Recortes si falta tiempo

En este orden: idioma inglés → sliders de velocidad y sensibilidad →
controles en pantalla → detalles del barco. Lo obligatorio de la consigna y
el build WebGL nunca se recortan.

## 12. Fuera de alcance

- Más Articulation Bodies que los 7 listados.
- Barco que se mece, viento, pluma abatible, multijugador, sonido avanzado.
