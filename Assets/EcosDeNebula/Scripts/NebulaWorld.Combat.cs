using UnityEngine;

namespace EcosDeNebula
{
    public sealed class CombatEnemyVisual
    {
        public Transform Root, Warning;
        public bool Ranged;
    }
    public sealed partial class NebulaWorld
    {
        Material enemyBody, enemyEye, enemyWarning, shotMaterial, strikeMaterial;
        Transform strikeRing;
        void CombatMaterials()
        {
            if(enemyBody!=null)return;
            enemyBody=Mat(new Color(0.14f,0.055f,0.21f));
            enemyEye=Mat(new Color(1f,0.3f,0.18f),2);
            enemyWarning=Mat(new Color(1f,0.17f,0.15f),1.8f);
            shotMaterial=Mat(new Color(1f,0.65f,0.14f),2);
            strikeMaterial=Mat(new Color(0.55f,0.95f,1),2);
        }
        public CombatEnemyVisual CreateEnemy(Vector3 position,bool ranged)
        {
            CombatMaterials();
            Transform root=new GameObject(ranged?"Centinela · tirador":"Sombra · perseguidora").transform;
            root.SetParent(transform,false);root.position=position;
            Shape(ranged?PrimitiveType.Sphere:PrimitiveType.Capsule,"Cuerpo",Vector3.up*0.85f,new Vector3(0.65f,ranged?0.65f:0.55f,0.65f),enemyBody,root);
            for(int s=-1;s<=1;s+=2)
            {
                Shape(PrimitiveType.Sphere,"Ojo",new Vector3(s*0.15f,0.95f,0.28f),Vector3.one*0.13f,enemyEye,root);
                var claw=Shape(PrimitiveType.Cube,"Garra",new Vector3(s*0.45f,0.6f,0.15f),new Vector3(0.12f,0.48f,0.13f),ranged?shotMaterial:enemyBody,root);
                claw.transform.localRotation=Quaternion.Euler(20,0,-s*25);
            }
            if(ranged)
            {
                Transform ring=Ring("Corona del centinela",Vector3.up*0.9f,0.58f,0.065f,shotMaterial,root);
                ring.localRotation=Quaternion.Euler(65,0,20);
            }
            Transform warning=Ring("Ataque anunciado",Vector3.up*0.08f,ranged?0.75f:1.8f,0.09f,enemyWarning,root);
            warning.gameObject.SetActive(false);
            return new CombatEnemyVisual{Root=root,Warning=warning,Ranged=ranged};
        }
        public Transform CreateProjectile(Vector3 position)
        {
            CombatMaterials();
            return Shape(PrimitiveType.Sphere,"Proyectil enemigo",position,Vector3.one*0.3f,shotMaterial).transform;
        }
        public void ShowStrike(Vector3 position,float life,bool reduced)
        {
            CombatMaterials();
            if(strikeRing==null)strikeRing=Ring("Onda del bastón",Vector3.zero,1,0.12f,strikeMaterial);
            strikeRing.gameObject.SetActive(life>0);
            if(life<=0)return;
            strikeRing.position=new Vector3(position.x,0.22f,position.z);
            strikeRing.localScale=Vector3.one*(reduced?3:Mathf.Lerp(3,0.5f,life/0.25f));
        }
        public Vector3 Walkable(Vector3 position)
        {
            Vector2 p=Vector2.ClampMagnitude(new Vector2(position.x,position.z),7.4f);
            if(p.magnitude<1.95f)p=(p.sqrMagnitude<0.001f?Vector2.down:p.normalized)*1.95f;
            for(int i=0;i<4;i++)
            {
                Vector2 r=new Vector2(Runes[i].Position.x,Runes[i].Position.z),delta=p-r;
                if(delta.magnitude<1.12f)p=r+(delta.sqrMagnitude<0.001f?Vector2.down:delta.normalized)*1.12f;
            }
            return new Vector3(p.x,position.y,p.y);
        }
        public void ClearRuneLight()
        {
            celebration=0;
            foreach(var rune in Runes){rune.Glow=0;rune.Light.intensity=0.5f;}
        }
        public void CodePulse(int rune)
        {
            ClearRuneLight();Pulse(rune);
        }
    }
}
