# Unity: escena Shark Tank

Implementación básica de la escena del reto (sección 8 del anteproyecto). Unity solo muestra lo que pasa en la sesión: quién habla, el último mensaje, las calificaciones, el turno y la decisión. Nunca le manda datos a los agentes.

## Abrir el proyecto

1. En Unity Hub: **Add > Add project from disk** y elige esta carpeta `Unity`.
2. El proyecto usa **Unity 6.6 (6000.6.7f1)**. Si Hub pide otra versión, ábrelo con la versión de Unity 6 que tengas instalada.
   - Si usan Unity 2022.3, cambien `"com.unity.ugui": "2.0.0"` por `"1.0.0"` en `Packages/manifest.json`.
3. Usa el render pipeline integrado (Built-in), así que no necesita URP.

## Verlo funcionando

- **Rápido:** dale Play en cualquier escena. Si la escena no tiene la conversación, `ArranqueAutomatico` arma todo y reproduce una sesión de ejemplo.
- **Con escena y prefabs:** menú **Codea > Crear escena Shark Tank**. Crea `Assets/Scenes/SharkTank.unity` y los prefabs `PersonajeJuezFinanzas`, `PersonajeJuezInnovacion`, `PersonajeJuezMercado`, `PersonajeEmprendedor`, `GloboDialogo` y `BarraCriterio` en `Assets/Prefabs`. Después de crearlos, hagan commit de la escena, los prefabs y todos los archivos `.meta`.

Por defecto corre en **modo simulado**: reproduce `Assets/Resources/sesion_ejemplo.json`, que es la sesión de ejemplo del documento de diseño (100,000 por 10 %, promedio de 6, trato final de 100,000 por 14 %).

## Escenario SharkMind

La escena usa la identidad del equipo (SharkMind). Los colores salen de `Assets/Scripts/Identidad/SharkMindColors.cs`, que es la misma paleta de `identidad/sharkmind/paleta.css`.

- **Jueces:** son tres, sentados en fila en sillas ejecutivas de piel, viendo hacia el emprendedor. Cada uno tiene un área y se viste distinto:
  - **Finanzas:** traje azul marino, corbata dorada y pañuelo; barba y pelo con canas.
  - **Innovación:** traje gris carbón, camisa azul claro, corbata Aleta y gafas.
  - **Mercado:** traje verde bosque y moño rojo (Mordida).
  Los estilos están en `ConstructorEscena.Jueces` por si quieren cambiar colores o accesorios.
- **Emprendedor:** de pie sobre una tarima ovalada con filo dorado, como escenario, frente al juez. La tarima es más larga que ancha y se alarga hacia el juez. Su color es Aleta.
- **Fondo de los jueces:** pared ancha de tablones de madera a la espalda de los tres jueces (ve hacia el emprendedor), con el icono redondo de SharkMind en grande detrás del juez de en medio, sin nombre ni marco. Dos focos colgados de la parte de arriba de la pared lo iluminan. Se ve de frente en los close ups de los jueces y de lado en la toma general.
- **Interfaz:** título en blanco "SharkMind: Simulación", panel en Océano y los colores de decisión de la paleta (oro = invertir, aleta = contraoferta, mordida = no invertir).

Las texturas están en `Assets/Resources/SharkMind/`: `icono-sharkmind.png` (el icono redondo con fondo transparente, del primer boceto del tiburón de negocios: `identidad/conceptos/6-tiburon-corbata.svg`; lo de afuera del círculo se recorta) y `madera.png` (la veta, que se tiñe con el color de cada tablón). Si se cambia el icono, basta con reemplazar ese PNG con el mismo nombre.

Para que el cono de los focos se vea en la pared, sus luces van en modo *Force Pixel*. Si la pared se ve muy oscura o muy brillante, se puede ajustar `intensity` de `Luz` en `FocoIzquierdo` y `FocoDerecho`.

## Comentarios del juez

La evidencia que usa el juez ya no va debajo de cada barra. En la esquina de arriba del panel de calificaciones hay un botón con una burbuja de chat que funciona como toggle: al presionarlo, el mismo panel cambia a la vista de comentarios, con fondo blanquecino, y muestra lo que ha ido tomando el juez, del más reciente arriba al más viejo abajo. Cada comentario dice el criterio, la calificación y la cita del pitch, con una franja del color de la barra. En esa vista el botón se vuelve una flecha para regresar a las estadísticas. Cuando llegan comentarios nuevos estando en estadísticas, el botón muestra cuántos hay sin leer. El clic en el botón no avanza la conversación.

## Avanzar la conversación

