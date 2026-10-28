using System;
using EcosDeNebula;
static class CombatTests
{
    static int checks;
    static void Check(bool pass,string why){checks++;if(!pass)throw new Exception(why);}
    static void Main()
    {
        Check(CombatDifficulty.All.Length==5,"Exactly five difficulties");
        for(int d=0;d<5;d++)
        {
            var cfg=CombatDifficulty.Get(d);
            Check(cfg.CodeLength==d+3&&cfg.Health==5-d&&cfg.Enemies==d+1,"Difficulty scaling");
            for(int seed=0;seed<200;seed++)
            for(int world=0;world<3;world++)
            {
                var t=new PatternDirector(new LearningProfile(),seed).Create(world,0);t.PrepareCode(cfg.CodeLength);
                Check(t.Demonstration.Length==cfg.CodeLength&&t.Answer.Length==cfg.CodeLength,"Flash/input counts match at every difficulty");
                for(int i=0;i<t.Answer.Length;i++)Check(t.Answer[i]==t.Demonstration[world==1?t.Answer.Length-1-i:i],"Code transformations remain correct");
                t.PrepareCode(cfg.CodeLength);Check(t.Answer.Length==cfg.CodeLength,"Replay is idempotent");
            }
            var life=new CombatLife(cfg.Health,1);
            Check(life.Hit()&&life.Health==cfg.Health&&life.Shields==0,"Shield blocks enemy damage");
            Check(!life.Hit(),"No stacked collision damage");life.Tick(2);life.Dodge();
            Check(!life.Hit(),"Dodge grants invulnerability");life.Tick(1);
            for(int i=0;i<cfg.Health;i++){life.Hit();life.Tick(2);}
            Check(life.Failed&&life.Health==0,"Zero health ends run");
            life.Heal();Check(life.Health==0,"Cannot revive failed run");
            var codeLife=new CombatLife(7,2);codeLife.FailCode();
            Check(codeLife.Failed,"Code error ignores health and shields");
            var run=new ExpeditionRun(7,false,2,2);
            for(int round=0;round<6;round++)
            {
                var trial=run.CreateTrial(cfg.CodeLength);trial.PrepareCode();
                Check(trial.Demonstration.Length==cfg.CodeLength&&trial.Answer.Length==cfg.CodeLength,"Expedition uses selected length");
                for(int i=0;i<cfg.CodeLength;i++)
                {
                    int raw=trial.Demonstration[round==2||round==3?cfg.CodeLength-1-i:i];
                    int expected=round==1||round==3?(raw+1)%4:round==5?3-raw:raw;
                    Check(trial.Answer[i]==expected,"Guardian transformation preserved");
                }
                run.Complete(false);if(round<5)run.Choose(Blessing.Patience);
            }
        }
        var legacy=new PatternTrial{Mode=PatternMode.Prediction,Demonstration=new[]{0,2,0,2},Answer=new[]{0,2}};
        legacy.PrepareCode();Check(legacy.Answer.Length==4,"Regression: four flashes need four inputs");
        var a=new PatternAttempt(legacy.Answer);a.Submit(0);a.Submit(2);Check(!a.Complete,"Two inputs cannot finish a four-flash code");a.Submit(0);a.Submit(2);Check(a.Complete,"All four inputs finish");
        Console.WriteLine("PASS: "+checks+" combat and code regression checks");
    }
}
