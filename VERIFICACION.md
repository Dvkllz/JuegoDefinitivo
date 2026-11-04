# Verificación de Ecos de Nébula

Última revisión: 4 de noviembre de 2026. Equipo Windows, Unity 6000.6.5f1.

## Edición bilingüe 4.2 — 4 de noviembre

- **Localización: PASS.** 1.268 comprobaciones de catálogo, textos compuestos con números, mayúsculas, límites de palabras, cambio de idioma y caché. Ejecutar `Tests/Run-Checks.ps1`.
- **Regresión offline: PASS.** Se repitieron las 69.194 comprobaciones de patrones, expediciones y combate. Sumadas a localización: 70.462 comprobaciones.
- **Unity Play Mode: PASS.** 17 comprobaciones de carga del catálogo, persistencia de idioma, contenido independiente de ayuda/acerca de, conservación del patrón y posición de respuesta, guardado intacto y tratamiento de texto generativo de otro idioma. La prueba restaura las preferencias y guardados previos. Resultado: `TestResults/Unity-localization-result.txt`.
- **Puente: PASS.** Ocho pruebas simuladas verifican el contrato de idiomas, las instrucciones de generación, validación de patrones, errores, autenticación local y límites. Comando: `node --test Server/director.test.mjs`. No se llamó a OpenAI ni se usaron credenciales reales.
- **Compilación: PASS.** Ejecutable Windows actualizado. Log: `TestResults/Unity-bilingual-build.log`.
- **Revisión visual:** menú en español/inglés; guía española desplazada hasta el final; ayuda inglesa; Acerca de en inglés y su sección final. Revisado en la ventana del juego con área aproximada de 1280 × 720. No se afirma una revisión manual de cada pantalla ni una prueba real de generación por API.
- Paquete: `Builds/EcosDeNebula-4.2-idiomas.zip`.

## Cristales despejados — 4 de noviembre

- **Compilación: PASS.** Ejecutable actualizado sin etiquetas flotantes de runas. Log: `TestResults/Unity-v413-Cristales.log`.
- Se eliminaron el dibujo de etiquetas y sus llamadas desde campaña y expediciones. Los avisos de proximidad se mantienen únicamente en el panel inferior.
- Cambio de presentación: no se modificaron las reglas de respuesta o combate ni se repitieron pruebas de lógica. No se llamó a OpenAI.
- Paquete: `Builds/EcosDeNebula-4.1.3-cristales.zip`.

## Portal y lobby — 3 de noviembre

- **Compilación y renderizado: PASS.** Unity generó el ejecutable y tres imágenes de los santuarios. Log: `TestResults/Unity-v412-Portal.log`.
- Se inspeccionaron `Capturas/Santuario-1.png`, `Santuario-2.png` y `Santuario-3.png`: el arco usa dovelas radiales separadas por juntas y no comparte caras superpuestas con los pilares. Los aros del pavimento son mallas horizontales a alturas distintas.
- Los paneles del lobby tienen fondos opacos y las descripciones disponen de más altura para evitar recortes.
- Esta corrección es visual; no altera las reglas de selección física, combate ni guardado. No se repitió la batería de lógica ni se llamó a OpenAI.
- Paquete de esta edición: `Builds/EcosDeNebula-4.1.2-lobby.zip`.

## Runas a pie — 3 de noviembre

- **Selección física y combate: PASS.** 150 comprobaciones en Unity Play Mode, incluidas 45 nuevas sobre entrada en los aros, permanencia sin duplicados, salida y reentrada para repetir símbolos, margen de salida, bloqueo durante demostración/pausa/ayuda/Atlas y rechazo de respuestas a distancia. Las comprobaciones se repiten en las cinco dificultades. Log: `TestResults/Unity-v41-BuildCombat.log`.
- **Controles:** solo `UpdateRuneEntry` llama al método de respuesta del juego. No hay manejadores de teclas numéricas, E, clic sobre cristales ni botones de respuesta. Los números visibles son identificadores.
- **Lógica offline: PASS.** 69.194 comprobaciones de patrones, expediciones, progresión y combate. No se llamó a OpenAI.
- **Compilación: PASS.** Ejecutable Windows actualizado; el paquete de esta edición es `Builds/EcosDeNebula-4.1-runas-a-pie.zip`.

## Códigos bajo asedio — 28 de octubre

