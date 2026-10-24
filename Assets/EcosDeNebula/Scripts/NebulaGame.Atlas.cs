using System;
using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        void ExpansionUI()
        {
            bool active=!paused&&!showHelp;
            if(Button(new Rect(uiW-499,22,124,40),"Atlas · TAB",false,active))panel=1;
            if(phase==JourneyPhase.Home)
            {
                float x=uiW-474;
                Box(new Rect(x,130,426,315),ink);
                Label(new Rect(x+26,151,375,24),"LA ERA DE LOS GUARDIANES",12,mint);
                Label(new Rect(x+25,187,376,49),"Un cielo por conquistar",29,paper,TextAnchor.UpperLeft,true);
                Label(new Rect(x+26,246,375,55),"6 encuentros · 3 guardianes · bendiciones\nTalentos, aspectos y un nuevo reto cada día.",16,dim);
                if(Button(new Rect(x+26,321,374,48),"Entrar en expedición   →",true,active))StartExpedition(false);
                if(Button(new Rect(x+26,382,180,40),"Desafío diario",false,active))StartExpedition(true);
                if(Button(new Rect(x+218,382,182,40),"Taller y diario",false,active))panel=2;
                Box(new Rect(x,461,426,68),ink);
                Label(new Rect(x+25,479,380,34),"NIVEL "+progress.Level+"   /   "+progress.dust+" ESENCIA   /   "+BitCount(progress.achievements)+" LOGROS",12,mint);
            }
            else if(phase!=JourneyPhase.Ending)
            {
                Box(new Rect(30,514,272,127),ink);
                Label(new Rect(50,528,231,20),"LUMA · NIVEL "+progress.Level+"   /   "+progress.dust+" ESENCIA",11,mint);
                Box(new Rect(50,557,230,4),new Color(0.16f,0.24f,0.28f));
                Box(new Rect(50,557,230*(progress.xp%200)/200f,4),mint);
                Label(new Rect(50,574,231,55),"Q · impulso "+(dashCooldown>0?dashCooldown.ToString("0.0")+" s":"listo")+"\nC · brújula     F · revelar siguiente",12,dim);
            }
        }

        void AtlasUI()
        {
            Box(new Rect(0,0,uiW,uiH),new Color(0.015f,0.03f,0.055f,0.94f));
            float x=uiW/2-550,y=uiH/2-350;
            Box(new Rect(x,y,1100,700),ink);
            Label(new Rect(x+30,y+20,850,43),"Atlas de Luma",32,paper,TextAnchor.UpperLeft,true);
            if(Button(new Rect(x+944,y+22,126,36),"Cerrar · Esc"))panel=0;
            string[] tabs={"Viajes","Taller","Logros","Memorias"};
            for(int i=0;i<4;i++)if(Button(new Rect(x+30+i*262,y+80,252,44),tabs[i],panel==i+1))panel=i+1;
            Label(new Rect(x+30,y+140,1040,28),"NIVEL "+progress.Level+"   ·   "+progress.xp+" XP   ·   "+progress.dust+" ESENCIA   ·   "+progress.runs+" EXPEDICIONES COMPLETADAS",12,mint);
            if(panel==1)TravelPanel(x,y);
            else if(panel==2)WorkshopPanel(x,y);
            else if(panel==3)AchievementsPanel(x,y);
            else MemoriesPanel(x,y);
        }

        void TravelPanel(float x,float y)
        {
            for(int i=0;i<3;i++)
            {
                float cx=x+30+i*352;
                Box(new Rect(cx,y+190,336,209),new Color(0.08f,0.14f,0.19f));
                Box(new Rect(cx,y+190,336,4),NebulaWorld.RuneColors[i==2?3:i]);
                Label(new Rect(cx+20,y+213,295,62),WorldNames[i],28,paper,TextAnchor.UpperLeft,true);
                Label(new Rect(cx+20,y+285,295,65),ExpeditionRun.Guardians[i],18,mint);
                Label(new Rect(cx+20,y+363,295,24),(progress.guardians&(1<<i))!=0?"GUARDIÁN RECONCILIADO":"GUARDIÁN POR DESCUBRIR",11,dim);
            }
            Label(new Rect(x+34,y+423,1020,64),"La expedición es una aventura independiente: tres corazones, seis pruebas y una bendición entre encuentros. Los errores gastan corazones; tu campaña nunca pierde sellos. Al retirarte conservas las recompensas obtenidas, pero el recorrido empieza de nuevo.",17,paper);
            int today=ExpeditionRun.DailySeed(DateTime.UtcNow);
            Label(new Rect(x+34,y+505,1020,48),"DIARIO · "+DateTime.UtcNow.ToString("yyyy-MM-dd")+" UTC · Mejor: "+(progress.dailyDate==today?progress.dailyBest:0)+"\nSemilla fija local. Talentos desactivados. Bonificación de victoria: una vez al día. Sin clasificación en línea.",14,dim);
            bool canStart=phase==JourneyPhase.Home;
            if(Button(new Rect(x+30,y+586,510,54),"Iniciar expedición",true,canStart))StartExpedition(false);
            if(Button(new Rect(x+560,y+586,510,54),"Jugar desafío diario",false,canStart))StartExpedition(true);
            Label(new Rect(x+34,y+652,1020,24),canStart?"Campaña y expediciones comparten XP, logros y cosméticos. La IA generativa sigue disponible en la campaña.":"Vuelve al inicio para iniciar otro viaje. Este panel pausa el reto actual.",12,dim);
        }

        void WorkshopPanel(float x,float y)
        {
            string[] names={"Corazón de raíz","Égida de cristal","Paso de brisa"};
            string[] descriptions={"+1 corazón inicial por rango en expediciones normales.","Absorbe un error por rango sin perder posición ni corazón.","Reduce 0,65 s la recarga del impulso Q por rango."};
            for(int i=0;i<3;i++)
            {
                float cx=x+30+i*352;int rank=progress.TalentRank(i);
                Box(new Rect(cx,y+187,336,251),new Color(0.08f,0.14f,0.19f));
                Label(new Rect(cx+20,y+208,295,46),names[i],25,paper,TextAnchor.UpperLeft,true);
                Label(new Rect(cx+20,y+267,295,72),descriptions[i],16,dim);
                Label(new Rect(cx+20,y+343,295,26),"RANGO "+rank+" / 2",12,mint);
                if(Button(new Rect(cx+20,y+383,295,37),rank==2?"Completado":"Mejorar · "+progress.TalentCost(i)+" esencia",false,rank<2&&progress.dust>=progress.TalentCost(i)&&expedition==null))
                {progress.Buy(i);SaveJourney();}
            }
            Label(new Rect(x+30,y+457,1040,29),"VESTUARIO · Los aspectos se desbloquean jugando y no se consumen.",13,mint);
            string[] skins={"Jade · inicial","Ámbar · nivel 3","Glacial · un guardián","Astral · una expedición"};
            for(int i=0;i<4;i++)
            {
                float cx=x+30+i*264;bool unlocked=progress.SkinUnlocked(i);
                Box(new Rect(cx,y+502,246,8),NebulaWorld.CloakColors[i]);
                Label(new Rect(cx,y+524,246,46),skins[i],16,paper,TextAnchor.MiddleCenter);
                if(Button(new Rect(cx,y+583,246,41),!unlocked?"Bloqueado":progress.skin==i?"Equipado":"Equipar",progress.skin==i,unlocked))
                {progress.skin=i;world.SetCloak(i);SaveJourney();}
            }
            Label(new Rect(x+30,y+655,1040,24),expedition!=null?"Termina el recorrido antes de comprar talentos. Los aspectos se pueden cambiar siempre.":"Los talentos y aspectos permanecen al comenzar una nueva campaña.",12,dim);
        }

        void AchievementsPanel(float x,float y)
        {
            for(int i=0;i<12;i++)
            {
                int col=i%3,row=i/3;float cx=x+30+col*352,cy=y+183+row*119;
                bool earned=(progress.achievements&(1<<i))!=0;
                Box(new Rect(cx,cy,336,105),new Color(0.08f,0.14f,0.19f));
                Box(new Rect(cx,cy,3,105),earned?mint:dim);
                Label(new Rect(cx+16,cy+13,305,27),(earned?"✓ ":"○ ")+AchievementNames[i],18,earned?mint:paper);
                Label(new Rect(cx+16,cy+46,305,48),AchievementHints[i]+(earned?" · Obtenido":" · +30 XP / +15 esencia"),13,dim);
            }
        }

        static readonly string[] MemoryTitles={"La primera semilla","El nombre de Pip","Un bosque sin olvido","La promesa de Cérvix","El mar detenido","La brújula rota","Las velas del ayer","El reloj de Nácar","La estrella ausente","Cartas del Tejedor","La capa de Luma","El secreto de Véspera"};
        static readonly string[] MemoryStories={
            "Antes de que existieran las islas, una niña guardó cuatro notas en una semilla. Cada árbol de Jade nació de una de ellas.",
            "Pip no era una estrella pequeña. Era una luciérnaga que se negó a abandonar la noche. Ahora encuentra el camino junto a Luma.",
            "Los árboles no recuerdan palabras. Recuerdan el orden de la lluvia, del sol y del silencio. Por eso sus canciones son patrones.",
            "Cérvix prometió que ninguna canción se repetiría sin cambiar. Su corona hace avanzar cada nota un paso en el círculo.",
            "Una ola quedó suspendida cuando el cielo perdió la memoria. Las velas de ámbar son sus gotas: todavía esperan llegar a la orilla.",
            "La brújula no apunta al norte. Apunta a lo que falta. Luma la reparó dejando de buscar respuestas y empezando a escuchar.",
            "Las navegantes de Ámbar aprendieron a cantar desde el final. Así encontraban el camino a casa incluso cuando el tiempo retrocedía.",
            "Nácar no custodia horas. Custodia despedidas. Para cruzar su reloj debes volver atrás y dar después un paso nuevo.",
            "En el observatorio hay un lugar vacío. No es una estrella perdida: es el lugar reservado para la historia que todavía escribirás.",
            "El Tejedor anotaba cada error como si fuera un mapa. No buscaba una guardiana perfecta. Buscaba a alguien que no dejara de aprender.",
            "La capa de Luma cambia de color con sus viajes. Jade por escuchar, ámbar por crecer, hielo por resistir y violeta por volver.",
            "Véspera aprendió que una sombra no es lo contrario de la luz. Es su compañera. Sus runas opuestas sólo cuentan la otra mitad de la canción."
        };
        void MemoriesPanel(float x,float y)
        {
            for(int i=0;i<12;i++)
            {
                bool found=(save.fragments&(1<<i))!=0;
                if(Button(new Rect(x+30,y+184+i*39,344,32),(i+1).ToString("00")+" · "+(found?MemoryTitles[i]:"Memoria por encontrar"),journalPage==i))journalPage=i;
            }
            bool open=(save.fragments&(1<<journalPage))!=0;
            Box(new Rect(x+397,y+184,673,465),new Color(0.08f,0.14f,0.19f));
            Label(new Rect(x+429,y+215,605,28),WorldNames[journalPage/4].ToUpperInvariant(),12,mint);
            Label(new Rect(x+427,y+270,609,92),open?MemoryTitles[journalPage]:"Un recuerdo espera",39,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+429,y+384,605,178),open?MemoryStories[journalPage]:"Busca los cuatro fragmentos dorados en este santuario. Mantén C mientras caminas para que la brújula señale el más cercano. Puedes encontrarlos en campaña o en expediciones.",22,dim);
            Label(new Rect(x+429,y+598,605,30),BitCount(save.fragments)+" / 12 MEMORIAS RECUPERADAS",12,mint);
        }

        void ExpeditionHUD()
        {
            Box(new Rect(30,112,272,389),ink);
            Label(new Rect(50,132,232,24),expedition.Daily?"DESAFÍO DIARIO · LOCAL":"EXPEDICIÓN · LOCAL",11,mint);
            Label(new Rect(49,171,232,74),WorldNames[world.Biome],29,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(50,267,232,33),"ENCUENTROS   "+expedition.Round+" / 6",12,mint);
            Label(new Rect(50,314,232,40),"CORAZONES   "+expedition.Hearts+"\nESCUDOS   "+expedition.Shields,14,paper);
            Label(new Rect(49,382,232,43),expedition.Score.ToString("N0"),33,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(50,444,232,30),"RACHA   "+expedition.Combo+"   /   FORTUNA   "+expedition.Fortune,11,dim);
            Box(new Rect(uiW-278,112,248,272),ink);
            Label(new Rect(uiW-256,133,203,48),trial.Title,23,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(uiW-256,201,203,103),trial.Hint,17,mint);
            Label(new Rect(uiW-256,326,203,42),"Reglas locales verificadas.\nNo usa ni consume la API.",12,dim);
            if(phase==JourneyPhase.Watching||phase==JourneyPhase.Answering)RuneLabels();
        }
        void ExpeditionBriefingUI()
        {
            float x=uiW/2-350,y=uiH-310;
            Box(new Rect(x,y,700,272),ink);Box(new Rect(x,y,700,3),expedition.Boss?amber:mint);
            Label(new Rect(x+28,y+22,644,24),"ENCUENTRO "+(expedition.Round+1)+" / 6 · "+(expedition.Boss?"GUARDIÁN":"SENDERO"),12,mint);
            Label(new Rect(x+27,y+60,646,54),trial.Title,33,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+28,y+125,644,60),trial.Hint,21,paper);
            if(Button(new Rect(x+28,y+205,644,43),"Escuchar y afrontar el encuentro",true,!paused&&!showHelp))BeginTrial();
        }
        void ExpeditionResultUI(bool success)
        {
            float x=uiW/2-350,y=uiH-284;
            Box(new Rect(x,y,700,246),ink);
            Label(new Rect(x+28,y+24,644,53),success?"El vínculo se fortalece":"El eco pide otra oportunidad",31,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+28,y+96,644,65),success?"+"+scoreGain+" puntos de expedición. "+(assisted?"Completado con ayuda.":"Resuelto sin ayuda."):
                "Se esperaba "+(lastWrong+1)+" · "+RuneNames[Mathf.Clamp(lastWrong,0,3)]+". Quedan "+expedition.Hearts+" corazones. La secuencia no cambia al reintentar.",18,dim);
            if(Button(new Rect(x+28,y+180,644,44),success?(expedition.Won?"Contemplar el nuevo cielo":"Elegir una bendición"):"Escuchar y reintentar",true,!paused&&!showHelp))
            {if(success)AdvanceExpedition();else Replay();}
        }
        void BlessingUI()
        {
            float x=uiW/2-450,y=uiH/2-115;
            Box(new Rect(x-20,y-85,940,350),ink);
            Label(new Rect(x,y-64,900,44),"Elige qué llevar al siguiente encuentro",32,paper,TextAnchor.MiddleCenter,true);
            string[] titles={"Refugio","Paciencia","Fortuna"};
            string[] body={"Recupera un corazón.\nMáximo: cinco.","Escucha más despacio.\n+0,15 s por nota.","Cada encuentro otorga\n50 puntos adicionales."};
            for(int i=0;i<3;i++)
            {
                float cx=x+i*310;
                Box(new Rect(cx,y,280,230),new Color(0.08f,0.14f,0.19f));
                Label(new Rect(cx+18,y+20,244,40),titles[i],27,mint,TextAnchor.MiddleCenter,true);
                Label(new Rect(cx+18,y+83,244,65),body[i],17,paper,TextAnchor.MiddleCenter);
                if(Button(new Rect(cx+18,y+174,244,38),"Elegir",false,!paused&&!showHelp))ChooseBlessing(i);
            }
        }
        void ExpeditionEndingUI()
        {
            float x=uiW/2-340;
            Box(new Rect(x,175,680,574),ink);
            Label(new Rect(x+40,209,600,28),expedition.Daily?"DESAFÍO DIARIO FINALIZADO":"FIN DE LA EXPEDICIÓN",12,mint,TextAnchor.MiddleCenter);
            Label(new Rect(x+35,271,610,110),expedition.Won?"Los guardianes\nrecuerdan tu nombre.":"Toda travesía\ndeja una luz.",45,paper,TextAnchor.MiddleCenter,true);
            Label(new Rect(x+40,417,600,46),expedition.Score+" puntos · "+expedition.Round+" / 6 encuentros",24,mint,TextAnchor.MiddleCenter);
            Label(new Rect(x+50,488,580,85),"Tus talentos, memorias y recompensas permanecen.\nLa campaña sigue exactamente donde la dejaste.\nVisita el taller para preparar tu próxima travesía.",18,dim,TextAnchor.MiddleCenter);
            if(Button(new Rect(x+40,608,600,55),"Volver al inicio",true,!paused&&!showHelp))ReturnHome();
        }
    }
}
