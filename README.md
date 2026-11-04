# Ecos de Nébula

## Español e inglés — 4 de noviembre de 2026

Selecciona **Español / English** en la barra superior o en la pausa. El cambio es inmediato y queda guardado; no reinicia el patrón ni modifica el progreso. Incluye menús, HUD, combate, dificultades, Atlas, talentos, logros, memorias y reglas de los guardianes.

**Cómo jugar** ahora contiene una guía propia de controles, selección física, combate y reglas. Usa la rueda, la barra lateral o Re Pág / Av Pág para recorrerla; el alto de los párrafos se calcula con su texto para evitar recortes. **Acerca de** muestra el lore, los personajes, el stack y los créditos de Josué Guerrero — Diseño de Interfaces de Software.

La integración generativa solicita títulos y pistas en el idioma seleccionado. Si cambias de idioma en un reto ya generado, conserva el código y muestra texto local traducido hasta la siguiente generación. Las pruebas de esta actualización son locales y simuladas; no se hicieron llamadas de pago a OpenAI.

## Runas a pie — 3 de noviembre de 2026

Ahora el patrón se responde **solo con el personaje**: mueve a Luma con WASD hasta entrar en el aro de color que rodea cada cristal. La entrada activa una única runa automáticamente. Para repetirla, cruza el círculo exterior y vuelve a entrar. No hace falta pulsar una tecla de interacción; los números identifican las runas, pero no son atajos.

Durante la demostración puedes moverte sin responder. Si terminas de escuchar dentro de un aro, sal y vuelve a entrar para elegirlo. Los enemigos siguen atacando mientras recorres el santuario; usa Q para esquivar y R para combatir.

## Códigos bajo asedio — 28 de octubre de 2026

**Cada destello corresponde ahora a una entrada.** Se corrigió el caso de cuatro luces y solo dos respuestas del Observatorio: hay que introducir el código completo, incluyendo las repeticiones. La pantalla indica ambos conteos y separa las luces de demostración de las celebraciones.

Al responder aparecen **sombras perseguidoras** y **centinelas que disparan**. Sus círculos rojos anuncian el ataque. Muévete con WASD, esquiva con **Q** (0,32 s de invulnerabilidad) y ataca con **R**: una onda de alcance corto que golpea enemigos y destruye proyectiles cercanos. También hay botones de ataque/esquiva en pantalla. La demostración inicial es segura; los enemigos entran al responder. Pausa y Atlas detienen el combate.

**Un código incorrecto o quedarse sin vida reinicia todo el recorrido**, no el mismo acertijo. En campaña se reinician mundo, sellos y puntuación del recorrido; en expedición se vuelve al primer encuentro sin borrar la campaña separada. Se conservan récords, XP, talentos, aspectos y memorias. Los escudos solo protegen de golpes enemigos, nunca de un código incorrecto.

Elige la dificultad en el menú antes de jugar:

| Dificultad | Runas / entradas | Vida base | Máximo de enemigos | Golpes para vencer un enemigo |
| --- | --- | --- | --- | --- |
| Aprendiz | 3 | 5 | 1 | 1 |
| Explorador | 4 | 4 | 2 | 1 |
| Guardián | 5 | 3 | 3 | 2 |
| Maestro | 6 | 2 | 4 | 2 |
| Pesadilla | 7 | 1 | 5 | 3 |

Las dificultades superiores también aceleran enemigos, proyectiles y la demostración. Los talentos de vida/escudo se aplican a expediciones normales. El desafío diario fija **Guardián**, sin talentos. Las carreras mantienen sus propias reglas y no tienen enemigos.

## Nueva expansión: La era de los guardianes

Además de la campaña original, ahora puedes jugar expediciones de seis encuentros, enfrentarte a tres guardianes con reglas propias, elegir bendiciones y conseguir XP, esencia, talentos y cuatro capas. El Atlas (TAB) reúne viajes, taller, doce logros y doce memorias de historia.

También hay **tres carreras 3D de luciérnagas**: doce balizas en 65 segundos, medallas de oro/plata/bronce y récords locales. Inícialas desde **Atlas → Viajes → Carrera 3D**. El desafío diario tiene semilla por fecha UTC, sin ventajas de talentos ni clasificación en línea.

