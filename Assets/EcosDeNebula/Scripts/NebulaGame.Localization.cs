using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        const string LanguageKey = "EcosDeNebula.Language.v1";
        bool showAbout;
        Vector2 informationScroll;
        float informationContentHeight;

        void LoadLanguage()
        {
            TextAsset catalogue = Resources.Load<TextAsset>("NebulaEnglish");
            if (catalogue != null) NebulaLocalization.LoadEnglish(catalogue.text);
            else Debug.LogError("Missing English localisation catalogue");
            NebulaLocalization.SetLanguage(PlayerPrefs.GetString(LanguageKey,"es"));
        }

        void ChangeLanguage(string language)
        {
            NebulaLocalization.SetLanguage(language);
            PlayerPrefs.SetString(LanguageKey,NebulaLocalization.Language);
            PlayerPrefs.Save();
            informationScroll = Vector2.zero;
        }

        void LanguageSelector(Rect rect)
        {
            float half = (rect.width-6)/2;
            if (Button(new Rect(rect.x,rect.y,half,rect.height),"Español",NebulaLocalization.Language=="es")) ChangeLanguage("es");
            if (Button(new Rect(rect.x+half+6,rect.y,half,rect.height),"English",NebulaLocalization.Language=="en")) ChangeLanguage("en");
        }

        static string B(string spanish,string english)
        {
            return NebulaLocalization.Language=="en" ? english : spanish;
        }

        void OpenInformation(bool about)
        {
            showAbout=about; showHelp=true; informationScroll=Vector2.zero;
        }

        // Generated prose cannot be translated offline word for word. If the user
        // switches language mid-code, keep the code and show localised rule text.
        // The next API request is generated in the newly selected language.
        string TrialText(string value,string fallback)
        {
            return trial!=null && trial.GeneratedByApi && trial.TextLanguage!=NebulaLocalization.Language
                ? fallback : value;
        }

        string[] InformationContent()
        {
            if (showAbout) return new[] {
                B("Un archipiélago que perdió la memoria","An archipelago that lost its memory"),
                B("Ecos de Nébula es una aventura 3D de patrones, memoria y combate. Luma recorre islas suspendidas para restaurar nueve sellos y devolver sus canciones a tres santuarios.","Echoes of Nebula is a 3D adventure of patterns, memory and combat. Luma explores floating islands to restore nine seals and return their songs to three sanctuaries."),
                B("Luma, Pip y el Tejedor","Luma, Pip and the Weaver"),
                B("Luma es la guardiana que aprende escuchando. Pip, su luciérnaga compañera, ilumina el viaje. El Tejedor propone nuevos desafíos y aprende de cada intento.","Luma is a guardian who learns by listening. Pip, her firefly companion, lights the journey. The Weaver proposes new challenges and learns from each attempt."),
                B("Proyecto y diseño de interfaces","Project and interface design"),
                B("Josué Guerrero\nDiseño de Interfaces de Software\nLa respuesta al patrón ocurre dentro del mundo: el personaje selecciona los cristales caminando, mientras la interfaz comunica la regla, el progreso y los peligros.","Josué Guerrero\nSoftware Interface Design\nPattern answers happen inside the world: the character selects crystals by walking, while the interface communicates the rule, progress and dangers."),
                B("Tecnología","Technology"),
                B("Unity 6 y C# para el mundo, la interfaz y el combate. Escenarios y audio procedurales, interfaz IMGUI y progreso local con PlayerPrefs. Un puente Node.js conecta la campaña con la API de OpenAI.","Unity 6 and C# power the world, interface and combat. Procedural environments and audio, IMGUI interface and local progress with PlayerPrefs. A Node.js bridge connects the campaign to the OpenAI API."),
                B("Dos formas de crear patrones","Two ways to create patterns"),
                B("El director local adapta los retos a tus respuestas y funciona sin Internet. Con Jugar-con-IA, OpenAI puede generar patrones, títulos y pistas en el idioma seleccionado. El juego valida los retos y calcula la solución. La clave permanece en el puente local. Si la API no está disponible, el juego usa un patrón local; el panel indica su origen. Las llamadas pueden consumir saldo de tu cuenta.","The local director adapts challenges to your answers and works offline. With Jugar-con-IA, OpenAI can generate patterns, titles and hints in the selected language. The game validates challenges and calculates the solution. The key stays in the local bridge. If the API is unavailable, the game uses a local pattern; its source is shown in the panel. API calls may use your account credit."),
                B("Un mundo para seguir explorando","A world to keep exploring"),
                B("Campaña, expediciones, tres guardianes, bendiciones, carreras de luciérnagas y doce memorias. El desafío diario usa una semilla local por fecha UTC. No hay clasificación en línea.","Campaign, expeditions, three guardians, blessings, firefly races and twelve memories. The daily challenge uses a local seed based on the UTC date. There is no online leaderboard.")
            };
            return new[] {
                B("1 · Escucha y recuerda","1 · Listen and remember"),
                B("Observa los destellos de los cristales. Cada destello equivale a una casilla de respuesta, incluidas las repeticiones. Durante la demostración puedes moverte y no hay enemigos.","Watch the crystal flashes. Every flash corresponds to one answer slot, including repetitions. You can move during the demonstration, and there are no enemies."),
                B("2 · Responde con Luma","2 · Answer with Luma"),
                B("Entra en el aro de un cristal para elegir su runa automáticamente. Para repetirla, sal del círculo exterior y vuelve a entrar. Quedarte dentro no envía respuestas adicionales. Si ya estás en un aro al terminar la demostración, sal antes de seleccionarlo. No se eligen runas con números ni clics.","Enter a crystal's ring to select its rune automatically. To repeat it, leave the outer circle and enter again. Staying inside does not send extra answers. If you are already in a ring when the demonstration ends, leave before selecting it. Number keys and clicks do not select runes."),
                B("3 · Descifra y sobrevive","3 · Decode and survive"),
                B("Los enemigos aparecen al responder. Los círculos rojos anuncian ataques: Q esquiva y R lanza una onda que golpea enemigos e intercepta proyectiles. Un código incorrecto o perder toda la vida reinicia el recorrido desde el primer encuentro. Conservas nivel, talentos, aspectos, memorias y récords.","Enemies appear when it is your turn. Red circles warn of attacks: Q dodges and R sends a wave that hits enemies and intercepts projectiles. A wrong code or losing all health restarts the run from the first encounter. You keep your level, talents, outfits, memories and records."),
                B("Movimiento y combate","Movement and combat"),
                B("WASD: mover a Luma\nMayús: correr\nQ: esquivar / impulso de exploración\nR: atacar e interceptar proyectiles\n← → o arrastre con botón derecho: girar cámara\nRueda del ratón: acercar o alejar","WASD: move Luma\nShift: run\nQ: dodge / exploration dash\nR: attack and intercept projectiles\n← → or right-button drag: rotate camera\nMouse wheel: zoom in or out"),
                B("Ayudas y navegación","Help and navigation"),
                B("Espacio: volver a escuchar el patrón\nF: revelar la siguiente runa\nC (mantener): brújula de memorias o balizas\nTAB: abrir Atlas, taller, logros y memorias\nIntro: continuar un diálogo\nEsc: pausa o cerrar un panel\nLas ayudas reducen la recompensa, pero permiten avanzar.","Space: listen to the pattern again\nF: reveal the next rune\nHold C: compass for memories or beacons\nTAB: open Atlas, workshop, achievements and memories\nEnter: continue a dialogue\nEsc: pause or close a panel\nHelp reduces rewards, but still lets you progress."),
                B("Las reglas de los tres santuarios","The three sanctuary rules"),
                B("Bosque de Jade: repite el código en el mismo orden.\nMar de Ámbar: responde desde el último destello hasta el primero.\nObservatorio Violeta: repite TODO el código, incluidas todas sus repeticiones.\nLos guardianes de expedición tienen sus propias transformaciones; lee su regla antes de escuchar.","Jade Forest: repeat the code in the same order.\nAmber Sea: answer from the last flash to the first.\nViolet Observatory: repeat the ENTIRE code, including every repetition.\nExpedition guardians have their own transformations; read their rule before listening."),
                B("Dificultad y otros viajes","Difficulty and other journeys"),
                B("Elige Aprendiz, Explorador, Guardián, Maestro o Pesadilla en el inicio. A mayor dificultad, códigos más largos, menos vida y más enemigos. El desafío diario fija Guardián y desactiva talentos. En Atlas → Viajes puedes iniciar carreras: recoge doce balizas en 65 segundos. Las carreras tienen sus propias reglas y no tienen enemigos.","Choose Apprentice, Explorer, Guardian, Master or Nightmare on the home screen. Higher difficulty means longer codes, less health and more enemies. The daily challenge uses Guardian difficulty and disables talents. In Atlas → Journeys you can start races: collect twelve beacons in 65 seconds. Races have their own rules and no enemies."),
                B("Qué significa Afinidad","What Affinity means"),
                B("Es una estimación local de tu dominio de los patrones de campaña, no tu porcentaje de aciertos ni una medida de inteligencia. Empieza en 25 %. Resolver un reto sin ayuda suma 7,5 puntos; con ayuda suma 1,5; equivocarte en el código resta 12. Se mantiene entre 0 y 100 %. El director la usa al crear los patrones, pero la dificultad elegida fija la longitud final del código y el ritmo de los destellos. Expediciones y carreras no cambian este indicador.","It is a local estimate of your mastery of campaign patterns, not your accuracy percentage or a measure of intelligence. It starts at 25%. Solving a challenge without help adds 7.5 points; with help adds 1.5; a wrong code subtracts 12. It stays between 0 and 100%. The director uses it when creating patterns, but your selected difficulty sets the final code length and flash speed. Expeditions and races do not change this indicator."),
                B("Preferencias","Preferences"),
                B("Cambia entre Español y English desde el inicio o la pausa. La elección queda guardada. La pausa también permite ajustar el sonido y activar movimiento reducido. Abrir la ayuda, la pausa o el Atlas detiene el reto.","Switch between Español and English from the home screen or pause menu. Your choice is saved. Pause also lets you adjust sound and enable reduced motion. Opening help, pause or Atlas stops the challenge.")
            };
        }

        GUIStyle InformationStyle(int size,Color color)
        {
            return new GUIStyle(GUI.skin.label) { font=sans,fontSize=size,wordWrap=true,richText=false,
                normal={textColor=color},padding=new RectOffset(0,0,0,0) };
        }

        void HelpUI()
        {
            Box(new Rect(0,0,uiW,uiH),new Color(0.015f,0.035f,0.06f,0.94f));
            float width=Mathf.Min(980,uiW-64),height=Mathf.Min(790,uiH-64);
            float x=(uiW-width)/2,y=(uiH-height)/2;
            Box(new Rect(x,y,width,height),lobbyInk);
            Label(new Rect(x+32,y+24,width-250,54),showAbout?"Acerca de":"Cómo jugar",36,paper,TextAnchor.UpperLeft,true);
            if(Button(new Rect(x+width-178,y+28,146,40),"Cerrar · Esc"))showHelp=false;
            RuleLine(x+32,y+93,width-64);
            Rect viewport=new Rect(x+32,y+115,width-64,height-205);
            float contentWidth=viewport.width-30;
            string[] content=InformationContent();
            GUIStyle heading=InformationStyle(23,mint),body=InformationStyle(21,paper);
            float[] heights=new float[content.Length];
            informationContentHeight=8;
            for(int i=0;i<content.Length;i++)
            {
                heights[i]=(i%2==0?heading:body).CalcHeight(new GUIContent(content[i]),contentWidth)+4;
                informationContentHeight+=heights[i]+(i%2==0?10:30);
            }
            if(Event.current.type==EventType.KeyDown)
            {
                if(Event.current.keyCode==KeyCode.PageDown) {informationScroll.y+=viewport.height*0.8f;Event.current.Use();}
                else if(Event.current.keyCode==KeyCode.PageUp) {informationScroll.y-=viewport.height*0.8f;Event.current.Use();}
            }
            informationScroll.y=Mathf.Clamp(informationScroll.y,0,Mathf.Max(0,informationContentHeight-viewport.height));
            informationScroll=GUI.BeginScrollView(viewport,informationScroll,new Rect(0,0,contentWidth,informationContentHeight),false,true);
            float cy=4;
            for(int i=0;i<content.Length;i++)
            {
                GUI.Label(new Rect(0,cy,contentWidth,heights[i]),content[i],i%2==0?heading:body);
                cy+=heights[i]+(i%2==0?10:30);
            }
            GUI.EndScrollView();
            RuleLine(x+32,y+height-77,width-64);
            Label(new Rect(x+32,y+height-60,width-64,34),B("Rueda o Re Pág / Av Pág para leer · Esc para cerrar","Scroll or Page Up / Page Down to read · Esc to close"),16,dim,TextAnchor.MiddleCenter);
        }
    }
}
