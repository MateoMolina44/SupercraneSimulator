# Diseño

Este documento describe **qué** hace el simulador y **cómo** se arma la grúa.
Las restricciones duras están en [CLAUDE.md](CLAUDE.md). Si algo de acá las
contradice, manda CLAUDE.md.

Los valores marcados con `TBD` se definen con las preguntas de diseño y se
registran en [DECISIONS.md](DECISIONS.md).

## 1. Experiencia

- El jugador opera una grúa torre desde la cabina o desde una cámara externa.
- Hay cajas y contenedores sueltos (Rigidbody comunes) en el suelo.
- La tarea es engancharlos y apilarlos dentro de una **zona marcada**.
- La pantalla muestra, para cada joint con drive, el **target** pedido y el
  **valor real** leído de `jointPosition`.

## 2. Árbol de Articulation Bodies

7 cuerpos. **No se agregan más.**

```
Tower            (raíz, immovable)
└─ Jib           Revolute, eje Y, drive   — pluma + cabina (malla hija)
   └─ Trolley    Prismatic, eje de la pluma, 2 a 20 m, drive
      └─ Hoist   Prismatic, vertical, 0 a 12 m, drive
         └─ CableUpper   Spherical, pasivo
            └─ CableLower   Spherical, pasivo
               └─ Hook   Revolute, eje Y, ±180°, drive
```

| # | Cuerpo | Joint | Eje | Límites | Drive | Rol |
|---|---|---|---|---|---|---|
| 1 | Tower | Fixed (raíz) | – | – | – | Base inmóvil |
| 2 | Jib | Revolute | Y | `TBD` (libre o limitado) | Sí | Giro de la pluma |
| 3 | Trolley | Prismatic | X local de la pluma | 2 a 20 m | Sí | Carro |
| 4 | Hoist | Prismatic | Y (hacia abajo) | 0 a 12 m | Sí | Largo del cable |
| 5 | CableUpper | Spherical | – | `TBD` | No | Péndulo |
| 6 | CableLower | Spherical | – | `TBD` | No | Péndulo |
| 7 | Hook | Revolute | Y | ±180° | Sí | Giro del gancho |

Cumple la consigna: 7 cuerpos (6 a 10), 3 tipos de joint (Revolute,
Prismatic, Spherical), 4 drives, 2 cuerpos pasivos.

### Reglas de armado

- Escala `(1,1,1)` en **toda** la jerarquía. El tamaño se da con mallas hijas
  escaladas, nunca con el transform de un cuerpo.
- Cada ancla en el pivote real del joint.
- La cabina y todo detalle visual son hijos de un cuerpo existente, sin
  ArticulationBody propio.
- Masas parecidas entre cuerpos vecinos (relación ideal ≤ 1:10). Valores
  iniciales en la tabla de abajo, a ajustar probando.

| Cuerpo | Masa inicial |
|---|---|
| Tower | (immovable, no importa) |
| Jib | `TBD` |
| Trolley | `TBD` |
| Hoist | `TBD` |
| CableUpper / CableLower | `TBD` |
| Hook | `TBD` |

### Unidades (verificar en Unity 6.3)

- `ArticulationDrive.target`: grados en joints de rotación, metros en
  prismáticos.
- `jointPosition`: radianes en joints de rotación, metros en prismáticos.
  Hay que convertir a grados antes de mostrarlo en la UI.

## 3. Controlador

Patrón obligatorio de la consigna:

1. `Update`: se lee la entrada (Input System o UI) y se guarda la intención.
2. `FixedUpdate`: la intención se integra en un **target** (con velocidad
   máxima y límites) y se escribe en el drive.
3. Se lee `jointPosition` y se publica para la UI.

Modo de control: `TBD` (posición incremental vs. velocidad).

## 4. Enganche de cargas

Mecanismo: `TBD` (por ejemplo `FixedJoint` creado en runtime entre el gancho
y la caja, o un trigger con "imán").

## 5. Cámaras

- **Cabina:** hija de la pluma, gira con ella.
- **Grúa (externa):** `TBD` (fija, orbital o que sigue al gancho).
- Cambio con la tecla **C** y con un botón de UI.

## 6. UI

- Panel de telemetría: por cada joint con drive, nombre, target y valor real.
- Botón de cambio de cámara.
- Controles en pantalla: `TBD`.
- Indicador de tarea (cajas apiladas, tiempo): `TBD`.

## 7. Controles

| Acción | Teclado | UI |
|---|---|---|
| Girar pluma | `TBD` | `TBD` |
| Mover carro | `TBD` | `TBD` |
| Subir/bajar gancho | `TBD` | `TBD` |
| Girar gancho | `TBD` | `TBD` |
| Enganchar/soltar | `TBD` | `TBD` |
| Cambiar cámara | C | Botón |

## 8. Fuera de alcance

- Más Articulation Bodies que los 7 listados.
- Viento, contrapesos dinámicos, multijugador, sonido avanzado.
