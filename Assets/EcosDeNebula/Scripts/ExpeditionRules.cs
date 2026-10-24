using System;

namespace EcosDeNebula
{
    public enum EchoRule { Copy, Step, Reverse, ReverseStep, Repeat, Opposite }
    public enum Blessing { Shelter, Patience, Fortune }

    // Pure deterministic rules, independent from Unity, API access and the campaign save.
    public sealed class ExpeditionRun
    {
        public readonly int Seed;
        public readonly bool Daily;
        public int Round { get; private set; }
        public int Hearts { get; private set; }
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int Shields { get; private set; }
        public int Fortune { get; private set; }
        public int Patience { get; private set; }
        public bool AwaitingBlessing { get; private set; }
        public bool Finished { get { return Round >= 6 || Hearts <= 0; } }
        public bool Won { get { return Round >= 6; } }
        public int Biome { get { return Math.Min(2, Round / 2); } }
        public bool Boss { get { return Round % 2 == 1 && !Finished; } }
        public EchoRule Rule { get { return (EchoRule)Math.Min(Round, 5); } }
        public static readonly string[] Guardians = { "Cérvix, raíz de cristal", "Nácar, reloj del mar", "Véspera, ojo del cielo" };
        public static readonly string[] Rules = {
            "Repite la secuencia en el mismo orden.",
            "Sube cada runa un lugar: 1→2, 2→3, 3→4, 4→1.",
            "Devuelve la secuencia desde el final al principio.",
            "Invierte el orden y luego sube cada runa un lugar.",
            "Observa dos bloques iguales y completa el tercero.",
            "Cambia cada runa por su opuesta: 1↔4 y 2↔3. Conserva el orden."
        };

        public ExpeditionRun(int seed, bool daily, int heartBonus, int shieldBonus)
        {
            Seed = seed; Daily = daily;
            Hearts = 3 + (daily ? 0 : Math.Max(0, Math.Min(2, heartBonus)));
            Shields = daily ? 0 : Math.Max(0, Math.Min(2, shieldBonus));
        }

        public PatternTrial CreateTrial()
        {
            if (Finished || AwaitingBlessing) throw new InvalidOperationException("No active encounter");
            var rng = new Random(unchecked(Seed * 397 ^ Round * 7919));
            int length = Round == 4 ? 3 : 3 + Round / 2;
            int[] motif = new int[length];
            for (int i = 0; i < length; i++) motif[i] = i == 0 ? rng.Next(4) : (motif[i-1] + 1 + rng.Next(3)) % 4;
            int[] demo = new int[Round == 4 ? length * 2 : length];
            for (int i = 0; i < demo.Length; i++) demo[i] = motif[i % length];
            int[] answer = (int[])motif.Clone();
            if (Round == 2 || Round == 3) Array.Reverse(answer);
            for (int i = 0; i < length; i++)
            {
                if (Round == 1 || Round == 3) answer[i] = (answer[i] + 1) % 4;
                if (Round == 5) answer[i] = 3 - answer[i];
            }
            return new PatternTrial {
                Mode = Round == 4 ? PatternMode.Prediction : PatternMode.Echo,
                Demonstration = demo, Answer = answer, MotifLength = length,
                BeatSeconds = 0.9f + Patience * 0.15f,
                Title = Boss ? Guardians[Biome] : "Sendero " + (Biome + 1),
                Intent = Boss ? "El guardián transforma los ecos. Lee su regla antes de responder." : "Un encuentro del archipiélago. Tu campaña permanece a salvo.",
                Hint = Rules[Round], Constellation = Boss ? "spiral" : "wave"
            };
        }

        // A shield absorbs a wrong input without advancing it; a lost heart restarts the encounter.
        public bool AbsorbError()
        {
            if (Finished || AwaitingBlessing) return false;
            Combo = 0;
            if (Shields > 0) { Shields--; return true; }
            Hearts--; return false;
        }