Todo este contenido nuevo funciona sin Internet. **La API no se ha activado ni probado de nuevo en esta ampliación**; conserva la integración y el estado de configuración anterior. Las expediciones indican claramente que sus reglas son locales.

Abre `Jugar.cmd` para el modo local o el ejecutable de `Builds/Windows`. Consulta `CHANGELOG.md` para las novedades completas.

Una aventura original **3D para Unity 6**, en español e inglés, sobre memoria, patrones y una IA que aprende de tus intentos. La edición generativa conecta con la **API de OpenAI** para crear patrones, nombres, mensajes y pistas. También incluye un modo local sin conexión.

## Abrir y jugar

**Con IA generativa:** abre `Jugar-con-IA.cmd`. Requiere Node.js 22.16+ (este equipo tiene Node 24), conexión a Internet y la clave guardada de forma segura en `.env.local`. El lanzador inicia el puente y el juego, y cierra el puente cuando terminas. Las llamadas a OpenAI pueden consumir saldo de tu cuenta.

**Modo local:** abre `Builds/Windows/EcosDeNebula.exe`. Conserva el ejecutable junto a su carpeta `EcosDeNebula_Data` y los demás archivos de `Windows`. El panel del Tejedor identifica el origen real del patrón; no presenta el modo local como una respuesta de OpenAI.

Para editar el juego en Unity:

1. Usa **Unity 6000.6.5f1**, instalado en este equipo durante la creación del proyecto.
2. En Unity Hub, selecciona **Añadir → Añadir proyecto desde disco** y elige `C:\juego`.
3. Abre `Assets/EcosDeNebula/Scenes/Nebula.unity` (o **Nébula → Abrir aventura**).
4. Pulsa **Play** y después **Despertar el primer eco** en la vista Game.

La escena contiene el controlador; el mundo 3D se construye al pulsar Play. Se recomienda la vista Game a 16:9 y un tamaño mínimo de 1280 × 720. El renderizado usa el pipeline integrado, sin paquetes URP ni descargas adicionales.

## La aventura

Luma, una pequeña guardiana con capa y bastón luminoso, viaja por un archipiélago suspendido. El Tejedor, un autómata de anillos dorados, inventa las pruebas que despiertan cada isla.

| Santuario | Regla |
| --- | --- |
| Bosque de Jade | Recuerda la secuencia y repítela. |
| Mar de Ámbar | Devuelve la secuencia en orden inverso. |
| Observatorio Violeta | Devuelve el código completo, incluidas todas sus repeticiones. |

Tres sellos por mundo, nueve en total, final de la historia y modo Constelación de patrones ilimitados. Un error de código o perder toda la vida devuelve la campaña al primer santuario, sin sellos ni puntos de recorrido. Pedir otra reproducción cuenta como ayuda y reduce la recompensa, pero no es un fallo.

La nueva edición incluye **doce fragmentos de memoria** para encontrar caminando, aros de activación junto a los cristales, pistas durante el reto, celebraciones de luz y escenarios diferenciados: bosque mineral, velas de ámbar y columnas astrales. Cada fragmento añade 40 destellos y se guarda con tu progreso.

## Controles

| Acción | Control |
| --- | --- |
| Activar cristal | Entrar con Luma en su aro de color |
| Mover a Luma | WASD; Mayús para correr |
| Repetir una runa | Salir del círculo exterior y volver a entrar |
| Girar cámara | Flechas izquierda/derecha o arrastre con botón derecho |
| Zoom | Rueda del ratón |
| Volver a escuchar | Espacio o botón de ayuda |
| Continuar en un diálogo | Intro |
| Pausa | Esc |
| Atlas / taller / diario | TAB |
| Impulso | Q (recarga visible en el HUD) |
| Atacar / interceptar proyectiles | R (onda a 3 metros, recarga de 0,48 s) |
| Brújula de memorias o balizas | Mantener C |
| Revelar la siguiente runa | F (cuenta como ayuda) |

La pausa ofrece sonido y movimiento reducido. Las runas se distinguen por número, nombre, forma, color y sonido. Los códigos no tienen cronómetro, pero los enemigos presionan mientras respondes; las carreras sí tienen un límite de 65 segundos.