La conversación no avanza sola: cada mensaje se queda en el globo hasta que das **clic** en cualquier parte de la pantalla, o presionas **Espacio**, **Enter** o **flecha derecha**. Cuando el mensaje ya terminó de escribirse, aparece una flechita en la esquina del globo: entra deslizándose de izquierda a derecha con fade in y se queda quieta. Si das clic mientras el mensaje todavía se está escribiendo, se termina de escribir de golpe, y el siguiente clic avanza.

- En modo simulado, el siguiente paso de la sesión de ejemplo (con sus calificaciones) no llega hasta que avanzas.
- En modo servidor, los mensajes que van llegando se forman y se muestran uno por uno conforme avanzas.
- Para que avance sola como antes, desactiva **Avance Manual** en `GestorConversacion` (objeto `Logica`).
- La opacidad, el recorrido y la duración de la entrada de la flecha se cambian en `GloboDialogo`.

## Quién habla

Por ahora habla un solo juez en cada turno (iteración de la propuesta), rotando: turno 1 Finanzas, turno 2 Innovación, turno 3 Mercado, turno 4 otra vez Finanzas, y así. Si el JSON trae en el mensaje el campo opcional `"juez": 1`, `2` o `3`, habla ese juez. Cuando habla un juez, el emprendedor voltea a verlo, y cuando contesta el emprendedor, la cámara lo toma desde el lado del último juez que habló.

## Cámara

`CamaraDirector` (en el objeto `Logica`) alterna dos tomas según quién habla:

- **Close up** de quien habla. A un juez se le toma de frente para que quede centrado; al emprendedor, de tres cuartos y desde el lado del juez que le habla. En esta toma el globo baja como subtítulo para no tapar la cara, y sin el triángulo que señala a quien habla.
- **Toma general** alta y con perspectiva, para que quepan el emprendedor, los tres jueces y la pared. Se usa al inicio, cuando alguien acepta o rechaza, al mostrar el resultado y después de 2 close ups seguidos.

De la toma general al close up la cámara se desliza en 0.4 segundos; desde un close up, a la general o a otro close up, es corte directo.

En el Inspector se puede cambiar la posición de la toma general, la distancia del close up, cuántos close ups van antes de volver a la general y la duración del deslizamiento hacia el close up.

Si ya habías creado la escena con el menú Codea, vuelve a correr **Codea > Crear escena Shark Tank** para que tome la nueva distribución.

## Conectarlo con el backend

