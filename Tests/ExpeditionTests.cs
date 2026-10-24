using System;
using EcosDeNebula;
static class ExpeditionTests
{
    static int checks;
    static void Check(bool value, string label) { checks++; if (!value) throw new Exception(label); }
    static void Main()
    {
        for (int seed=0;seed<300;seed++)
        {
            var run = new ExpeditionRun(seed,false,0,0);
            for(int r=0;r<6;r++)
            {
                var t=run.CreateTrial(); var copy=run.CreateTrial();
                for(int i=0;i<t.Answer.Length;i++)
                {
                    int source=t.Demonstration[(r==2||r==3)?t.Answer.Length-1-i:i];
                    int expected=(r==1||r==3)?(source+1)%4:r==5?3-source:source;
                    Check(t.Answer[i]==expected,"Correct transformation");
                    Check(t.Answer[i]==copy.Answer[i],"Deterministic retry");
                }
                Check(run.Complete(false)>0,"Round awards score");
                Check(run.Complete(false)==0,"Cannot award twice");
                if(r<5) { Check(run.Choose((Blessing)(r%3)),"Blessing advances"); Check(!run.Choose(Blessing.Fortune),"No double blessing"); }
            }
            Check(run.Won&&run.Finished,"Six encounters complete");
        }
        var fail=new ExpeditionRun(0,false,0,1);
        Check(fail.AbsorbError()&&fail.Hearts==3,"Shield protects heart");
        for(int i=0;i<3;i++)fail.AbsorbError();
        Check(fail.Finished&&!fail.Won,"Defeat after three errors");
        Check(fail.Complete(false)==0,"Cannot reward defeated run");
        var daily=new ExpeditionRun(20261024,true,2,2);
        Check(daily.Hearts==3&&daily.Shields==0,"Daily ignores purchased advantages");
        Check(ExpeditionRun.DailySeed(new DateTime(2026,10,24))==20261024,"Stable daily seed");
        var p=new NebulaProgress();Check(!p.Buy(0),"Cannot buy without dust");
        p.Reward(500,500);Check(p.Level==3&&p.SkinUnlocked(1),"Level unlocks skin");
        Check(p.Buy(0)&&p.dust==430&&p.vitality==1,"Purchase charged once");
        Check(p.Buy(0)&&!p.Buy(0),"Talent capped");
        Check(!p.Buy(-1)&&!p.Buy(3),"Invalid talent rejected");
        Check(p.Unlock(0)&&!p.Unlock(0),"Achievement paid once");
        Check(!p.FinishDaily(20261024,100,false),"Daily loss no payout");
        Check(p.FinishDaily(20261024,2000,true)&&!p.FinishDaily(20261024,2100,true),"Daily payout once");
        Check(p.dailyBest==2100,"Daily best improves on replays");
        p.xp=int.MaxValue;p.dust=-50;p.stride=999;p.skin=99;p.Sanitize();
        Check(p.xp==9999999&&p.dust==0&&p.stride==2&&p.skin==0,"Corrupt save sanitized");
        for(int biome=0;biome<3;biome++)
        {
            var race=new StarRace(biome);
            for(int i=0;i<StarRace.Total;i++)
            {
                Check(race.TargetIndex>=0&&race.TargetIndex<8,"Legal race target");
                Check(!race.Touch((race.TargetIndex+1)%8),"Wrong beacon rejected");
                race.Tick(2);Check(race.Touch(race.TargetIndex),"Target reached");
            }
            Check(race.Won&&race.Medal==3&&race.Finished,"Gold race completion");
            Check(!race.Touch(race.TargetIndex),"No rewards after finish");
        }
        var expired=new StarRace(0);expired.Tick(float.NaN);expired.Tick(-5);
        Check(expired.Elapsed==0,"Invalid race time ignored");expired.Tick(66);
        Check(expired.Finished&&!expired.Won&&!expired.Touch(0),"Timeout blocks later completion");
        p.raceBest=new[]{float.NaN,-1f,float.PositiveInfinity};p.raceMedals=null;p.Sanitize();
        Check(p.raceMedals.Length==3&&p.raceBest[0]==0&&p.raceBest[1]==0,"Race save repair");
        Console.WriteLine("PASS: "+checks+" expedition and progression checks");
    }
}