- **Regresión de códigos: PASS.** El caso de cuatro destellos/dos entradas se cubre expresamente: tras dos respuestas el intento sigue incompleto; se completa con las cuatro. El contrato de reproducción iguala siempre destellos y casillas. Los modos repetidos requieren ahora el código entero.
- **Lógica offline: PASS.** 25.944 comprobaciones de patrones, 22.031 de expediciones/progresión/carreras y 21.219 de combate/dificultad: **69.194** en total. Ejecutar `Tests/Run-Checks.ps1`.
- **Combate en Unity:** se verificaron cinco dificultades, cantidades de enemigos, pausa, advertencias previas al ataque, invulnerabilidad de esquiva, golpes para matar, creación/intercepción de proyectiles y colisiones continuas. Las pruebas también verifican que un código erróneo termine el recorrido incluso con vida y escudos, y que la muerte reinicie la campaña sin borrar XP ni memorias. Resultado: `TestResults/Unity-combat-result.txt`.
- **Campaña: PASS.** Las 31 comprobaciones vuelven a completar nueve sellos, tres santuarios y modo infinito, con la nueva derrota y reinicio. Log: `TestResults/Unity-v4-Run.log`.
- **Expediciones y carreras: PASS.** 47 comprobaciones de seis encuentros, guardianes, escudos contra enemigos, bendiciones, derrota diaria por un solo código, guardado separado y tres carreras. Resultado: `TestResults/Unity-expansion-result.txt`.
- **Inspección del ejecutable:** se revisaron selector de dificultad, demostración de cinco luces/cinco casillas, aparición de perseguidores y tiradores, HUD y derrota real por daño enemigo. Esta revisión motivó oscurecer las runas no activas, añadir pausas entre destellos y sustituir etiquetas grandes de enemigos por barras de vida compactas. No es una evaluación de rendimiento en otros equipos ni una partida manual completa en cada dificultad.
- **Guardados:** las pruebas restauran campaña, progresión y dificultad anteriores. El reinicio por derrota forma parte de la mecánica solicitada: en campaña elimina los sellos/puntos del recorrido; en expedición elimina solo el recorrido independiente. Récords, memorias y desbloqueos se conservan.
- **API:** esta actualización no crea credenciales ni hace solicitudes a OpenAI. La integración existente se conserva y la prueba real pendiente no se considera resuelta.

Las secciones siguientes son históricas. Las reglas de reintento anteriores fueron sustituidas por el reinicio completo descrito arriba.

## La era de los guardianes — 24 de octubre

- **Lógica offline: PASS.** 25.944 comprobaciones de patrones y 20.232 de expediciones, progresión y carreras: 46.176 en total. Comando reproducible: `Tests/Run-Checks.ps1`.
- **Expansión en Unity Play Mode: PASS.** 47 comprobaciones completaron seis encuentros y tres guardianes, escudo, bendiciones, derrota diaria, aislamiento del guardado de campaña, progresión, colección, vestuario, pausa del Atlas y las doce balizas de cada una de las tres carreras. Se comprobó que las recompensas no se duplicaran.
- **Regresión de campaña: PASS.** 31 comprobaciones volvieron a completar los nueve sellos, tres mundos, pausa, error, reintento, guardado/reanudación y modo infinito. Log: `TestResults/Unity-v3-campaign.log`.
- **Revisión interactiva del ejecutable:** se abrió la ventana y se revisaron menú, Atlas, taller, carrera, pausa, presentación de patrón y pantalla de respuesta. Se detectaron y corrigieron textos secundarios pequeños, bloqueo de clics detrás de paneles y claridad del modo local. No se afirma haber completado manualmente todos los recorridos ni medido el rendimiento en otros equipos.
- **Protección del guardado:** las pruebas del Editor respaldan y restauran tanto `EcosDeNebula.Journey.v1` como `EcosDeNebula.Progress.v1`. Las expediciones y carreras no reemplazan el mundo ni los sellos de campaña.
- **Compilación y escenas: PASS.** Unity generó el ejecutable Windows y tres renders de guardianes (`Capturas/Guardian-1.png` a `Guardian-3.png`). Son capturas de cámara sin HUD. Se inspeccionaron las tres imágenes y la interfaz del ejecutable por separado.
- **API:** no se creó ninguna clave ni se hizo ninguna solicitud a OpenAI en esta ampliación. `Server` y `NebulaAIClient.cs` conservan la integración anterior. Los modos nuevos son locales y se identifican como tales. La activación/prueba real pendiente de la edición anterior no se considera resuelta.

Resultados: `TestResults/Unity-expansion-result.txt` y `TestResults/Unity-smoke-result.txt`. Los logs y binarios de pruebas no se incluyen en Git; los scripts sí.

## Historial de verificación — 13 de octubre

## Edición generativa y mejoras

