using UnityEngine;

namespace EcosDeNebula
{
    public sealed partial class NebulaWorld
    {
        public Transform Guardian, Companion, Compass;
        Transform guardianCore;
        Transform[] guardianArms = new Transform[6];
        Transform[] lanterns = new Transform[8];
        Material guardianGlow;
        TrailRenderer dashTrail;
        Vector3 companionVelocity;
        float guardianEnergy = 1;
        public static readonly Color[] CloakColors = {
            new Color(0.26f,0.73f,0.66f), new Color(0.98f,0.59f,0.18f),
            new Color(0.42f,0.65f,1f), new Color(0.83f,0.42f,0.95f)
        };

        void BuildExpansion(Color accent,Material stone,Material gold,Material glow)
        {
            // Outer ruins frame the arena without blocking walkable space or crystal silhouettes.
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=new Vector3(side*8.2f,0,-3.8f);
                Facet("Ruina del vigía",p,new[]{0f,0.4f,2.7f,2.9f},new[]{0.8f,0.6f,0.32f,0.65f},6,stone);
                Ring("Sello del vigía",p+Vector3.up*3.1f,0.55f,0.07f,glow);
                Shape(PrimitiveType.Sphere,"Fuego del vigía",p+Vector3.up*3.2f,Vector3.one*0.24f,glow);
            }
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;
                lanterns[i]=new GameObject("Farol viajero").transform;lanterns[i].SetParent(transform,false);
                lanterns[i].localPosition=new Vector3(Mathf.Cos(a)*9.4f,3.5f,Mathf.Sin(a)*9.4f);
                Facet("Farol",Vector3.zero,new[]{-0.25f,0f,0.3f},new[]{0.15f,0.22f,0.1f},5,gold,lanterns[i]);
                Shape(PrimitiveType.Sphere,"Luz",Vector3.zero,Vector3.one*0.22f,glow,lanterns[i]);
            }
            // A readable boss silhouette: floating mask, horns, articulated orbit and luminous eye.
            Guardian=new GameObject("Guardián del santuario").transform;Guardian.SetParent(transform,false);
            Guardian.localPosition=new Vector3(0,3.7f,5.2f);
            guardianGlow=Mat(Color.Lerp(accent,Color.white,0.25f),1.5f);
            guardianCore=Facet("Máscara ancestral",Vector3.zero,new[]{-1f,-0.4f,0.5f,0.9f},new[]{0f,0.9f,0.7f,0f},Biome+5,stone,Guardian).transform;
            Shape(PrimitiveType.Sphere,"Ojo ancestral",new Vector3(0,0.05f,-0.65f),Vector3.one*0.5f,guardianGlow,Guardian);
            for(int s=-1;s<=1;s+=2)
            {
                Transform horn=Facet("Corona de cristal",new Vector3(s*0.7f,0.45f,0),new[]{0,0.7f,1.5f},new[]{0.24f,0.16f,0},5,gold,Guardian).transform;
                horn.localRotation=Quaternion.Euler(0,0,-s*28);
            }
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3;
                guardianArms[i]=Facet("Fragmento del guardián",new Vector3(Mathf.Cos(a)*1.6f,Mathf.Sin(a)*1.3f,0),new[]{-0.4f,0,0.4f},new[]{0,0.24f,0},4,guardianGlow,Guardian).transform;
            }
            var seal=Ring("Órbita ancestral",Vector3.zero,2,0.055f,gold,Guardian);seal.localRotation=Quaternion.Euler(90,0,0);
            Guardian.gameObject.SetActive(false);
            Companion=new GameObject("Pip · luciérnaga de los recuerdos").transform;Companion.SetParent(transform,false);
            Companion.position=Luma.position+new Vector3(0.8f,1.8f,0.2f);
            Shape(PrimitiveType.Sphere,"Pip",Vector3.zero,Vector3.one*0.22f,glow,Companion);
            for(int s=-1;s<=1;s+=2)
                Shape(PrimitiveType.Sphere,"Ala",new Vector3(s*0.18f,0.05f,0),new Vector3(0.22f,0.06f,0.16f),gold,Companion);
            Compass=new GameObject("Brújula de memorias").transform;Compass.SetParent(transform,false);
            var arrow=Shape(PrimitiveType.Cube,"Dirección",Vector3.forward*0.5f,new Vector3(0.08f,0.04f,0.7f),glow,Compass);
            arrow.transform.localRotation=Quaternion.identity;
            Compass.gameObject.SetActive(false);
            dashTrail=Luma.gameObject.AddComponent<TrailRenderer>();dashTrail.time=0.28f;
            dashTrail.startWidth=0.32f;dashTrail.endWidth=0;dashTrail.sharedMaterial=glow;
            dashTrail.emitting=false;dashTrail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void SetCloak(int skin)
        {
            Transform cape=Luma.Find("Capa");
            if(cape!=null)cape.GetComponent<Renderer>().sharedMaterial.color=CloakColors[Mathf.Clamp(skin,0,3)];
        }
        public Transform CreateRaceBeacon()
        {
            Transform beacon=new GameObject("Baliza de carrera").transform;beacon.SetParent(transform,false);
            Material glow=Mat(new Color(1,0.8f,0.28f),2);
            Ring("Objetivo",Vector3.up*0.08f,0.8f,0.1f,glow,beacon);
            for(int i=0;i<3;i++)
                Facet("Flecha de luz",new Vector3(0,1.4f+i*0.45f,0),new[]{-0.15f,0,0.2f},new[]{0,0.22f,0},4,glow,beacon);
            return beacon;
        }
        public void SetGuardian(bool visible,float energy=1)
        {
            Guardian.gameObject.SetActive(visible);guardianEnergy=Mathf.Clamp01(energy);
        }
        public void DashEffect(bool active) { if(dashTrail!=null)dashTrail.emitting=active&&!ReducedMotion; }
        public void PointCompass(Vector3 target,bool visible)
        {
            Compass.gameObject.SetActive(visible);
            if(!visible)return;
            Compass.position=new Vector3(Luma.position.x,0.2f,Luma.position.z);
            Vector3 delta=target-Compass.position;delta.y=0;
            if(delta.sqrMagnitude>0.01f)Compass.rotation=Quaternion.LookRotation(delta);
        }
        void AnimateExpansion(float dt)
        {
            if(Companion==null)return;
            Vector3 follow=Luma.position+new Vector3(0.85f,1.9f,0.25f);
            if(!ReducedMotion)follow.y+=Mathf.Sin(clock*3)*0.15f;
            Companion.position=Vector3.SmoothDamp(Companion.position,follow,ref companionVelocity,0.25f,20,dt);
            if(Guardian.gameObject.activeSelf)
            {
                guardianGlow.SetColor("_EmissionColor",Color.Lerp(Color.gray,RuneColors[Biome==0?0:Biome==1?1:3],guardianEnergy)*1.7f);
                if(!ReducedMotion)
                {
                    Guardian.localPosition=new Vector3(0,3.7f+Mathf.Sin(clock*1.5f)*0.18f,5.2f);
                    guardianCore.localRotation=Quaternion.Euler(0,Mathf.Sin(clock)*12,0);
                    for(int i=0;i<6;i++)guardianArms[i].Rotate(0,dt*40,dt*20);
                }
            }
            if(!ReducedMotion)
                for(int i=0;i<lanterns.Length;i++)
                {Vector3 p=lanterns[i].localPosition;p.y=3.5f+Mathf.Sin(clock*0.8f+i)*0.25f;lanterns[i].localPosition=p;}
        }
    }
}
