using System;
using EcosDeNebula;

static class PatternDirectorTests
{
    static int checks;
    static void Check(bool ok, string label) { checks++; if (!ok) throw new Exception(label); }

    static void Main()
    {
        var profile = new LearningProfile();
        var ai = new PatternDirector(profile, 42);
        Check(ai.Create(0, 0).Answer.Length == 3, "Beginner gets three notes");
        for (int world = 0; world < 3; world++)
        for (int run = 0; run < 600; run++)
        {
            var trial = ai.Create(world, run % 3);
            Check(trial.BeatSeconds >= 0.62f && trial.BeatSeconds <= 1.22f, "Tempo bounds");
            foreach (int rune in trial.Demonstration) Check(rune >= 0 && rune < 4, "Legal rune");
            if (world == 0)
                for (int i = 0; i < trial.Answer.Length; i++) Check(trial.Answer[i] == trial.Demonstration[i], "Echo");
            if (world == 1)
                for (int i = 0; i < trial.Answer.Length; i++) Check(trial.Answer[i] == trial.Demonstration[trial.Answer.Length - 1 - i], "Mirror");
            if (world == 2)
            {
                Check(trial.Demonstration.Length == trial.Answer.Length * 2, "Observable period");
                Check(trial.Answer[0] != trial.Answer[1], "Nontrivial motif");
                for (int i = 0; i < trial.Demonstration.Length; i++) Check(trial.Demonstration[i] == trial.Answer[i % trial.Answer.Length], "Prediction");
            }
            ai.Finish(run % 4 != 0, false);
            Check(profile.skill >= 0 && profile.skill <= 1, "Skill bounds");
        }
        var attempt = new PatternAttempt(new[] { 0, 2, 1 });
        Check(attempt.Submit(0) && !attempt.Complete, "Advance");
        Check(!attempt.Submit(3) && attempt.Failed, "Wrong note fails");
        Check(!attempt.Submit(2) && attempt.Position == 1, "No input after failure");
        attempt = new PatternAttempt(new[] { 3 });
        Check(attempt.Submit(3) && attempt.Complete && attempt.Expected == -1, "Complete");
        Check(!attempt.Submit(3), "No double completion");
        var beginner = new PatternDirector(new LearningProfile(), 11);
        float before = beginner.Profile.skill;
        beginner.Finish(false, false);
        Check(beginner.Profile.skill < before, "Failure lowers difficulty");
        beginner.Finish(true, true);
        Check(beginner.Profile.streak == 0, "Replay is assisted");
        var learning = new PatternDirector(new LearningProfile(), 10);
        for (int i = 0; i < 100; i++)
        {
            learning.ObserveSymbol(2, false, 2);
            learning.ObserveSymbol(0, true, 1);
            learning.ObserveSymbol(1, true, 1);
            learning.ObserveSymbol(3, true, 1);
        }
        Check(learning.FocusRune == 2, "Learns weakest rune");
        int[] counts = new int[4];
        for (int i = 0; i < 1500; i++) foreach (int rune in learning.Create(0, 0).Answer) counts[rune]++;
        Check(counts[2] > counts[0] * 1.5 && counts[2] > counts[1] * 1.5, "Generation responds to learning");
        var corrupt = new LearningProfile { seen = null, missed = new[] { 10 }, skill = float.NaN, responseSeconds = float.NaN, wins = -2 };
        corrupt.Sanitize();
        Check(corrupt.seen.Length == 4 && corrupt.missed.Length == 4 && corrupt.skill == 0.25f, "Save repair");
        for(int world=0;world<3;world++)
        {
            var trial=new PatternDirector(new LearningProfile(),7).Create(world,0);
            var generated=new GeneratedPattern{provider="openai",model="test-model",title="La danza",message="Escucha el cielo.",hint="Sigue la regla del santuario.",constellation="spiral",motif=world==2?new[]{1,3}:new[]{1,3,0}};
            Check(generated.TryApply(trial)&&trial.GeneratedByApi,"Accept valid generation");
            Check(trial.Answer[0]==(world==1?0:1),"Derive answer locally, including mirror");
            if(world==2)Check(trial.Demonstration.Length==4&&trial.Demonstration[2]==1,"Prediction remains solvable");
            var answer=(int[])trial.Answer.Clone();
            generated.motif=new[]{9,9,9};
            Check(!generated.TryApply(trial)&&trial.Answer[0]==answer[0],"Reject malformed remote data without mutation");
            generated.motif=world==2?new[]{2,2}:new[]{2,2,2};
            Check(!generated.TryApply(trial),"Reject degenerate remote sequence");
        }
        Console.WriteLine("PASS: " + checks + " checks; 1800 generated trials, all modes, terminal states, adaptive learning and damaged-save repair.");
    }
}
