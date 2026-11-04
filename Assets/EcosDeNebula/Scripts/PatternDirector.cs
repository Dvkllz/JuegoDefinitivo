using System;

namespace EcosDeNebula
{
    public enum PatternMode { Echo, Mirror, Prediction }

    [Serializable]
    public sealed class LearningProfile
    {
        public int[] seen = new int[4];
        public int[] missed = new int[4];
        public int trials;
        public int wins;
        public int streak;
        public float skill = 0.25f;
        public float responseSeconds = 1.5f;

        public void Sanitize()
        {
            if (seen == null || seen.Length != 4) seen = new int[4];
            if (missed == null || missed.Length != 4) missed = new int[4];
            for (int i = 0; i < 4; i++)
            {
                seen[i] = Math.Max(0, seen[i]);
                missed[i] = Math.Max(0, Math.Min(seen[i], missed[i]));
            }
            trials = Math.Max(0, trials);
            wins = Math.Max(0, Math.Min(trials, wins));
            streak = Math.Max(0, streak);
            skill = float.IsNaN(skill) ? 0.25f : Clamp(skill, 0, 1);
            responseSeconds = float.IsNaN(responseSeconds) ? 1.5f : Clamp(responseSeconds, 0.2f, 8);
        }

        internal static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public sealed class PatternTrial
    {
        public PatternMode Mode;
        public int[] Demonstration;
        public int[] Answer;
        public int MotifLength;
        public float BeatSeconds;
        public string Intent;
        public string Title = "Un nuevo eco";
        public string Hint = "Escucha la regla de este santuario y agrupa los ecos.";
        public string Constellation = "orbit";
        public string Model = "";
        public bool GeneratedByApi;
        public string TextLanguage = "es";

        // Playback contract: each visible flash always has one input slot, regardless of source.
        public void PrepareCode(int length = 0)
        {
            if(Demonstration==null||Demonstration.Length==0||Answer==null||Answer.Length==0)
                throw new InvalidOperationException("Empty code");
            foreach(int rune in Demonstration)if(rune<0||rune>3)throw new InvalidOperationException("Invalid rune");
            foreach(int rune in Answer)if(rune<0||rune>3)throw new InvalidOperationException("Invalid answer");
            int count=length>0?Math.Max(3,Math.Min(7,length)):Demonstration.Length;
            int[] visible=new int[count],response=new int[count];
            for(int i=0;i<count;i++){visible[i]=Demonstration[i%Demonstration.Length];response[i]=Answer[i%Answer.Length];}
            if(Mode==PatternMode.Prediction)response=(int[])visible.Clone();
            if(Mode==PatternMode.Mirror){response=(int[])visible.Clone();Array.Reverse(response);}
            Demonstration=visible;Answer=response;
        }
    }

    [Serializable]
    public sealed class GeneratedPattern
    {
        public int[] motif;
        public string title, message, hint, constellation, provider, model;

        public bool TryApply(PatternTrial trial)
        {
            int required = trial.Mode == PatternMode.Prediction ? trial.MotifLength : trial.Demonstration.Length;
            if (provider != "openai" || string.IsNullOrEmpty(model) || motif == null || motif.Length != required ||
                !ValidText(title,44) || !ValidText(message,140) || !ValidText(hint,130) ||
                (constellation != "orbit" && constellation != "wave" && constellation != "spiral")) return false;
            bool different = false;
            for(int i=0;i<motif.Length;i++)
            {
                if(motif[i]<0 || motif[i]>3)return false;
                if(motif[i]!=motif[0])different=true;
            }
            if(!different)return false;
            if(trial.Mode==PatternMode.Prediction)
            {
                trial.Demonstration=new int[motif.Length*2];
                for(int i=0;i<trial.Demonstration.Length;i++)trial.Demonstration[i]=motif[i%motif.Length];
            }
            else trial.Demonstration=(int[])motif.Clone();
            trial.Answer=(int[])motif.Clone();
            if(trial.Mode==PatternMode.Mirror)Array.Reverse(trial.Answer);
            trial.Title=title;trial.Intent=message;trial.Hint=hint;trial.Constellation=constellation;
            trial.Model=model;trial.GeneratedByApi=true;return true;
        }
        static bool ValidText(string text,int limit)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Length<=limit && text.IndexOf('<')<0 && text.IndexOf('>')<0;
        }
    }

    // Small, local adaptive tutor: Bayesian error estimates plus an online skill model.
    // No network, external model, fabricated chat responses or API credentials.
    public sealed class PatternDirector
    {
        readonly Random random;
        public LearningProfile Profile { get; private set; }

        public PatternDirector(LearningProfile profile, int seed)
        {
            Profile = profile ?? new LearningProfile();
            Profile.Sanitize();
            random = new Random(seed);
        }

