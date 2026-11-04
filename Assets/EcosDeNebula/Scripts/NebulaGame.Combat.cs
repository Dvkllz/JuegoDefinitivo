using System.Collections.Generic;
using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaGame
    {
        const string DifficultyKey="EcosDeNebula.Difficulty.v1";
        int difficulty=1;
        CombatLife combat;
        readonly List<Enemy> enemies=new List<Enemy>();
        readonly List<Shot> shots=new List<Shot>();
        float spawnClock,attackCooldown,strikeTime;
        int spawnSerial,waveKills,runKills;
        bool defeatedExpedition,defeatedDaily;
        string defeatReason="";
        CombatDifficulty Difficulty { get { return CombatDifficulty.Get(expedition!=null&&expedition.Daily?2:difficulty); } }
        sealed class Enemy
        {
            public CombatEnemyVisual Visual;
            public int Health,Serial;
            public float Cooldown,Windup,Stun;
            public Vector3 Aim;
        }
        sealed class Shot {public Transform Visual;public Vector3 Velocity;public float Life;}

        void NewCombatRun()
        {
            ClearCombatWave();
            bool talents=expedition!=null&&!expedition.Daily;
            combat=new CombatLife(Difficulty.Health+(talents?progress.vitality:0),talents?progress.aegis:0);
            runKills=0;dashCooldown=0;dashTime=0;attackCooldown=0;
        }
        void ClearCombatWave()
        {
            foreach(var e in enemies)if(e.Visual.Root!=null){e.Visual.Root.gameObject.SetActive(false);Destroy(e.Visual.Root.gameObject);}
            foreach(var s in shots)if(s.Visual!=null){s.Visual.gameObject.SetActive(false);Destroy(s.Visual.gameObject);}
            enemies.Clear();shots.Clear();strikeTime=0;spawnClock=0;
            if(world!=null)world.ShowStrike(Vector3.zero,0,reducedMotion);
        }
        void StartCombatWave()
        {
            if(combat==null||combat.Failed)NewCombatRun();
            ClearCombatWave();waveKills=0;
            for(int i=0;i<1+Difficulty.Index/2;i++)SpawnEnemy();
            spawnClock=Difficulty.SpawnSeconds;
        }
        void SpawnEnemy()
        {
            if(phase!=JourneyPhase.Answering||enemies.Count>=Difficulty.Enemies)return;
            Vector3 point=Vector3.zero;float farthest=-1;
            for(int i=0;i<8;i++)
            {
                float a=(spawnSerial*137.5f+i*45)*Mathf.Deg2Rad;
                Vector3 candidate=new Vector3(Mathf.Cos(a)*6.85f,0.1f,Mathf.Sin(a)*6.85f);
                bool crowded=false;foreach(var other in enemies)if((candidate-other.Visual.Root.position).sqrMagnitude<2.25f)crowded=true;
                if(crowded)continue;
                float distance=(candidate-world.Luma.position).sqrMagnitude;
                if(distance>farthest){farthest=distance;point=candidate;}
            }
            bool ranged=spawnSerial%2==1;
            enemies.Add(new Enemy{Visual=world.CreateEnemy(point,ranged),Health=Difficulty.EnemyHealth,Serial=spawnSerial++,Cooldown=0.9f});
        }
        void UpdateCombat(float dt)
        {
            if(combat==null)return;
            combat.Tick(dt);attackCooldown=Mathf.Max(0,attackCooldown-dt);strikeTime=Mathf.Max(0,strikeTime-dt);
            world.ShowStrike(world.Luma.position,strikeTime,reducedMotion);
            if(phase!=JourneyPhase.Answering)return;
            if(Input.GetKeyDown(KeyCode.R))Attack();
            spawnClock-=dt;
            if(spawnClock<=0){SpawnEnemy();spawnClock=Difficulty.SpawnSeconds;}
            for(int i=enemies.Count-1;i>=0;i--)
            {
                Enemy e=enemies[i];e.Cooldown=Mathf.Max(0,e.Cooldown-dt);e.Stun=Mathf.Max(0,e.Stun-dt);
                if(e.Stun>0)continue;
                Vector3 delta=world.Luma.position-e.Visual.Root.position;delta.y=0;
                float distance=delta.magnitude;
                if(e.Windup>0)
                {
                    e.Windup-=dt;
                    if(e.Windup<=0)
                    {
                        e.Visual.Warning.gameObject.SetActive(false);e.Cooldown=e.Visual.Ranged?2f:1.2f;
                        if(e.Visual.Ranged)FireShot(e);
                        else if(distance<1.85f){DamagePlayer();if(phase==JourneyPhase.Defeat)return;}
                    }
                    continue;
                }
                float range=e.Visual.Ranged?5.5f:1.65f;
                if(distance<=range&&e.Cooldown<=0)
                {
                    e.Windup=Difficulty.Windup;e.Aim=world.Luma.position;e.Visual.Warning.gameObject.SetActive(true);continue;
                }
                if(distance>range*0.8f)
                {
                    Vector3 direction=delta.normalized;
                    Vector3 old=e.Visual.Root.position;
                    Vector3 next=world.Walkable(old+direction*Difficulty.Speed*dt);
                    if((next-old).sqrMagnitude<0.00003f)
                    {
                        direction=new Vector3(-direction.z,0,direction.x)*(e.Serial%2==0?1:-1);
                        next=world.Walkable(old+direction*Difficulty.Speed*dt);
                    }
                    next.y=0.1f;e.Visual.Root.position=next;
                }
                if(delta.sqrMagnitude>0.001f)e.Visual.Root.rotation=Quaternion.LookRotation(delta);
            }
            for(int i=shots.Count-1;i>=0;i--)
            {
                Shot s=shots[i];Vector3 old=s.Visual.position,next=old+s.Velocity*dt;
                s.Visual.position=next;s.Life-=dt;
                Vector3 a=new Vector3(old.x,0,old.z),b=new Vector3(next.x,0,next.z),p=world.Luma.position;p.y=0;
                Vector3 segment=b-a;
                float t=segment.sqrMagnitude<0.0001f?0:Mathf.Clamp01(Vector3.Dot(p-a,segment)/segment.sqrMagnitude);
                bool hit=(p-(a+segment*t)).sqrMagnitude<0.55f*0.55f;
                if(hit||s.Life<=0||next.magnitude>20)
                {
                    Destroy(s.Visual.gameObject);shots.RemoveAt(i);
                    if(hit){DamagePlayer();if(phase==JourneyPhase.Defeat)return;}
                }
            }
        }
        void FireShot(Enemy enemy)
        {
            if(shots.Count>=24)return;
            Vector3 from=enemy.Visual.Root.position+Vector3.up*0.85f;
            Vector3 direction=enemy.Aim-enemy.Visual.Root.position;direction.y=0;
            if(direction.sqrMagnitude<0.001f)direction=Vector3.forward;
            shots.Add(new Shot{Visual=world.CreateProjectile(from),Velocity=direction.normalized*Difficulty.ProjectileSpeed,Life=5});
        }
        void Attack()
        {
            if(phase!=JourneyPhase.Answering||paused||showHelp||panel!=0||attackCooldown>0)return;
            attackCooldown=0.48f;strikeTime=0.25f;Play(notes[2]);
            for(int i=enemies.Count-1;i>=0;i--)
            {
                Enemy e=enemies[i];Vector3 delta=e.Visual.Root.position-world.Luma.position;delta.y=0;
                if(delta.magnitude>3f)continue;
                e.Health--;e.Stun=0.35f;e.Windup=0;e.Visual.Warning.gameObject.SetActive(false);
                e.Visual.Root.position=world.Walkable(e.Visual.Root.position+delta.normalized*0.65f);
                if(e.Health<=0){Destroy(e.Visual.Root.gameObject);enemies.RemoveAt(i);waveKills++;runKills++;}
            }
            // The staff wave also intercepts nearby projectiles.
            for(int i=shots.Count-1;i>=0;i--)
                if(Vector3.Distance(shots[i].Visual.position,world.Luma.position)<3.2f)
                {Destroy(shots[i].Visual.gameObject);shots.RemoveAt(i);}
        }
        void DamagePlayer()
        {
            if(phase!=JourneyPhase.Answering||paused||showHelp||panel!=0||combat==null||!combat.Hit())return;
            flash=0.45f;Play(errorSound);
            if(combat.Failed)FailRun("Te quedaste sin vida frente a los enemigos.");
        }
        void WinCombatWave()
        {
            if(expedition==null||!expedition.Daily)progress.Reward(Mathf.Min(waveKills,5)*8,Mathf.Min(waveKills,5)*2);
            ClearCombatWave();
        }
        void FailRun(string reason)
        {
            if(phase==JourneyPhase.Defeat)return;
            defeatReason=reason;defeatedExpedition=expedition!=null;defeatedDaily=expedition!=null&&expedition.Daily;
            if(combat!=null)combat.FailCode();CancelGeneration();ClearCombatWave();
            if(expedition!=null)
            {
                expedition.Fail();
                if(expedition.Daily)progress.FinishDaily(dailyRunDate,expedition.Score,false);
                expedition=null;expeditionSettled=true;
            }
            else
            {
                save.best=Mathf.Max(save.best,save.score);save.world=0;save.seals=0;save.score=0;save.completed=false;
                director.Profile.streak=0;
            }
            endless=false;occupiedRune=-1;dashTime=0;focusTime=0;trial=null;showHint=false;
            phase=JourneyPhase.Defeat;phaseClock=0;SaveJourney();BuildWorld();
        }
        void RestartFailedRun()
        {
            if(phase!=JourneyPhase.Defeat)return;
            phase=JourneyPhase.Home;
            if(defeatedExpedition)StartExpedition(defeatedDaily);else StartJourney(false);
        }
    }
}