Los agentes corren en el backend [shark-tank-app](https://github.com/aefrodriguez21/shark-tank-app) (FastAPI + CrewAI), en la misma computadora que Unity:

1. En `backend/`, instala `requirements.txt`, crea `backend/.env` con la llave, el endpoint y el deployment de Azure OpenAI y arráncalo con `python main.py` (puerto 8000).
2. En `frontend/`, corre `npm install` y `npm run dev`, y abre la interfaz web (puerto 3000).
3. En Unity, en el objeto `Logica`, componente `ClienteAPI`, pon estos valores y dale Play:

| Campo | Valor |
|---|---|
| Modo | `Backend` |
| Url Base | `http://localhost:8000` |
| Ruta Backend | `/api/conversation-history` |
| Intervalo | `2` segundos |

4. En la interfaz web captura la propuesta y a los jueces y arranca el pitch. Unity muestra cada mensaje conforme llega al historial.

Mientras no haya sesión, el backend regresa el historial vacío y la consola de Unity dice que está esperando. Para usar una URL `http://` que no sea `localhost`, hay que activar **Project Settings > Player > Allow downloads over HTTP**.

Cómo se lee el historial del backend (`ClienteAPI.ConvertirBackend`):

- Cada elemento de `conversation_history` es un mensaje. `role` `Entrepreneur` es el emprendedor y `Judge` es un juez.
- El juez de la escena sale de `judge_name`: si el nombre dice Finanzas, Innovación o Mercado, habla ese juez; si no, cada nombre nuevo toma el siguiente lugar libre. Conviene nombrar a los jueces en la interfaz web como sus áreas.
- El prefijo `Nombre:` que pide el backend se quita del texto, porque el globo ya muestra el nombre.
- El primer mensaje del emprendedor es el pitch y cada mensaje suyo abre un turno nuevo. El tipo de los mensajes del juez sale del texto (pregunta si trae `?`, oferta si trae `%`, rechazo si dice “I'm out” o “no invierto”).
- Si un juez escribe calificaciones como `"innovacion": 7` (por ejemplo en un JSON), el panel muestra el promedio de las últimas calificaciones de cada juez, y ese JSON no sale en el globo. Mientras los jueces no las den, las barras quedan vacías.
- El backend no regresa todavía oferta ni decisión, así que esos campos del panel quedan pendientes.

El modo `Servidor` sigue leyendo el formato de la versión anterior del juez (`Agentes/`, `GET /sesion/actual`), descrito abajo.

### JSON del modo Servidor

Es el formato interno de la escena: el modo Backend convierte el historial a esto, y la ruta del modo Servidor lo regresa tal cual. Los nombres de los campos tienen que ser iguales porque se leen con `JsonUtility`.

```json
{
  "sesion_id": "s-0007",
  "estado": "en_curso",
  "turno": 3,
  "turno_maximo": 8,
  "decision": "contraoferta",
  "oferta_actual": { "monto": 100000, "porcentaje": 17 },
  "calificaciones": [
    { "criterio": "innovacion", "valor": 6, "evidencia": "rentamos loncheras retornables" },
    { "criterio": "sostenibilidad", "valor": 7, "evidencia": "..." },
    { "criterio": "mercado", "valor": 6, "evidencia": "..." },
    { "criterio": "rentabilidad", "valor": 5, "evidencia": "..." }
  ],
  "mensajes": [
    { "id": "m1", "emisor": "emprendedor", "tipo": "pitch", "texto": "...", "turno": 1 },
    { "id": "m2", "emisor": "juez", "tipo": "pregunta", "texto": "...", "turno": 1, "juez": 2 }
  ]
}
```

- `estado`: `en_curso` o `terminada`.
- `decision`: `pendiente`, `contraoferta`, `invertir` o `no_invertir`.
- `emisor`: `juez` o `emprendedor`.
- `juez` (opcional): qué juez habla, `1` Finanzas, `2` Innovación o `3` Mercado. Si no viene, rota por turno.
- `tipo`: `pitch`, `pregunta`, `respuesta`, `oferta`, `aceptar`, `rechazar` o `retroalimentacion`.
- Cada mensaje necesita un `id` único: así Unity sabe cuáles son nuevos.

## Scripts

| Script | Qué hace | Interacciones |
|---|---|---|
| `ClienteAPI` | Consulta el historial del backend cada 2 segundos y lo convierte al formato de la escena. En modo simulado reproduce la sesión de ejemplo. | Le pasa los datos a `GestorConversacion`. |
| `GestorConversacion` | Detecta mensajes nuevos, controla quién habla y espera el clic para pasar al siguiente. Al terminar la sesión muestra el resultado. | Actualiza personajes y globo, y le pasa las calificaciones al panel. |
| `PanelCalificaciones` | Actualiza las barras, el promedio, el turno, la oferta y la decisión. | Recibe los datos de `GestorConversacion`. |
| `PersonajeAgente` | Sirve para los dos personajes; se ilumina y se mueve cuando habla. Marca dónde está la cabeza para el close up (el juez va sentado). | Lo controla `GestorConversacion`. |
| `GloboDialogo` | Escribe el último mensaje arriba de quien habla y muestra la flecha para avanzar. | Lo controla `GestorConversacion`. |
| `BarraCriterio` | Muestra la calificación de un criterio. El color sigue la regla: rojo debajo de 4, amarillo de 4 a 6, verde de 7 en adelante. | La controla `PanelCalificaciones`. |
| `ListaComentarios` | Toggle del panel entre estadísticas y comentarios (la evidencia que ha tomado el juez, del más reciente al más viejo, con fondo blanquecino). Marca cuántos hay sin leer. | Se los pasa `PanelCalificaciones` cuando cambia la evidencia de un criterio. |
| `Entrada` | Lee clic y teclas con el sistema de entrada activo. Un clic en un botón no avanza la conversación. | La usan `GestorConversacion` y `ListaComentarios`. |
| `CamaraDirector` | Alterna entre la toma general y el close up de quien habla. | Lo llama `GestorConversacion` con cada mensaje nuevo. |
| `MirarCamara` | Mantiene los nombres de los personajes de frente a la cámara. | Va en la etiqueta de `PersonajeAgente`. |
| `ConstructorEscena` | Crea el set (pared de madera con logo, focos, tarima), los personajes y la interfaz con primitivas de Unity. | Lo usan `ArranqueAutomatico` y el menú `Codea`. |
| `ColorSolido` | Pinta las primitivas sin crear materiales. También pone una textura (logo o madera) y qué tan brillante se ve. | Lo usan el set y los personajes. |
| `SharkMindColors` | Paleta de la identidad SharkMind. | La usan `ConstructorEscena` y `PanelCalificaciones`. |

Los modelos son primitivas (cubos, cápsulas y esferas) para tener algo funcionando rápido. Se pueden cambiar por modelos reales dentro de los prefabs sin tocar los scripts.
