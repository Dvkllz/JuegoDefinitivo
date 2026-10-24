using System;
using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        const string ProgressKey="EcosDeNebula.Progress.v1";
        NebulaProgress progress;
        ExpeditionRun expedition;
        int panel, journalPage;
        float dashCooldown, dashTime, focusTime;
        bool expeditionSettled;
        int dailyRunDate;
        string CurrentRule { get { return expedition==null?WorldRules[save.world]:trial.Hint; } }
        static readonly string[] AchievementNames={"Primer despertar","Aprendiz de ecos","Tejedora constante","Sin una grieta","Coleccionista","El archivo completo","Vigía del bosque","Relojera del mar","Cartógrafa celeste","El archipiélago unido","Viajera del viento","Afinidad estelar"};
        static readonly string[] AchievementHints={"Resuelve un reto.","Resuelve 5 retos.","Resuelve 20 retos.","Consigue 3 sellos perfectos.","Encuentra 4 memorias.","Encuentra las 12 memorias.","Vence a Cérvix.","Vence a Nácar.","Vence a Véspera.","Completa una expedición.","Usa el impulso 20 veces.","Alcanza el nivel 5."};

        void LoadProgress()
        {
            try { progress=JsonUtility.FromJson<NebulaProgress>(PlayerPrefs.GetString(ProgressKey,"")); }
            catch(Exception){progress=null;}
            if(progress==null||progress.version!=1)progress=new NebulaProgress();
            progress.Sanitize();
        }
        void SaveProgress()
        {
            if(progress==null)return;
            progress.Sanitize();PlayerPrefs.SetString(ProgressKey,JsonUtility.ToJson(progress));
        }
        static int BitCount(int n){int count=0;while(n!=0){count+=n&1;n>>=1;}return count;}
        void CheckAchievements()
        {
            bool[] unlocked={progress.wins>=1,progress.wins>=5,progress.wins>=20,save.perfectSeals>=3,
                BitCount(save.fragments)>=4,BitCount(save.fragments)>=12,(progress.guardians&1)!=0,
                (progress.guardians&2)!=0,(progress.guardians&4)!=0,progress.runs>=1,progress.dashes>=20,progress.Level>=5};
            for(int i=0;i<unlocked.Length;i++)
                if(unlocked[i]&&progress.Unlock(i)){toast="Logro: "+AchievementNames[i]+" · +15 esencia";toastTime=4;}
        }
        void RewardCampaignWin(){progress.wins++;progress.Reward(80,assisted?10:20);CheckAchievements();SaveJourney();}
        void RewardFragment(){progress.Reward(35,10);CheckAchievements();SaveJourney();}

        void StartExpedition(bool daily)
        {
            if(phase!=JourneyPhase.Home)return;
            CancelGeneration();SaveJourney();panel=0;paused=false;showHelp=false;endless=false;
            dailyRunDate=ExpeditionRun.DailySeed(DateTime.UtcNow);
            expedition=new ExpeditionRun(daily?dailyRunDate:Environment.TickCount,daily,progress.vitality,progress.aegis);
            expeditionSettled=false;trial=null;showHint=false;BuildWorld();PrepareExpeditionBriefing();
        }
        void PrepareExpeditionBriefing()
        {
            trial=expedition.CreateTrial();phase=JourneyPhase.Briefing;phaseClock=0;
            world.SetGuardian(expedition.Boss);world.SetConstellation(trial.Constellation);
        }
        void BeginExpeditionRound()
        {
            if(expedition.Finished||expedition.AwaitingBlessing)return;
            showHint=false;focusTime=0;assisted=false;lastWrong=-1;
            trial=expedition.CreateTrial();world.SetGuardian(expedition.Boss);PlaySequence();
        }
        void SubmitExpedition(int rune)
        {
            if(attempt.Complete||attempt.Failed)return;
            int expected=attempt.Expected;
            world.Pulse(rune);Play(notes[rune]);
            if(rune!=expected)
            {
                lastWrong=expected;assisted=true;flash=0.3f;Play(errorSound);
                if(expedition.AbsorbError())
                {toast="Tu escudo absorbe el error. Responde la misma posición.";toastTime=4;return;}
                attempt.Submit(rune);
                if(expedition.Finished){SettleExpedition();phase=JourneyPhase.Ending;}
                else phase=JourneyPhase.Retry;
                phaseClock=0;return;
            }
            attempt.Submit(rune);
            world.SetGuardian(expedition.Boss,1-(float)attempt.Position/trial.Answer.Length);
            if(!attempt.Complete)return;
            int bossWorld=expedition.Biome;bool wasBoss=expedition.Boss;
            scoreGain=expedition.Complete(assisted);progress.wins++;
            // Daily rewards are settled only once on completion. Repeat practice remains free.
            if(!expedition.Daily)progress.Reward(wasBoss?120:70,wasBoss?35:20);
            if(wasBoss)progress.guardians|=1<<bossWorld;
            CheckAchievements();SaveJourney();world.Celebrate();Play(winSound);
            phase=JourneyPhase.Success;phaseClock=0;
            if(expedition.Finished)SettleExpedition();
        }
        void AdvanceExpedition()
        {
            if(expedition.Finished){SettleExpedition();phase=JourneyPhase.Ending;}
            else phase=JourneyPhase.Blessing;
        }
        void ChooseBlessing(int choice)
        {
            if(expedition==null||!expedition.Choose((Blessing)choice))return;
            BuildWorld();PrepareExpeditionBriefing();
        }
        void SettleExpedition()
        {
            if(expeditionSettled)return;expeditionSettled=true;
            if(expedition.Daily)progress.FinishDaily(dailyRunDate,expedition.Score,expedition.Won);
            else if(expedition.Won)progress.Reward(180,80);
            if(expedition.Won)progress.runs++;
            CheckAchievements();SaveJourney();
        }
        void ReturnHome()
        {
            CancelGeneration();if(expedition!=null)SettleExpedition();
            SaveJourney();expedition=null;race=null;phase=JourneyPhase.Home;paused=false;showHelp=false;endless=false;panel=0;
            trial=null;dashTime=0;BuildWorld();
        }
        void UpdateExpansion(float dt)
        {
            dashCooldown=Mathf.Max(0,dashCooldown-dt);dashTime=Mathf.Max(0,dashTime-dt);focusTime=Mathf.Max(0,focusTime-dt);
            bool canMove=phase==JourneyPhase.Briefing||phase==JourneyPhase.Watching||phase==JourneyPhase.Answering||phase==JourneyPhase.Retry||phase==JourneyPhase.Success||phase==JourneyPhase.Race&&race!=null&&!race.Finished&&raceCountdown<=0;
            if(canMove&&Input.GetKeyDown(KeyCode.Q)&&dashCooldown<=0)
            {
                dashTime=0.25f;dashCooldown=3.5f-(expedition!=null&&expedition.Daily?0:progress.stride*0.65f);
                progress.dashes++;CheckAchievements();SaveProgress();
            }
            world.DashEffect(dashTime>0);
            if(phase==JourneyPhase.Answering&&Input.GetKeyDown(KeyCode.F))RevealNextRune();
            if(focusTime>0&&phase==JourneyPhase.Answering&&attempt.Expected>=0)world.Pulse(attempt.Expected);
            float closest=float.MaxValue;Vector3 target=Vector3.zero;bool found=false;
            for(int i=0;i<4;i++)if((save.fragments&(1<<(world.Biome*4+i)))==0)
            {
                float distance=(world.Fragments[i].position-world.Luma.position).sqrMagnitude;
                if(distance<closest){closest=distance;target=world.Fragments[i].position;found=true;}
            }
            if(race!=null)world.PointCompass(RaceTarget(),!race.Finished&&Input.GetKey(KeyCode.C));
            else world.PointCompass(target,canMove&&Input.GetKey(KeyCode.C)&&found);
        }
        void RevealNextRune()
        {
            if(phase!=JourneyPhase.Answering||attempt.Expected<0)return;
            assisted=true;focusTime=1.5f;
            toast="Concentración · siguiente runa: "+(attempt.Expected+1)+" · recompensa con ayuda";toastTime=2;
        }
    }
}
