using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        StarRace race;
        Transform raceBeacon;
        bool raceSettled;
        float raceCountdown;
        void StartRace(int biome)
        {
            if(phase!=JourneyPhase.Home||biome<0||biome>2)return;
            SaveJourney();CancelGeneration();expedition=null;race=new StarRace(biome);
            raceSettled=false;raceCountdown=3;panel=0;trial=null;phase=JourneyPhase.Race;paused=false;showHelp=false;
            dashTime=0;dashCooldown=0;BuildWorld();raceBeacon=world.CreateRaceBeacon();raceBeacon.position=RaceTarget();
        }
        Vector3 RaceTarget()
        {
            float a=race.TargetIndex*Mathf.PI/4;
            return new Vector3(Mathf.Cos(a)*6.65f,0.1f,Mathf.Sin(a)*6.65f);
        }
        void UpdateRace(float dt)
        {
            if(race==null||race.Finished)return;
            if(raceCountdown>0){raceCountdown=Mathf.Max(0,raceCountdown-dt);return;}
            race.Tick(dt);
            Vector3 distance=world.Luma.position-RaceTarget();distance.y=0;
            if(distance.magnitude<0.85f&&race.Touch(race.TargetIndex))
            {
                Play(notes[race.Checkpoint%4]);world.Pulse(race.Checkpoint%4);
                raceBeacon.position=RaceTarget();
            }
            if(!reducedMotion)raceBeacon.Rotate(0,dt*35,0);
            if(race.Finished&&!raceSettled)
            {
                raceSettled=true;raceBeacon.gameObject.SetActive(false);
                if(race.Won)
                {
                    progress.raceMedals[race.Biome]=Mathf.Max(progress.raceMedals[race.Biome],race.Medal);
                    if(progress.raceBest[race.Biome]<=0||race.Elapsed<progress.raceBest[race.Biome])progress.raceBest[race.Biome]=race.Elapsed;
                    progress.Reward(60,race.Medal*10);world.Celebrate();Play(winSound);
                }
                SaveJourney();
            }
        }
        void RaceUI()
        {
            Box(new Rect(30,112,272,389),ink);
            Label(new Rect(50,134,232,24),"CARRERA DE LUCIÉRNAGAS",11,mint);
            Label(new Rect(49,174,232,72),WorldNames[race.Biome],29,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(50,277,232,40),race.Checkpoint+" / "+StarRace.Total+" BALIZAS",18,mint);
            Label(new Rect(49,337,232,60),Mathf.Max(0,StarRace.Limit-race.Elapsed).ToString("0.0")+" s",38,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(50,430,232,43),"ORO ≤ 26 s   /   PLATA ≤ 42 s\nBRONCE · completar",12,dim);
            float x=uiW/2-350,y=uiH-220;
            Box(new Rect(x,y,700,182),ink);
            if(!race.Finished)
            {
                Label(new Rect(x+25,y+18,650,43),raceCountdown>0?"Prepárate… "+Mathf.CeilToInt(raceCountdown):"Alcanza la siguiente baliza dorada",28,paper,TextAnchor.MiddleCenter,true);
                Label(new Rect(x+25,y+82,650,56),"WASD · caminar   /   Mayús · correr   /   Q · impulso\nMantén C para señalar la baliza. Recoge las doce antes de que termine el tiempo.",17,dim,TextAnchor.MiddleCenter);
            }
            else
            {
                string medal=race.Medal==3?"ORO":race.Medal==2?"PLATA":"BRONCE";
                Label(new Rect(x+25,y+18,650,43),race.Won?medal+" · "+race.Elapsed.ToString("0.00")+" segundos":"El tiempo terminó. Puedes volver a intentarlo.",26,paper,TextAnchor.MiddleCenter,true);
                if(Button(new Rect(x+25,y+87,650,51),"Volver al atlas",true,!paused&&!showHelp)){ReturnHome();panel=1;}
            }
        }
    }
}
