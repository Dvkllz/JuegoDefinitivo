using UnityEditor;
using UnityEngine;

namespace EcosDeNebula.Editor
{
    public static partial class NebulaSmokeTest
    {
        static string campaignSnapshot;
        public static void Expansion()
        {
            SessionState.SetBool("Nebula.Smoke.Expansion",true);Run();
        }
        static void ExpansionTick(double now)
        {
            JourneyPhase phase=Get<JourneyPhase>("phase");
            if(stage==0)
            {
                Set("progress",new NebulaProgress{aegis=1,vitality=1});
                var save=Get<NebulaSave>("save");save.world=1;save.seals=2;save.score=600;save.fragments=0;save.completed=false;
                campaignSnapshot=JsonUtility.ToJson(save);
                Call("StartExpedition",false);
                Check(Get<NebulaWorld>("world").Biome==0,"Expedition starts in Jade");
                Check(Get<ExpeditionRun>("expedition").Hearts==4,"Vitality adds a heart");
                Set("panel",1);frozenClock=Get<float>("phaseClock");pauseUntil=now+0.6;stage=1;
            }
            else if(stage==1&&now>=pauseUntil)
            {
                Check(Get<float>("phaseClock")==frozenClock,"Atlas pauses gameplay");
                Set("panel",0);Call("BeginTrial");stage=2;
            }
            else if(stage==2&&phase==JourneyPhase.Answering)
            {
                int expected=Get<PatternAttempt>("attempt").Expected;
                Call("Submit",(expected+1)%4);
                Check(Get<ExpeditionRun>("expedition").Hearts==4,"Shield prevents heart loss");
                Check(Get<PatternAttempt>("attempt").Position==0&&!Get<PatternAttempt>("attempt").Failed,"Shield preserves input state");
                Check(Get<ExpeditionRun>("expedition").Shields==0,"Shield consumed once");stage=3;
            }
            else if(stage==3)
            {
                if(phase==JourneyPhase.Answering)Call("Submit",Get<PatternAttempt>("attempt").Expected);
                else if(phase==JourneyPhase.Success)
                {
                    completions++;Call("Advance");
                    Check(Get<NebulaSave>("save").world==1&&Get<NebulaSave>("save").seals==2&&Get<NebulaSave>("save").score==600,"Campaign checkpoint isolated");
                }
                else if(phase==JourneyPhase.Blessing)
                {
                    Call("ChooseBlessing",1);
                    Check(Get<JourneyPhase>("phase")==JourneyPhase.Briefing,"Blessing opens next briefing");
                    Check(Get<NebulaWorld>("world").Guardian.gameObject.activeSelf==Get<ExpeditionRun>("expedition").Boss,"Guardian visibility matches encounter");
                    Call("BeginTrial");
                }
                else if(phase==JourneyPhase.Ending)
                {
                    Check(completions==6&&Get<ExpeditionRun>("expedition").Won,"Six wins finish expedition");
                    Check(Get<NebulaProgress>("progress").runs==1&&Get<NebulaProgress>("progress").guardians==7,"All guardian progression awarded");
                    Call("ReturnHome");Check(Get<NebulaWorld>("world").Biome==1,"Campaign biome restored");
                    Call("StartExpedition",true);
                    Check(Get<ExpeditionRun>("expedition").Hearts==3&&Get<ExpeditionRun>("expedition").Shields==0,"Daily ignores talents");
                    stage=4;
                }
            }
            else if(stage==4)
            {
                Call("BeginTrial");Set("phase",JourneyPhase.Answering);Set("lastInput",-10f);
                Call("Submit",(Get<PatternAttempt>("attempt").Expected+1)%4);
                if(Get<ExpeditionRun>("expedition").Finished)
                {
                    Check(Get<JourneyPhase>("phase")==JourneyPhase.Ending,"Daily defeat summary");
                    Check(Get<NebulaProgress>("progress").dailyRewardDate==0,"No daily reward for failure");
                    Call("ReturnHome");
                    var world=Get<NebulaWorld>("world");
                    world.Luma.position=world.Fragments[0].position;Call("MoveLuma",0f);
                    Check((Get<NebulaSave>("save").fragments&(1<<4))!=0,"Collectible uses rendered biome");
                    var progress=Get<NebulaProgress>("progress");progress.skin=3;world.SetCloak(3);Call("SaveJourney");Call("LoadProgress");
                    Check(Get<NebulaProgress>("progress").skin==3,"Wardrobe persists");
                    Check(Get<NebulaProgress>("progress").dust>0,"Earned currency persists");
                    for(int biome=0;biome<3;biome++)
                    {
                        Call("ReturnHome");Call("StartRace",biome);
                        Check(Get<NebulaWorld>("world").Biome==biome,"Race selects its own biome");
                        Call("UpdateRace",1f);Check(Get<StarRace>("race").Elapsed==0,"Countdown does not advance race clock");
                        Set("raceCountdown",0f);
                        for(int checkpoint=0;checkpoint<StarRace.Total;checkpoint++)
                        {
                            Get<NebulaWorld>("world").Luma.position=Get<Transform>("raceBeacon").position;Call("UpdateRace",0.5f);
                        }
                        Check(Get<StarRace>("race").Won,"Twelve spatial checkpoints complete race");
                        Check(Get<NebulaProgress>("progress").raceMedals[biome]==3,"Race medal persists");
                        int currency=Get<NebulaProgress>("progress").dust;Call("UpdateRace",1f);
                        Check(currency==Get<NebulaProgress>("progress").dust,"Race reward cannot duplicate");
                    }
                    Call("ReturnHome");Check(Get<NebulaSave>("save").world==1&&Get<NebulaSave>("save").seals==2,"Races preserve campaign checkpoint");
                    Finish(null);
                }
            }
        }
    }
}
