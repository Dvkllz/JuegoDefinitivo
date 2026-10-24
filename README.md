# Ecos de Nébula

Una aventura original **3D para Unity 6**, en español, sobre memoria, patrones y una IA que aprende de tus intentos. La edición generativa conecta con la **API de OpenAI** para crear patrones, nombres, mensajes y pistas. También incluye un modo local sin conexión.

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
| Observatorio Violeta | Observa un bloque repetido dos veces y completa la siguiente repetición. |

Tres sellos por mundo, nueve en total, final de la historia y modo Constelación de patrones ilimitados. Los errores permiten reintentar; no eliminan sellos. Pedir otra reproducción reduce la puntuación de ese sello, pero permite avanzar.

La nueva edición incluye **doce fragmentos de memoria** para encontrar caminando, botones de runas en pantalla, pistas durante el reto, celebraciones de luz y escenarios diferenciados: bosque mineral, velas de ámbar y columnas astrales. Cada fragmento añade 40 destellos y se guarda con tu progreso.

## Controles

| Acción | Control |
| --- | --- |
| Activar cristal | Teclas 1–4, teclado numérico o clic en el cristal |
| Mover a Luma | WASD; Mayús para correr |
| Activar cristal cercano | E |
| Girar cámara | Flechas izquierda/derecha o arrastre con botón derecho |
| Zoom | Rueda del ratón |
| Volver a escuchar | Espacio o botón de ayuda |
| Continuar en un diálogo | Intro |
| Pausa | Esc |

La pausa ofrece sonido y movimiento reducido. Las runas se distinguen por número, nombre, forma, color y sonido. No hay límite de tiempo.

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

Pruebas del puente y del contrato generativo: `node --test Server/director.test.mjs`. Estas pruebas usan respuestas simuladas y no consumen la API. `node Server/live-check.mjs` sí realiza una solicitud real y guarda únicamente el resultado del reto, sin secretos.