## Qué hace la IA

La integración usa la **Responses API de OpenAI**, con `gpt-4.1-mini` como modelo configurable y Structured Outputs. El modelo recibe únicamente el mundo, el tamaño del patrón y estadísticas de aciertos/errores; devuelve un motivo de runas, un título, un mensaje, una pista y la forma de una constelación decorativa. No recibe claves, nombres personales ni archivos del equipo.

El servidor y Unity validan el resultado. La solución correcta se calcula en código: invertir para Mar de Ámbar o repetir el motivo en el Observatorio. Una respuesta inválida, rechazo, falta de cuota o desconexión activa el modo local. El juego muestra el estado y permite saltar la espera. Se puede desactivar la generación por API en la pausa.

`PatternDirector.cs` sigue ajustando longitud y ritmo a tus resultados y sirve como generador local de respaldo. El progreso y aprendizaje se guardan mediante PlayerPrefs. Nueva aventura reinicia la campaña y el aprendizaje, conservando el récord.

## Clave y puente local

La clave permanece en `C:\juego\.env.local`, fuera de `Assets` y de los paquetes distribuibles. Unity **nunca** lee ese archivo ni recibe la clave de OpenAI. El puente Node escucha exclusivamente en `127.0.0.1`, autentica al juego con un token temporal y envía las llamadas a OpenAI por HTTPS. Rechaza peticiones de páginas web, procesa una generación a la vez, espera después de fallos y limita cada sesión a 60 solicitudes. No registra credenciales ni cuerpos de errores del proveedor.

En una copia del paquete, el archivo `.env.local` debe estar junto a `Jugar-con-IA.cmd`. El paquete no incluye credenciales; sin una clave propia configurada, funciona en modo local.

Para usar el puente desde el Editor, ejecuta `node Server/start.mjs --server-only` antes de pulsar Play. Termínalo con Ctrl+C. No compartas `.env.local` ni `.nebula-session.json`.

El modelo puede cambiarse mediante `NEBULA_OPENAI_MODEL` en el entorno del puente. Su disponibilidad depende del acceso de tu proyecto de OpenAI.

Documentación usada: [Structured Outputs](https://developers.openai.com/api/docs/guides/structured-outputs) y [GPT-4.1 mini](https://developers.openai.com/api/docs/models/gpt-4.1-mini).

## Ejecutable y verificación

Desde Unity: **Nébula → Crear ejecutable de Windows**. El resultado se guarda en `Builds/Windows/EcosDeNebula.exe`.

Las pruebas de lógica no dependen del Editor. En Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force TestResults
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /out:TestResults\PatternDirectorTests.exe Assets\EcosDeNebula\Scripts\PatternDirector.cs Tests\PatternDirectorTests.cs
& .\TestResults\PatternDirectorTests.exe
```

Validación adicional con el Editor instalado:

```powershell
& '<ruta al Editor>\Unity.exe' -batchmode -projectPath C:\juego -executeMethod EcosDeNebula.Editor.NebulaEditor.Validate -logFile C:\juego\TestResults\Unity-validation.log -quit
```

Consulta `VERIFICACION.md` para conocer qué se pudo comprobar en el equipo y qué queda pendiente.

Para comprobar toda la lógica offline: `powershell -File Tests/Run-Checks.ps1`. Añade `-Unity` para ejecutar también la campaña completa y las expediciones/carreras en Play Mode; cierra antes el proyecto en el Editor. Las pruebas restauran los guardados previos. No hacen llamadas a OpenAI.

Los talentos, experiencia y aspectos sobreviven a **Nueva aventura**. Esa opción sí reinicia la campaña y sus memorias. Las expediciones y carreras no modifican sus sellos ni su mundo. Las recompensas de cada encuentro se guardan, pero los recorridos en curso no se reanudan al cerrar el juego.

Pruebas del puente y del contrato generativo: `node --test Server/director.test.mjs`. Estas pruebas usan respuestas simuladas y no consumen la API. `node Server/live-check.mjs` sí realiza una solicitud real y guarda únicamente el resultado del reto, sin secretos.