        public int Complete(bool assisted)
        {
            if (Finished || AwaitingBlessing) return 0;
            Combo = assisted ? 0 : Combo + 1;
            int gain = (Boss ? 450 : 250) + Combo * 60 + Fortune * 50;
            if (assisted) gain /= 2;
            Score += gain; Round++; AwaitingBlessing = !Finished;
            return gain;
        }

        public bool Choose(Blessing blessing)
        {
            if (!AwaitingBlessing || Finished || (int)blessing < 0 || (int)blessing > 2) return false;
            if (blessing == Blessing.Shelter) Hearts = Math.Min(5, Hearts + 1);
            if (blessing == Blessing.Patience) Patience = Math.Min(4, Patience + 1);
            if (blessing == Blessing.Fortune) Fortune++;
            AwaitingBlessing = false; return true;
        }

        public static int DailySeed(DateTime utc) { return utc.Year * 10000 + utc.Month * 100 + utc.Day; }
    }

    [Serializable]
    public sealed class NebulaProgress
    {
        public int version = 1;
        public int xp, dust, wins, runs, guardians, achievements, dashes, skin;
        public int vitality, aegis, stride;
        public int dailyDate, dailyBest, dailyRewardDate;
        public int[] raceMedals = new int[3];
        public float[] raceBest = new float[3];
        public int Level { get { return 1 + Math.Min(49, xp / 200); } }
        public void Sanitize()
        {
            xp = Clamp(xp, 0, 9999999); dust = Clamp(dust, 0, 999999);
            wins = Clamp(wins, 0, 999999); runs = Clamp(runs, 0, 999999); dashes = Clamp(dashes, 0, 999999);
            guardians &= 7; achievements &= 4095;
            vitality = Clamp(vitality, 0, 2); aegis = Clamp(aegis, 0, 2); stride = Clamp(stride, 0, 2);
            skin = Clamp(skin, 0, 3); if (!SkinUnlocked(skin)) skin = 0;
            dailyBest = Clamp(dailyBest, 0, 100000);
            if(raceMedals==null||raceMedals.Length!=3)raceMedals=new int[3];
            if(raceBest==null||raceBest.Length!=3)raceBest=new float[3];
            for(int i=0;i<3;i++)
            {
                raceMedals[i]=Clamp(raceMedals[i],0,3);
                if(float.IsNaN(raceBest[i])||float.IsInfinity(raceBest[i])||raceBest[i]<0||raceBest[i]>StarRace.Limit)raceBest[i]=0;
            }
        }
        static int Clamp(int n, int a, int b) { return Math.Max(a, Math.Min(b, n)); }
        public int TalentRank(int id) { return id == 0 ? vitality : id == 1 ? aegis : id == 2 ? stride : 2; }
        public int TalentCost(int id) { return 70 + TalentRank(id) * 100; }
        public bool Buy(int id)
        {
            if (id < 0 || id > 2 || TalentRank(id) >= 2 || dust < TalentCost(id)) return false;
            dust -= TalentCost(id);
            if (id == 0) vitality++; else if (id == 1) aegis++; else stride++;
            return true;
        }
        public void Reward(int experience, int currency)
        {
            xp = (int)Math.Min(9999999L, xp + (long)Math.Max(0, experience));
            dust = (int)Math.Min(999999L, dust + (long)Math.Max(0, currency));
        }
        public bool SkinUnlocked(int id) { return id == 0 || id == 1 && Level >= 3 || id == 2 && guardians != 0 || id == 3 && runs > 0; }
        public bool Unlock(int id)
        {
            if (id < 0 || id >= 12 || (achievements & (1 << id)) != 0) return false;
            achievements |= 1 << id; Reward(30, 15); return true;
        }
        public bool FinishDaily(int date, int score, bool won)
        {
            if (dailyDate != date) { dailyDate = date; dailyBest = 0; }
            dailyBest = Math.Max(dailyBest, score);
            if (!won || dailyRewardDate == date) return false;
            dailyRewardDate = date; Reward(150, 100); return true;
        }
    }
}
