# Estilo de código

Se siguen las convenciones habituales de C# en Unity. Ante la duda, imitar el
código que ya existe en el proyecto.

## Nombres

| Elemento | Formato | Ejemplo |
|---|---|---|
| Clases, structs, enums, métodos, propiedades | PascalCase | `CraneController`, `SetTarget()` |
| Campos privados | `_camelCase` | `_targetDegrees` |
| Parámetros y variables locales | camelCase | `deltaMeters` |
| Constantes | PascalCase | `MaxTrolleySpeed` |
| Interfaces | `I` + PascalCase | `ICraneInput` |

- Un tipo público por archivo; el archivo se llama igual que el tipo.
- Nombres en **inglés** en el código. Comentarios y docs en español.
- Incluir la unidad en el nombre cuando hay ambigüedad: `_speedMetersPerSecond`,
  `targetDegrees`, `positionRadians`.

## Namespaces

`SupercraneSimulator.<Carpeta>`, por ejemplo `SupercraneSimulator.Crane`.

## Campos del Inspector

- Privados con `[SerializeField]`, nunca campos públicos para el Inspector.
- Agrupar con `[Header("...")]` y documentar con `[Tooltip("...")]` cuando la
  unidad o el rango no son obvios.
- Usar `[Min]` o `[Range]` para validar valores.

```csharp
[Header("Límites")]
[SerializeField, Tooltip("Metros desde la torre")] private float _minMeters = 2f;
[SerializeField, Tooltip("Metros desde la torre")] private float _maxMeters = 20f;
```

## Física

- Escribir targets de drive **solo** en `FixedUpdate`.
- Leer entrada **solo** en `Update` (o en callbacks del Input System).
- Usar `Time.fixedDeltaTime` dentro de `FixedUpdate`.
- Convertir radianes a grados con `Mathf.Rad2Deg` antes de mostrar.
- Cachear referencias en `Awake`; nada de `GetComponent` o `Find` por frame.

## Formato

- 4 espacios, llaves en línea propia (estilo Allman, el default de Visual
  Studio y Rider).
- `var` cuando el tipo es obvio por la derecha.
- Sin código comentado en los commits.
- El archivo [.editorconfig](.editorconfig) aplica estas reglas en el IDE.

## Comentarios

- `///` en clases y miembros públicos que no se explican solos.
- Comentarios en línea para el **por qué**, no para el qué.
