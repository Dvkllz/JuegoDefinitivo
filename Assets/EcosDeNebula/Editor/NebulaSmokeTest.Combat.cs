using System.Collections;
using UnityEditor;
using UnityEngine;

namespace EcosDeNebula.Editor
{
    public static partial class NebulaSmokeTest
    {
        static int combatDifficulty;
        static Vector3 pausedEnemy;
        public static void Combat(){SessionState.SetBool("Nebula.Smoke.Combat",true);Run();}
        public static void BuildCombat(){NebulaEditor.BuildWindows();Combat();}
        static IList Enemies {get{return Get<IList>("enemies");}}
        static CombatEnemyVisual Visual(object enemy){return (CombatEnemyVisual)enemy.GetType().GetField("Visual").GetValue(enemy);}
        static void EnemyField(object enemy,string name,object value){enemy.GetType().GetField(name).SetValue(enemy,value);}
        static void CombatTick(double now)
        {
            if(stage==0)
            {
                Set("difficulty",combatDifficulty);Set("progress",new NebulaProgress{xp=600,dust=200,skin=1});
                Call("StartJourney",true);
                var save=Get<NebulaSave>("save");save.world=2;save.seals=2;save.score=600;save.fragments=9;
                Call("BuildWorld");
                var world=Get<NebulaWorld>("world");world.CodePulse(2);world.Animate(0.01f);
                Check(world.Runes[2].Light.intensity>3&&world.Runes[0].Light.intensity<0.1f,"Only the current code crystal is bright");
                world.CodeGap();world.Animate(0.01f);Check(world.Runes[2].Light.intensity<0.1f,"A visible dark gap separates flashes");
                world.ClearRuneLight();world.Animate(0.01f);Check(world.Runes[0].Light.intensity>=0.5f,"Normal lighting restored after code");
                Call("BeginTrial");stage=1;
            }
            else if(stage==1&&Get<JourneyPhase>("phase")==JourneyPhase.Answering)
            {
                var t=Get<PatternTrial>("trial");
                Check(t.Demonstration.Length==combatDifficulty+3&&t.Answer.Length==t.Demonstration.Length,"Visible code matches inputs in all five difficulties");
                Check(Enemies.Count>=1&&Enemies.Count<=combatDifficulty+1,"Enemies spawned at selected cap");
                pausedEnemy=Visual(Enemies[0]).Root.position;Set("paused",true);pauseUntil=now+0.6;stage=2;
                int health=Get<CombatLife>("combat").Health;Call("DamagePlayer");
                Check(Get<CombatLife>("combat").Health==health,"Pause prevents damage");
                EnterRune((Get<PatternAttempt>("attempt").Expected+1)%4);
                Check(Get<JourneyPhase>("phase")==JourneyPhase.Answering,"Pause prevents wrong input");
            }
            else if(stage==2&&now>=pauseUntil)
            {
                Check(Visual(Enemies[0]).Root.position==pausedEnemy,"Pause freezes enemy movement");Set("paused",false);
                var w=Get<NebulaWorld>("world");var cfg=CombatDifficulty.Get(combatDifficulty);
                CheckRuneWalking();
                Call("ClearCombatWave");Set("spawnSerial",0);Call("SpawnEnemy");Set("spawnClock",100f);
                object foe=Enemies[0];Visual(foe).Root.position=w.Luma.position+Vector3.right*1.2f;
                EnemyField(foe,"Cooldown",0f);Call("UpdateCombat",0.01f);
                Check(Visual(foe).Warning.gameObject.activeSelf,"Enemy announces attack before damage");
                int health=Get<CombatLife>("combat").Health;
                Call("UpdateCombat",cfg.Windup-0.1f);Set("dashCooldown",0f);Call("PerformDodge");Call("UpdateCombat",0.11f);
                Check(Get<CombatLife>("combat").Health==health,"Dodge avoids announced melee damage");
                Set("dashTime",0f);Visual(foe).Root.position=w.Luma.position+Vector3.right*0.7f;
                int kills=Get<int>("runKills");
                for(int hit=0;hit<cfg.EnemyHealth;hit++){Set("attackCooldown",0f);Call("Attack");}
                Check(Enemies.Count==0&&Get<int>("runKills")==kills+1,"Staff kills enemies with correct hit count");
                Set("spawnSerial",1);Call("SpawnEnemy");foe=Enemies[0];
                Check(Visual(foe).Ranged,"Second enemy archetype is ranged");
                Visual(foe).Root.position=w.Luma.position+Vector3.right*2;
                EnemyField(foe,"Aim",w.Luma.position);Call("FireShot",foe);
                Check(Get<IList>("shots").Count==1,"Ranged enemy creates projectile");
                Set("attackCooldown",0f);Call("Attack");Check(Get<IList>("shots").Count==0,"Staff intercepts projectile");
                Call("ClearCombatWave");Set("spawnSerial",1);Call("SpawnEnemy");Set("spawnClock",100f);foe=Enemies[0];
                Visual(foe).Root.position=w.Luma.position+Vector3.right*2;EnemyField(foe,"Aim",w.Luma.position);EnemyField(foe,"Cooldown",10f);
                Set("combat",new CombatLife(2,0));Call("FireShot",foe);Call("UpdateCombat",0.8f);
                Check(Get<CombatLife>("combat").Health==1,"Swept projectile collision damages player");
                Call("ClearCombatWave");Set("combat",new CombatLife(7,2));Set("lastInput",-10f);
                EnterRune((Get<PatternAttempt>("attempt").Expected+1)%4);
                Check(Get<JourneyPhase>("phase")==JourneyPhase.Defeat,"A single wrong rune defeats even a shielded player");
                Check(Get<NebulaSave>("save").world==0&&Get<NebulaSave>("save").seals==0&&Get<NebulaSave>("save").score==0,"Failure resets entire campaign");
                Check(Get<NebulaSave>("save").fragments==9&&Get<NebulaProgress>("progress").xp==600,"Failure preserves memories and permanent XP");
                Call("LoadJourney");Check(Get<NebulaSave>("save").world==0&&Get<NebulaSave>("save").seals==0,"Reset persisted to disk");
                Call("RestartFailedRun");Check(Get<JourneyPhase>("phase")==JourneyPhase.Briefing&&Get<CombatLife>("combat").Health==cfg.Health,"Restart begins first sanctuary with full health");
                combatDifficulty++;stage=combatDifficulty<5?0:3;
            }
            else if(stage==3)
            {
                Call("ReturnHome");var save=Get<NebulaSave>("save");save.world=1;save.seals=2;save.score=600;
                Call("StartExpedition",false);Call("BeginTrial");Set("phase",JourneyPhase.Answering);Set("lastInput",-10f);
                EnterRune((Get<PatternAttempt>("attempt").Expected+1)%4);
                Check(Get<JourneyPhase>("phase")==JourneyPhase.Defeat&&Get<ExpeditionRun>("expedition")==null,"Expedition code error terminates run");
                Check(save.world==1&&save.seals==2&&save.score==600,"Expedition defeat does not erase separate campaign");
                Call("RestartFailedRun");Check(Get<ExpeditionRun>("expedition").Round==0,"Expedition restart returns to first encounter");
                Call("ReturnHome");Set("difficulty",4);Call("StartJourney",false);Call("BeginTrial");Set("phase",JourneyPhase.Answering);
                Call("DamagePlayer");Check(Get<JourneyPhase>("phase")==JourneyPhase.Defeat,"Lethal enemy damage also ends run");
                Check(save.world==0&&save.seals==0,"Lethal damage resets campaign checkpoint");
                Finish(null);
            }
        }

