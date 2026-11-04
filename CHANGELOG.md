# Ecos de Nébula — La era de los guardianes

## 4 de noviembre de 2026 — Cristales despejados

- Eliminados los nombres, números y avisos flotantes sobre los cristales durante la demostración y la respuesta, tanto en campaña como en expediciones y diario.
- Se conservan los aros, colores, sonidos y las instrucciones del panel inferior. La selección sigue realizándose al entrar con Luma en cada aro.

## 3 de noviembre de 2026 — Corrección visual del lobby

- Portal reconstruido con dovelas radiales y juntas entre piezas: desaparecen las caras superpuestas del arco y de su unión con los pilares.
- Paneles del lobby opacos: el archipiélago ya no se transparenta por detrás de títulos, descripciones y controles.
- Más espacio vertical para las descripciones, evitando líneas recortadas.
- Aros del pavimento y sellos de los pedestales construidos como superficies horizontales fijas, con alturas separadas. Ya no giran hacia la cámara ni atraviesan el suelo al cambiar el ángulo.

## 3 de noviembre de 2026 — Runas a pie

- Selección exclusiva por la posición de Luma: entrar en el aro de un cristal introduce una runa.
- Eliminados los atajos 1–4, teclado numérico, E, clic sobre cristales y botones de respuesta.
- Una entrada cuenta una vez; hay que cruzar el círculo exterior para repetir. Los pequeños movimientos en el borde no duplican respuestas.
- Estar junto a un cristal cuando termina la demostración no responde automáticamente.
- Aros visibles y mensajes de proximidad; instrucciones actualizadas en campaña, ayuda y HUD compartido con expediciones.
- Las pruebas de campaña y expedición ahora responden desplazando a Luma. Nuevas comprobaciones de entrada, repetición, pausa y rechazo a distancia en las cinco dificultades.

## 28 de octubre de 2026 — Códigos bajo asedio

- Corregido el contrato de presentación: el número de destellos siempre coincide con las casillas, también en códigos repetidos y al reproducir de nuevo.
- Las celebraciones ya no contaminan el comienzo de la siguiente demostración; solo se ilumina la runa actual.
- Cinco dificultades seleccionables: Aprendiz, Explorador, Guardián, Maestro y Pesadilla.
- Combate durante la respuesta: sombras que persiguen y atacan cuerpo a cuerpo, centinelas con proyectiles y avisos rojos previos.
- R: onda del bastón con alcance corto, daño, interrupción y bloqueo de proyectiles; Q: esquiva con invulnerabilidad breve.
- Vida, escudos de combate, bajas, botones de combate y pantalla de derrota con reinicio desde el principio.
- Un error de código termina inmediatamente el recorrido, aunque queden vida o escudos. Llegar a cero vida tiene el mismo efecto.
- Se preservan desbloqueos y coleccionables. La derrota en expedición no borra la campaña independiente.
- El desafío diario fija Guardián para mantener la misma dificultad; las carreras siguen siendo una actividad separada.

Las notas del 24 de octubre siguientes describen el comportamiento anterior al combate.

## 24 de octubre de 2026

Esta expansión mantiene la campaña y la integración de OpenAI existentes. Las nuevas expediciones y el desafío diario usan reglas locales deterministas y no consumen API.

### Aventura y progresión

- Expedición independiente de seis encuentros por tres santuarios.
- Cérvix transforma cada runa en la siguiente; Nácar combina inversión y desplazamiento; Véspera utiliza runas opuestas.
- Corazones, escudos, rachas y cinco elecciones de bendiciones por recorrido.
- Refugio recupera un corazón, Paciencia ralentiza la demostración y Fortuna aumenta los puntos.
- Desafío diario con semilla por fecha UTC, récord local y bonificación una vez por día. No tiene clasificación en línea.
- XP, niveles y esencia persistentes, independientes del reinicio de campaña.
- Tres talentos de dos rangos y cuatro capas desbloqueables.
- Doce logros con recompensa única y doce páginas de historia ligadas a los fragmentos.

### Exploración y presentación

- Pip, compañero luminoso que sigue a Luma.
- Guardianes tridimensionales con máscaras, coronas, ojos y fragmentos orbitales.
- Ruinas, faroles flotantes y vestuario visible en el personaje.
- Impulso con Q, brújula de fragmentos manteniendo C y concentración con F.
- Atlas con TAB: viajes, taller, logros y memorias; pausa la acción mientras está abierto.
- Tres carreras espaciales de luciérnagas: doce balizas, cuenta atrás, límite de 65 segundos, tres medallas y récord por santuario.
- El impulso también funciona desde parado en la dirección de Luma; la brújula señala la siguiente baliza durante una carrera.

### Guardados y pruebas

- La campaña sigue en `EcosDeNebula.Journey.v1`; la progresión nueva utiliza `EcosDeNebula.Progress.v1`.
- Las expediciones no sustituyen el mundo ni los sellos del guardado de campaña. El recorrido en curso no se reanuda después de cerrar; sus recompensas ya obtenidas sí permanecen.
- Las pruebas automáticas conservan y restauran ambos guardados.
- El historial Git comienza con un respaldo de la edición anterior y separa reglas, mundo, integración y pruebas.

Consulta `VERIFICACION.md` para los resultados finales, los límites de las pruebas y el estado pendiente de la API real.