- **Lógica actualizada: PASS.** 25.944 comprobaciones, incluyendo validación de respuestas generativas y soluciones calculadas localmente.
- **Puente Node: PASS.** Seis pruebas verifican el contrato Responses API, respuestas inválidas/rechazadas, límites de solicitudes, autenticación local, bloqueo de solicitudes de páginas web y exclusión de generaciones simultáneas. Se usaron respuestas simuladas; estas pruebas no consumieron la API.
- **Unity + puente HTTP: PASS con simulación.** `Unity-api-result.txt` identifica expresamente `model=MOCK-CONTRACT-TEST`. Unity recibió un patrón por HTTP, lo validó y lo reprodujo. También se comprobó la cancelación y que una respuesta tardía no reemplazara el reto local.
- **Campaña tras las mejoras: PASS.** Se completaron los nueve sellos y tres mundos dentro de Unity Play Mode, con guardado, reintentos y modo Constelación.
- **Compilación actual de Windows: PASS.** `Unity-v2-build.log` confirma `NEBULA_BUILD_OK 95454853 bytes`. Los tres santuarios mejorados se renderizaron y revisaron como imágenes de cámara, sin HUD.
- **Llamada real a OpenAI: pendiente.** La clave inicial fue creada por el conector, pero un archivo temporal de la configuración desapareció antes de guardarla. `.env.local` no se creó. Se solicitó autorización para una clave de reemplazo; no se fabricaron respuestas reales ni se afirmó que la cuenta tuviera saldo o acceso al modelo.

Las comprobaciones históricas de la primera edición se conservan a continuación. Sus límites corresponden a esa fecha; para la revisión interactiva actual, consulta la sección del 24 de octubre.

## Pruebas realizadas

- **Lógica independiente: PASS.** 25.931 comprobaciones sobre 1.800 retos generados. Incluyen copia, inversión, periodicidad deducible, límites de longitud/ritmo, estados terminales, reducción de dificultad al fallar, efecto de pedir ayuda, aprendizaje de la runa más difícil y reparación del perfil.
- **Importación y compilación en Unity: PASS.** El Editor abrió la escena de entrada y validó sus referencias y shaders.
- **Campaña real en Unity Play Mode: PASS.** 31 comprobaciones de integración completaron los nueve sellos, las tres escenas procedurales 3D, un error intencional, su reintento, pausa, puntuación, guardado/restauración y una ronda del modo Constelación. Se verificó además que el mundo tuviera más de cien objetos con mallas 3D y cuatro cristales interactivos. La prueba restaura el guardado previo al terminar.
- **Ejecutable Windows: PASS.** Unity terminó la compilación final con `NEBULA_BUILD_OK`. La carpeta distribuible ocupa 95.441.877 bytes y está en `Builds/Windows`.
- **Renderizado 3D: PASS.** Unity produjo imágenes de los tres santuarios. Se revisaron los materiales, las mallas, los cristales, Luma y la composición del mundo. Las imágenes están en `Capturas/Santuario-1.png`, `Capturas/Santuario-2.png` y `Capturas/Santuario-3.png`; son renders de cámara sin la interfaz superpuesta.

## Límite de la verificación

La apertura del ejecutable mediante control de ventanas agotó el tiempo de autorización. Por eso no se afirma haber realizado una sesión manual en la ventana del ejecutable ni una revisión visual de toda la interfaz. Las pruebas de interacción y progresión se ejecutaron automáticamente dentro de Unity Play Mode. Para la comprobación manual, abre `Builds/Windows/EcosDeNebula.exe`.

El Editor produjo una excepción en su servicio interno `UnityEditor.Search.SearchDatabase` al indexar un proyecto nuevo en modo batch. Se conservó en el log y se excluyó específicamente de la clasificación de errores del juego; el servicio de búsquedas del Editor no forma parte del ejecutable. La prueba sigue fallando ante otros errores o excepciones.

Resultado de integración: `TestResults/Unity-smoke-result.txt`.

## Reproducir las pruebas

Las pruebas de lógica están en `Tests/PatternDirectorTests.cs`; las instrucciones de compilación están en `README.md`.

Para repetir la integración, con el proyecto cerrado en el Editor:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe' -batchmode -projectPath C:\juego -executeMethod EcosDeNebula.Editor.NebulaSmokeTest.Run -logFile C:\juego\TestResults\Unity-smoke.log
```

El comprobador finaliza Unity automáticamente y devuelve un código de salida distinto de cero si detecta un fallo. No añadas `-quit`: cerraría el Editor antes de terminar la prueba en Play Mode.