        static void CheckRuneWalking()
        {
            var w=Get<NebulaWorld>("world");
            var original=Get<PatternAttempt>("attempt");
            var probe=new PatternAttempt(new[]{0,0,1,2});
            Set("attempt",probe);Set("lastInput",-10f);
            w.Luma.position=new Vector3(0,0.1f,-5);Call("SyncRuneEntry");
            Call("Submit",0);Check(probe.Position==0,"Remote submission is rejected");
            w.Luma.position=w.Runes[0].Position+Vector3.back*1.35f;
            Set("phase",JourneyPhase.Watching);Call("UpdateRuneEntry");
            Check(probe.Position==0,"Walking during demonstration cannot answer");
            Set("phase",JourneyPhase.Answering);Call("SyncRuneEntry");Call("UpdateRuneEntry");
            Check(probe.Position==0,"Standing at a crystal when the turn starts cannot answer");
            EnterRune(0);Check(probe.Position==1,"Entering the reachable ring submits once");
            Set("lastInput",-10f);
            for(int i=0;i<20;i++)Call("UpdateRuneEntry");
            Check(probe.Position==1,"Standing in a ring never repeats the answer");
            w.Luma.position=w.Runes[0].Position+Vector3.back*1.8f;Call("UpdateRuneEntry");
            w.Luma.position=w.Runes[0].Position+Vector3.back*1.5f;Call("UpdateRuneEntry");
            Check(probe.Position==1,"Small movements on the entry edge do not repeat");
            EnterRune(0);Check(probe.Position==2,"Leaving the outer ring enables repeated symbols");
            Set("lastInput",-10f);
            w.Luma.position=new Vector3(0,0.1f,-5);Call("UpdateRuneEntry");
            w.Luma.position=w.Runes[1].Position+Vector3.back*1.35f;
            Set("paused",true);Call("UpdateRuneEntry");Set("paused",false);
            Set("showHelp",true);Call("UpdateRuneEntry");Set("showHelp",false);
            Set("panel",1);Call("UpdateRuneEntry");Set("panel",0);
            Check(probe.Position==2,"Pause, help and Atlas block physical answers");
            Call("UpdateRuneEntry");Check(probe.Position==3,"Another crystal accepts the next symbol");
            Set("attempt",original);Set("lastInput",-10f);
            w.Luma.position=new Vector3(0,0.1f,-5);Call("SyncRuneEntry");
        }
    }
}
