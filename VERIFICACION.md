# Verificación de Ecos de Nébula

Fecha: 13 de octubre de 2026. Equipo Windows, Unity 6000.6.5f1.

## Edición generativa y mejoras

- **Lógica actualizada: PASS.** 25.944 comprobaciones, incluyendo validación de respuestas generativas y soluciones calculadas localmente.
- **Puente Node: PASS.** Seis pruebas verifican el contrato Responses API, respuestas inválidas/rechazadas, límites de solicitudes, autenticación local, bloqueo de solicitudes de páginas web y exclusión de generaciones simultáneas. Se usaron respuestas simuladas; estas pruebas no consumieron la API.
- **Unity + puente HTTP: PASS con simulación.** `Unity-api-result.txt` identifica expresamente `model=MOCK-CONTRACT-TEST`. Unity recibió un patrón por HTTP, lo validó y lo reprodujo. También se comprobó la cancelación y que una respuesta tardía no reemplazara el reto local.
- **Campaña tras las mejoras: PASS.** Se completaron los nueve sellos y tres mundos dentro de Unity Play Mode, con guardado, reintentos y modo Constelación.
- **Compilación actual de Windows: PASS.** `Unity-v2-build.log` confirma `NEBULA_BUILD_OK 95454853 bytes`. Los tres santuarios mejorados se renderizaron y revisaron como imágenes de cámara, sin HUD.
- **Llamada real a OpenAI: pendiente.** La clave inicial fue creada por el conector, pero un archivo temporal de la configuración desapareció antes de guardarla. `.env.local` no se creó. Se solicitó autorización para una clave de reemplazo; no se fabricaron respuestas reales ni se afirmó que la cuenta tuviera saldo o acceso al modelo.

Las comprobaciones históricas de la primera edición se conservan a continuación. La revisión interactiva completa de la ventana continúa pendiente.

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
