using System;

namespace EcosDeNebula
{
    public sealed class StarRace
    {
        public readonly int Biome;
        public int Checkpoint { get; private set; }
        public float Elapsed { get; private set; }
        public const int Total = 12;
        public const float Limit = 65;
        public bool Won { get { return Checkpoint >= Total; } }
        public bool Finished { get { return Won || Elapsed >= Limit; } }
        public int Medal { get { return !Won ? 0 : Elapsed <= 26 ? 3 : Elapsed <= 42 ? 2 : 1; } }
        public int TargetIndex
        {
            get { return (Checkpoint * (Biome == 0 ? 1 : Biome == 1 ? 3 : 5)) % 8; }
        }
        public StarRace(int biome)
        {
            if(biome<0||biome>2)throw new ArgumentOutOfRangeException("biome");Biome=biome;
        }
        public void Tick(float seconds)
        {
            if(Finished||float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<0)return;
            Elapsed=Math.Min(Limit,Elapsed+seconds);
        }
        public bool Touch(int target)
        {
            if(Finished||target!=TargetIndex)return false;
            Checkpoint++;return true;
        }
    }
}
