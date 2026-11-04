using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        void CombatUI()
        {
            if(phase==JourneyPhase.Home)
            {
                float x=uiW-474;
                Box(new Rect(x,547,426,181),lobbyInk);
                Label(new Rect(x+22,561,382,25),"DIFICULTAD · "+Difficulty.Name.ToUpperInvariant(),16,mint);
                for(int i=0;i<5;i++)
                    if(Button(new Rect(x+22+i*77,599,68,34),(i+1).ToString(),difficulty==i,!paused&&!showHelp))
                    {difficulty=i;SaveJourney();}
                Label(new Rect(x+22,645,382,61),Difficulty.CodeLength+" runas · "+Difficulty.Health+" vida · hasta "+Difficulty.Enemies+" enemigos\nUn código incorrecto reinicia el recorrido.\nEl diario fija la dificultad en Guardián.",14,dim);
                return;
            }
            if(combat==null||race!=null||phase==JourneyPhase.Defeat||phase==JourneyPhase.Ending)return;
            Box(new Rect(30,652,272,165),ink);
            Label(new Rect(50,666,232,24),Difficulty.Name.ToUpperInvariant()+" · VIDA "+combat.Health+" / "+combat.MaxHealth,15,combat.Immunity>0?mint:paper);
            for(int i=0;i<combat.MaxHealth;i++)Box(new Rect(50+i*29,704,23,9),i<combat.Health?mint:new Color(0.26f,0.13f,0.17f));
            Label(new Rect(50,726,232,25),"Escudos "+combat.Shields+" · Enemigos "+enemies.Count+" · Bajas "+runKills,14,dim);
            bool enabled=phase==JourneyPhase.Answering&&!paused&&!showHelp;
            if(Button(new Rect(48,768,114,34),"R · Atacar",false,enabled&&attackCooldown<=0))Attack();
            if(Button(new Rect(172,768,114,34),"Q · Esquivar",false,enabled&&dashCooldown<=0))PerformDodge();
            if(phase!=JourneyPhase.Answering)return;
            Box(new Rect(uiW/2-290,102,580,38),ink);
            Label(new Rect(uiW/2-280,107,560,28),"DESCIFRA + SOBREVIVE   ·   ROJO = ATAQUE INMINENTE",14,amber,TextAnchor.MiddleCenter);
            foreach(var e in enemies)
            {
                Vector3 p=view.WorldToScreenPoint(e.Visual.Root.position+Vector3.up*1.65f);
                if(p.z<0)continue;
                Rect r=new Rect(p.x/uiScale-25,(Screen.height-p.y)/uiScale-8,50,7);
                Box(r,ink);Box(new Rect(r.x,r.y,50f*e.Health/Difficulty.EnemyHealth,7),e.Windup>0?amber:mint);
            }
        }
        void DefeatUI()
        {
            float x=uiW/2-360,y=uiH/2-235;
            Box(new Rect(x,y,720,470),ink);Box(new Rect(x,y,720,3),amber);
            Label(new Rect(x+35,y+26,650,28),"RECORRIDO REINICIADO",15,amber,TextAnchor.MiddleCenter);
            Label(new Rect(x+30,y+82,660,70),"De vuelta al primer santuario",36,paper,TextAnchor.MiddleCenter,true);
            Label(new Rect(x+40,y+170,640,75),defeatReason,21,paper,TextAnchor.MiddleCenter);
            Label(new Rect(x+40,y+253,640,57),"No hay reintento del mismo código.\nConservas niveles, talentos, aspectos, memorias y récords.",17,dim,TextAnchor.MiddleCenter);
            if(Button(new Rect(x+40,y+337,640,49),"Recomenzar desde el inicio",true,!paused&&!showHelp))RestartFailedRun();
            if(Button(new Rect(x+40,y+401,640,40),"Volver al menú y elegir dificultad",false,!paused&&!showHelp))ReturnHome();
        }
        void PerformDodge()
        {
            if(paused||showHelp||panel!=0||dashCooldown>0)return;
            bool allowed=phase==JourneyPhase.Answering||phase==JourneyPhase.Watching||phase==JourneyPhase.Briefing||phase==JourneyPhase.Success||phase==JourneyPhase.Race&&race!=null&&!race.Finished&&raceCountdown<=0;
            if(!allowed)return;
            dashTime=0.25f;dashCooldown=3.5f-(expedition!=null&&expedition.Daily?0:progress.stride*0.65f);
            if(phase==JourneyPhase.Answering&&combat!=null){combat.Dodge();dashCooldown=1.8f;}
            progress.dashes++;CheckAchievements();SaveProgress();
        }
    }
}
