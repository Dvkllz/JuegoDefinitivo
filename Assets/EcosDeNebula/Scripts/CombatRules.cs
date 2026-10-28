using System;

namespace EcosDeNebula
{
    public sealed class CombatDifficulty
    {
        public readonly string Name;
        public readonly int Index, CodeLength, Health, Enemies, EnemyHealth;
        public readonly float Beat, Speed, SpawnSeconds, Windup, ProjectileSpeed;
        CombatDifficulty(int index,string name)
        {
            Index=index;Name=name;CodeLength=3+index;Health=5-index;Enemies=1+index;
            EnemyHealth=1+index/2;Beat=1.05f-index*0.1f;Speed=1.15f+index*0.3f;
            SpawnSeconds=4.8f-index*0.75f;Windup=0.95f-index*0.12f;ProjectileSpeed=3.6f+index*0.8f;
        }
        public static readonly CombatDifficulty[] All={
            new CombatDifficulty(0,"Aprendiz"),new CombatDifficulty(1,"Explorador"),
            new CombatDifficulty(2,"Guardián"),new CombatDifficulty(3,"Maestro"),new CombatDifficulty(4,"Pesadilla")
        };
        public static CombatDifficulty Get(int index){return All[Math.Max(0,Math.Min(4,index))];}
    }

    public sealed class CombatLife
    {
        public int Health {get;private set;}
        public int MaxHealth {get;private set;}
        public int Shields {get;private set;}
        public float Immunity {get;private set;}
        public bool Failed {get;private set;}
        public CombatLife(int health,int shields)
        {MaxHealth=Math.Max(1,Math.Min(7,health));Health=MaxHealth;Shields=Math.Max(0,Math.Min(2,shields));}
        public void Tick(float dt)
        {if(dt>=0&&!float.IsNaN(dt)&&!float.IsInfinity(dt))Immunity=Math.Max(0,Immunity-dt);}
        public void Dodge(){if(!Failed)Immunity=Math.Max(Immunity,0.32f);}
        public bool Hit()
        {
            if(Failed||Immunity>0)return false;
            if(Shields>0)Shields--;else Health--;
            Immunity=1f;if(Health<=0)Failed=true;return true;
        }
        public void FailCode(){Failed=true;}
        public void Heal(){if(!Failed)Health=Math.Min(MaxHealth,Health+1);}
    }
}