        public float ErrorEstimate(int rune)
        {
            ValidateRune(rune);
            return (Profile.missed[rune] + 1f) / (Profile.seen[rune] + 4f);
        }

        public int FocusRune
        {
            get
            {
                int best = 0;
                for (int i = 1; i < 4; i++) if (ErrorEstimate(i) > ErrorEstimate(best)) best = i;
                return best;
            }
        }

        int ChooseRune(int previous)
        {
            double total = 0;
            double[] weights = new double[4];
            for (int i = 0; i < 4; i++)
            {
                weights[i] = (0.35 + 2.8 * ErrorEstimate(i)) * (i == previous ? 0.38 : 1);
                total += weights[i];
            }
            double draw = random.NextDouble() * total;
            for (int i = 0; i < 4; i++) { draw -= weights[i]; if (draw <= 0) return i; }
            return 3;
        }

        public PatternTrial Create(int world, int seal)
        {
            if (world < 0 || world > 2) throw new ArgumentOutOfRangeException("world");
            if (seal < 0) throw new ArgumentOutOfRangeException("seal");
            PatternTrial trial = new PatternTrial();
            trial.Mode = (PatternMode)world;
            trial.BeatSeconds = LearningProfile.Clamp(0.98f - Profile.skill * 0.32f +
                (Profile.responseSeconds - 1.5f) * 0.06f, 0.62f, 1.22f);
            int length = Math.Max(3, Math.Min(7, 3 + (int)(Profile.skill * 3) + Math.Min(seal, 2) / 2));
            if (world == 2)
            {
                // Two complete repeats make the periodic rule observable and unambiguous.
                int motifLength = Profile.skill > 0.65f ? 3 : 2;
                int[] motif = new int[motifLength];
                motif[0] = ChooseRune(-1);
                for (int i = 1; i < motifLength; i++)
                {
                    motif[i] = ChooseRune(motif[i - 1]);
                    if (motif[i] == motif[i - 1]) motif[i] = (motif[i] + 1 + random.Next(3)) % 4;
                }
                trial.MotifLength = motifLength;
                trial.Demonstration = new int[motifLength * 2];
                for (int i = 0; i < trial.Demonstration.Length; i++) trial.Demonstration[i] = motif[i % motifLength];
                trial.Answer = (int[])trial.Demonstration.Clone();
            }
            else
            {
                trial.Demonstration = new int[length];
                int previous = -1;
                for (int i = 0; i < length; i++) { previous = ChooseRune(previous); trial.Demonstration[i] = previous; }
                trial.Answer = (int[])trial.Demonstration.Clone();
                if (world == 1) Array.Reverse(trial.Answer);
            }
            trial.Intent = Profile.trials == 0 ? "Estoy descubriendo tu ritmo. Empezamos con tres ecos." :
                Profile.streak >= 2 ? "Has encadenado aciertos. Aumentaré el desafío poco a poco." :
                "Reforzaré tu runa menos segura y adaptaré el ritmo a tus respuestas.";
            return trial;
        }

        public void ObserveSymbol(int expected, bool correct, float seconds)
        {
            ValidateRune(expected);
            Profile.seen[expected]++;
            if (!correct) Profile.missed[expected]++;
            float safeSeconds = float.IsNaN(seconds) ? 1.5f : LearningProfile.Clamp(seconds, 0.2f, 8);
            Profile.responseSeconds = Profile.responseSeconds * 0.85f + safeSeconds * 0.15f;
        }

        public void Finish(bool success, bool assisted)
        {
            Profile.trials++;
            if (success)
            {
                Profile.wins++;
                Profile.streak = assisted ? 0 : Profile.streak + 1;
                Profile.skill = LearningProfile.Clamp(Profile.skill + (assisted ? 0.015f : 0.075f), 0, 1);
            }
            else
            {
                Profile.streak = 0;
                Profile.skill = LearningProfile.Clamp(Profile.skill - 0.12f, 0, 1);
            }
        }

        static void ValidateRune(int rune)
        {
            if (rune < 0 || rune > 3) throw new ArgumentOutOfRangeException("rune");
        }
    }

    public sealed class PatternAttempt
    {
        readonly int[] answer;
        public int Position { get; private set; }
        public bool Failed { get; private set; }
        public bool Complete { get { return !Failed && Position == answer.Length; } }
        public int Expected { get { return Complete || Failed ? -1 : answer[Position]; } }

        public PatternAttempt(int[] sequence)
        {
            if (sequence == null || sequence.Length == 0) throw new ArgumentException("Empty answer");
            answer = (int[])sequence.Clone();
        }

        public bool Submit(int rune)
        {
            if (Failed || Complete) return false;
            if (rune != answer[Position]) { Failed = true; return false; }
            Position++;
            return true;
        }
    }
}
