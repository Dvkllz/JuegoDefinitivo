using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcosDeNebula
{
    public sealed class RuneTarget : MonoBehaviour { public int Index; }

    public sealed class RuneVisual
    {
        public Transform Crystal;
        public Transform Halo;
        public Renderer Renderer;
        public Light Light;
        public Vector3 Position;
        public Color Color;
        public float Glow;
    }

    public sealed partial class NebulaWorld : MonoBehaviour
    {
        public static readonly Color[] RuneColors = {
            new Color(0.27f, 0.96f, 0.77f), new Color(1f, 0.68f, 0.28f),
            new Color(0.43f, 0.7f, 1f), new Color(0.84f, 0.48f, 1f)
        };
        [System.NonSerialized] public RuneVisual[] Runes = new RuneVisual[4];
        public Transform Luma, Weaver, Heart, Portal;
        public Transform[] Fragments = new Transform[4];
        public int Biome;
        readonly List<Material> materials = new List<Material>();
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly List<Transform> orbitals = new List<Transform>();
        readonly List<Transform> motes = new List<Transform>();
        readonly List<Vector3> moteOrigins = new List<Vector3>();
        Material sky;
        Material oldSky;
        float clock;
        LineRenderer constellation;
        string constellationStyle="orbit";
        float celebration;
        public bool ReducedMotion;

        Material Mat(Color color, float emission = 0)
        {
            Material m = new Material(Shader.Find("Standard"));
            m.color = color;
            m.SetFloat("_Glossiness", 0.28f);
            m.SetFloat("_Metallic", emission > 0 ? 0.24f : 0.05f);
            if (emission > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * emission); }
            materials.Add(m);
            return m;
        }

        GameObject Shape(PrimitiveType type, string label, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = label;
            go.transform.SetParent(parent == null ? transform : parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Destroy(collider); }
            return go;
        }

        GameObject Facet(string label, Vector3 position, float[] heights, float[] radii, int sides, Material material, Transform parent = null)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            for (int ring = 0; ring < heights.Length - 1; ring++)
            for (int s = 0; s < sides; s++)
            {
                float a = (float)s / sides * Mathf.PI * 2;
                float b = (float)(s + 1) / sides * Mathf.PI * 2;
                Vector3 p0 = new Vector3(Mathf.Cos(a)*radii[ring],heights[ring],Mathf.Sin(a)*radii[ring]);
                Vector3 p1 = new Vector3(Mathf.Cos(b)*radii[ring],heights[ring],Mathf.Sin(b)*radii[ring]);
                Vector3 p2 = new Vector3(Mathf.Cos(a)*radii[ring+1],heights[ring+1],Mathf.Sin(a)*radii[ring+1]);
                Vector3 p3 = new Vector3(Mathf.Cos(b)*radii[ring+1],heights[ring+1],Mathf.Sin(b)*radii[ring+1]);
                int n = vertices.Count;
                vertices.AddRange(new[] { p0, p2, p1, p1, p2, p3 });
                for (int k=0;k<6;k++) triangles.Add(n+k);
            }
            Mesh mesh = new Mesh(); mesh.name = label; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); meshes.Add(mesh);
            GameObject go = new GameObject(label,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent == null ? transform : parent,false);
            go.transform.localPosition = position;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        Transform Ring(string label, Vector3 position, float radius, float width, Material material, Transform parent = null)
        {
            GameObject go = new GameObject(label,typeof(LineRenderer));
            go.transform.SetParent(parent == null ? transform : parent,false);
            go.transform.localPosition = position;
            LineRenderer line = go.GetComponent<LineRenderer>();
            line.useWorldSpace = false; line.loop = true; line.positionCount = 96;
            line.startWidth=width; line.endWidth=width; line.sharedMaterial=material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            for(int i=0;i<96;i++) { float a=i*Mathf.PI*2/96; line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); }
            return go.transform;
        }

        public void Build(int biome)
        {
            Biome=biome;
            Color accent = RuneColors[biome == 0 ? 0 : biome == 1 ? 1 : 3];
            Material stone=Mat(new Color(0.16f,0.23f,0.27f));
            Material edge=Mat(new Color(0.27f,0.36f,0.39f));
            Material dark=Mat(new Color(0.07f,0.12f,0.16f));
            Material grass=Mat(biome==0 ? new Color(0.17f,0.40f,0.32f) : biome==1 ? new Color(0.48f,0.32f,0.19f) : new Color(0.28f,0.23f,0.43f));
            Material gold=Mat(new Color(0.74f,0.58f,0.32f),0.2f);
            Material lineMat=Mat(accent,1.6f);
            Material pale=Mat(new Color(0.84f,0.94f,0.89f));
            Material foliage=Mat(biome==0 ? new Color(0.2f,0.63f,0.49f) : biome==1 ? new Color(0.88f,0.48f,0.24f) : new Color(0.56f,0.39f,0.73f));
            var constellationObject=new GameObject("Constelación del Tejedor",typeof(LineRenderer));
            constellationObject.transform.SetParent(transform,false);
            constellation=constellationObject.GetComponent<LineRenderer>();constellation.useWorldSpace=false;
            constellation.sharedMaterial=lineMat;constellation.positionCount=100;constellation.widthMultiplier=0.025f;
            constellation.shadowCastingMode=ShadowCastingMode.Off;
            SetConstellation("orbit");

            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(0.43f,0.53f,0.65f);
            RenderSettings.ambientEquatorColor=new Color(0.28f,0.35f,0.43f);
            RenderSettings.ambientGroundColor=new Color(0.15f,0.2f,0.29f);
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=biome==0 ? new Color(0.18f,0.33f,0.38f) : biome==1 ? new Color(0.31f,0.24f,0.32f) : new Color(0.18f,0.19f,0.35f);
            RenderSettings.fogStartDistance=32; RenderSettings.fogEndDistance=110;
            oldSky=RenderSettings.skybox;
            Shader skyShader=Shader.Find("Nebula/GradientSky");
            if(skyShader != null)
            {
                sky=new Material(skyShader); materials.Add(sky);
                sky.SetColor("_Top",new Color(0.025f,0.055f,0.12f));
                sky.SetColor("_Horizon",RenderSettings.fogColor);
                sky.SetColor("_Bottom",new Color(0.07f,0.1f,0.19f));
                RenderSettings.skybox=sky;
            }
            GameObject sun=new GameObject("Luz de Nébula",typeof(Light)); sun.transform.SetParent(transform);
            sun.transform.rotation=Quaternion.Euler(42,-38,0);
            Light sunLight=sun.GetComponent<Light>(); sunLight.type=LightType.Directional;
            sunLight.color=new Color(1f,0.86f,0.69f);sunLight.intensity=1.6f;sunLight.shadows=LightShadows.Soft;
            sunLight.shadowStrength=0.65f; sunLight.shadowBias=0.04f;
            QualitySettings.shadowDistance=55; QualitySettings.antiAliasing=4;

            Facet("Isla flotante",Vector3.zero,new[]{-9.8f,-5.5f,-2.5f,-0.45f,-0.2f,-0.2f},new[]{0.4f,6.8f,9.4f,10.4f,10.0f,0f},15,stone);
            Facet("Borde de musgo",Vector3.zero,new[]{-0.48f,-0.12f,-0.12f},new[]{10.3f,10.1f,0f},15,grass);
            Facet("Plaza ceremonial",Vector3.zero,new[]{-0.1f,0.02f,0.02f},new[]{7.7f,7.5f,0f},48,edge);
            Facet("Mosaico central",Vector3.zero,new[]{0.03f,0.05f,0.05f},new[]{5.9f,5.9f,0f},48,stone);
            Ring("Círculo exterior",new Vector3(0,0.085f,0),7.15f,0.055f,gold);
            Ring("Círculo de ecos",new Vector3(0,0.09f,0),5.95f,0.035f,lineMat);
            Ring("Círculo del corazón",new Vector3(0,0.12f,0),2.35f,0.045f,gold);
            for(int i=0;i<32;i++)
            {
                float a=i*Mathf.PI*2/32;
                var tile=Shape(PrimitiveType.Cube,"Marca del santuario",new Vector3(Mathf.Cos(a)*6.75f,0.06f,Mathf.Sin(a)*6.75f),new Vector3(0.10f,0.035f,i%4==0?0.42f:0.2f),gold);
                tile.transform.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);
            }
            Vector3[] positions={new Vector3(-4.2f,0,-2.4f),new Vector3(4.2f,0,-2.4f),new Vector3(-4.2f,0,3.0f),new Vector3(4.2f,0,3.0f)};
            for(int i=0;i<4;i++)
            {
                Vector3 p=positions[i];
                Facet("Pedestal "+(i+1),p,new[]{0.08f,0.32f,0.45f,0.45f},new[]{1.2f,1.2f,0.98f,0f},8,dark);
                Ring("Sello de runa",p+Vector3.up*0.46f,0.84f,0.06f,Mat(RuneColors[i],1.5f));
                Ring("Aro de entrada",p+Vector3.up*0.12f,NebulaGame.RuneEntryRadius,0.075f,Mat(RuneColors[i],0.65f));
                Ring("Límite para repetir",p+Vector3.up*0.11f,NebulaGame.RuneExitRadius,0.025f,Mat(RuneColors[i]*0.55f,0.2f));
                GameObject crystal=Facet("Cristal "+(i+1),p+Vector3.up*1.35f,new[]{-0.65f,-0.28f,0.48f,1.02f},new[]{0f,0.52f,0.45f,0f},i+4,Mat(RuneColors[i],0.45f));
                var collider=crystal.AddComponent<SphereCollider>();collider.radius=0.95f;collider.center=Vector3.up*0.2f;
                crystal.AddComponent<RuneTarget>().Index=i;
                Light lamp=crystal.AddComponent<Light>();lamp.color=RuneColors[i];lamp.range=4;lamp.intensity=0.5f;
                Transform halo=Ring("Órbita de runa",p+Vector3.up*0.7f,0.64f,0.024f,Mat(RuneColors[i],1.2f));
                Runes[i]=new RuneVisual{Crystal=crystal.transform,Halo=halo,Renderer=crystal.GetComponent<Renderer>(),Light=lamp,Position=p,Color=RuneColors[i]};
                for(int tick=0;tick<=i;tick++) Shape(PrimitiveType.Cube,"Identificador táctil",p+new Vector3((tick-i*0.5f)*0.22f,0.5f,-0.75f),new Vector3(0.10f,0.06f,0.18f),pale);
            }
            Facet("Altar del corazón",Vector3.zero,new[]{0.06f,0.4f,0.55f,0.55f},new[]{1.65f,1.65f,1.25f,0f},12,dark);
            Heart=Facet("Corazón de Nébula",new Vector3(0,2.4f,0),new[]{-0.8f,0f,0.8f},new[]{0f,0.55f,0f},6,Mat(accent,1.25f)).transform;
            for(int i=0;i<3;i++)
            {
                Transform orbital=Ring("Astrolabio",new Vector3(0,2.4f,0),1.05f+i*0.19f,0.035f,i==1?lineMat:gold);
                orbital.rotation=Quaternion.Euler(35+i*35,20+i*60,15);
                orbitals.Add(orbital);
            }
            // The portal is an actual 3D arch, built from individual stone voussoirs.
            Portal=new GameObject("Puerta entre santuarios").transform;Portal.SetParent(transform,false);Portal.localPosition=new Vector3(0,0,7);
            for(int i=0;i<13;i++)
            {
                float a=i*Mathf.PI/12;
                var block=Shape(PrimitiveType.Cube,"Dovela",new Vector3(Mathf.Cos(a)*2.3f,2.1f+Mathf.Sin(a)*2.3f,0),new Vector3(0.64f,0.61f,0.8f),i%3==0?gold:edge,Portal);
                block.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);
            }
            Shape(PrimitiveType.Cube,"Pilar izquierdo",new Vector3(-2.3f,1,0),new Vector3(0.58f,2,0.8f),edge,Portal);
            Shape(PrimitiveType.Cube,"Pilar derecho",new Vector3(2.3f,1,0),new Vector3(0.58f,2,0.8f),edge,Portal);
            Transform gateRing=Ring("Umbral luminoso",new Vector3(0,2,0),1.85f,0.055f,lineMat,Portal);gateRing.localRotation=Quaternion.Euler(90,0,0);

            for(int side=-1;side<=1;side+=2)
            for(int i=0;i<3;i++)
            {
                Vector3 p=new Vector3(side*(7.6f-i*0.35f),0,1.5f+i*2.1f);
                Tree(p,1.1f+i*0.25f,stone,foliage,gold);
            }
            // Optional exploration rewards sit within the walkable boundary.
            Vector3[] fragmentPositions={new Vector3(-6.5f,0.75f,-1),new Vector3(6.5f,0.75f,-1),new Vector3(-2.4f,0.75f,6.6f),new Vector3(2.4f,0.75f,6.6f)};
            for(int i=0;i<4;i++)
            {
                Fragments[i]=Facet("Fragmento de memoria",fragmentPositions[i],new[]{-0.25f,0f,0.3f},new[]{0f,0.18f,0f},4,Mat(new Color(1,0.85f,0.45f),2)).transform;
                Ring("Sello del fragmento",fragmentPositions[i]+Vector3.down*0.65f,0.38f,0.025f,gold);
            }
            // Large celestial silhouettes make each sanctuary recognizable from its skyline.
            var planet=Shape(PrimitiveType.Sphere,"Luna guardiana",new Vector3(-20,16,35),Vector3.one*10,Mat(Color.Lerp(accent,new Color(0.2f,0.28f,0.45f),0.7f),0.15f));
            if(biome==2)
            {
                var belt=Ring("Anillos del observatorio",planet.transform.position,7.6f,0.13f,gold);
                belt.rotation=Quaternion.Euler(30,0,18);
                var belt2=Ring("Órbita de la luna",planet.transform.position,8.5f,0.04f,lineMat);
                belt2.rotation=belt.rotation;
            }
            System.Random rng=new System.Random(500+biome);
            for(int i=0;i<26;i++)
            {
                float a=(float)rng.NextDouble()*Mathf.PI*2;
                float r=8.1f+(float)rng.NextDouble()*1.3f;
                Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
                if(p.z>6 && Mathf.Abs(p.x)<3) continue;
                Facet("Roca de borde",p,new[]{0f,0.3f+(float)rng.NextDouble()*0.55f,0.65f},new[]{0.45f,0.35f,0f},5,i%3==0?foliage:stone);
            }
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI*2/16+0.2f;
                float radius=24+(float)rng.NextDouble()*28;
                Vector3 p=new Vector3(Mathf.Cos(a)*radius,-5+(float)rng.NextDouble()*12,Mathf.Sin(a)*radius);
                float size=1.6f+(float)rng.NextDouble()*3.1f;
                Facet("Archipiélago lejano",p,new[]{-size*1.5f,-0.3f,0f,0f},new[]{0f,size,size*0.9f,0f},7,stone);
                Facet("Jardín lejano",p,new[]{0,0.14f,0.14f},new[]{size*0.9f,size*0.88f,0},7,grass);
                Tree(p+Vector3.up*0.1f,size*0.65f,stone,foliage,gold);
            }
            for(int i=0;i<60;i++)
            {
                Vector3 p=new Vector3((float)rng.NextDouble()*24-12,(float)rng.NextDouble()*7+0.4f,(float)rng.NextDouble()*22-8);
                Transform mote=Shape(PrimitiveType.Sphere,"Polvo de estrellas",p,Vector3.one*(i%7==0?0.065f:0.025f),lineMat).transform;
                motes.Add(mote);moteOrigins.Add(p);
            }
            BuildLuma(pale,dark,gold,Mat(new Color(0.26f,0.73f,0.66f)),lineMat);
            Weaver=new GameObject("El Tejedor · IA adaptativa").transform;Weaver.SetParent(transform,false);Weaver.position=new Vector3(2.3f,3.7f,1);
            Facet("Máscara del Tejedor",Vector3.zero,new[]{-0.38f,0f,0.38f},new[]{0f,0.43f,0f},5,gold,Weaver);
            Shape(PrimitiveType.Sphere,"Ojo del Tejedor",new Vector3(0,0,-0.3f),Vector3.one*0.22f,lineMat,Weaver);
            var wRing=Ring("Anillo del Tejedor",Vector3.zero,0.64f,0.035f,gold,Weaver);wRing.localRotation=Quaternion.Euler(60,0,25);
            BuildExpansion(accent,stone,gold,lineMat);
        }

        void Tree(Vector3 p,float size,Material bark,Material leaves,Material gold)
        {
            if(Biome==2)
            {
                Facet("Columna astral",p,new[]{0f,size*1.3f,size*1.5f},new[]{size*0.22f,size*0.17f,size*0.3f},6,bark);
                Transform astrolabe=Ring("Mapa celeste",p+Vector3.up*size*1.65f,size*0.65f,0.045f,gold);
                astrolabe.rotation=Quaternion.Euler(75,0,20);
                Shape(PrimitiveType.Sphere,"Estrella cautiva",p+Vector3.up*size*1.65f,Vector3.one*size*0.38f,leaves);
                return;
            }
            if(Biome==1)
            {
                Shape(PrimitiveType.Cylinder,"Tallo de ámbar",p+Vector3.up*size*0.65f,new Vector3(size*0.12f,size*0.65f,size*0.12f),gold);
                for(int j=0;j<3;j++)
                {
                    var petal=Facet("Vela de cristal",p+Vector3.up*size*1.2f,new[]{0f,size*0.6f,size*1.1f},new[]{0.1f,size*0.3f,0f},4,leaves);
                    petal.transform.rotation=Quaternion.Euler(0,j*120,35);
                }
                return;
            }
            Shape(PrimitiveType.Cylinder,"Tronco",p+Vector3.up*size*0.65f,new Vector3(size*0.19f,size*0.65f,size*0.19f),bark);
            Facet("Copa mineral",p+Vector3.up*size,new[]{0f,size*0.85f,size*1.8f},new[]{size*0.85f,size*0.7f,0f},5,leaves);
            Shape(PrimitiveType.Sphere,"Fruto de luz",p+new Vector3(size*0.5f,size*1.2f,-0.2f),Vector3.one*0.14f,gold);
        }

        void BuildLuma(Material pale,Material dark,Material gold,Material cloth,Material glow)
        {
            Luma=new GameObject("Luma · guardiana de los ecos").transform;Luma.SetParent(transform,false);Luma.position=new Vector3(0,0.1f,-4.7f);Luma.rotation=Quaternion.Euler(0,180,0);
            Facet("Capa",Vector3.zero,new[]{0.22f,0.5f,1.08f},new[]{0.5f,0.42f,0.18f},8,cloth,Luma);
            Shape(PrimitiveType.Sphere,"Capucha",new Vector3(0,1.26f,0),Vector3.one*0.65f,pale,Luma);
            Shape(PrimitiveType.Sphere,"Máscara",new Vector3(0,1.27f,0.26f),new Vector3(0.45f,0.33f,0.16f),dark,Luma);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(PrimitiveType.Sphere,"Ojo",new Vector3(side*0.105f,1.3f,0.335f),new Vector3(0.075f,0.09f,0.035f),glow,Luma);
                Shape(PrimitiveType.Capsule,"Bota",new Vector3(side*0.18f,0.18f,0.08f),new Vector3(0.2f,0.21f,0.28f),dark,Luma);
                Shape(PrimitiveType.Sphere,"Mano",new Vector3(side*0.39f,0.7f,0.04f),Vector3.one*0.18f,pale,Luma);
            }
            Shape(PrimitiveType.Cylinder,"Bastón",new Vector3(0.56f,0.77f,0.08f),new Vector3(0.05f,0.66f,0.05f),gold,Luma);
            Facet("Luz del bastón",new Vector3(0.56f,1.52f,0.08f),new[]{-0.18f,0f,0.23f},new[]{0f,0.14f,0f},5,glow,Luma);
        }

        public void Pulse(int rune) { Runes[rune].Glow=1; }
        public void Celebrate(){celebration=2.5f;}
        public void SetConstellation(string style)
        {
            constellationStyle=style;
            if(constellation==null)return;
            constellation.loop=style=="orbit";
            for(int i=0;i<100;i++)
            {
                float t=i/99f,a=t*Mathf.PI*2;
                Vector3 p=style=="wave"?new Vector3((t-0.5f)*3,3.1f+Mathf.Sin(a*2)*0.35f,0):
                    style=="spiral"?new Vector3(Mathf.Cos(a*2)*(0.3f+t),2+t*1.8f,Mathf.Sin(a*2)*(0.3f+t)):
                    new Vector3(Mathf.Cos(a)*1.8f,2.8f+Mathf.Sin(a*2)*0.22f,Mathf.Sin(a)*1.8f);
                constellation.SetPosition(i,p);
            }
        }

        public void Animate(float dt)
        {
            clock+=dt;
            AnimateExpansion(dt);
            celebration=Mathf.Max(0,celebration-dt);
            if(celebration>0 && !ReducedMotion)
            {
                for(int i=0;i<4;i++)Runes[i].Glow=Mathf.Max(Runes[i].Glow,0.45f+Mathf.Sin(clock*8+i)*0.4f);
                Portal.localScale=Vector3.one*(1+Mathf.Sin(clock*5)*0.015f);
            }
            else Portal.localScale=Vector3.one;
            for(int i=0;i<4;i++)
            {
                RuneVisual r=Runes[i];r.Glow=Mathf.MoveTowards(r.Glow,0,dt*1.6f);
                r.Crystal.localPosition=r.Position+Vector3.up*(1.35f+(ReducedMotion?0:Mathf.Sin(clock*1.6f+i)*0.10f));
                if(!ReducedMotion) r.Crystal.Rotate(0,dt*(12+i*2),0,Space.Self);
                r.Crystal.localScale=Vector3.one*(1+r.Glow*0.15f);
                bool dimmed=codeFocus!=-1&&i!=codeFocus;
                r.Renderer.sharedMaterial.SetColor("_EmissionColor",r.Color*(dimmed?0.025f:0.35f+r.Glow*2.5f));
                r.Light.intensity=dimmed?0.025f:0.5f+r.Glow*3;
                r.Halo.localScale=Vector3.one*(1+r.Glow*0.35f);
            }
            if(ReducedMotion) return;
            if(constellation!=null)constellation.transform.localRotation=Quaternion.Euler(0,clock*8,0);
            for(int i=0;i<Fragments.Length;i++)if(Fragments[i]!=null)Fragments[i].Rotate(0,dt*55,0);
            Heart.Rotate(0,dt*20,0);Heart.localPosition=new Vector3(0,2.4f+Mathf.Sin(clock)*0.14f,0);
            for(int i=0;i<orbitals.Count;i++) orbitals[i].Rotate(dt*(9+i*3),dt*12,0,Space.Self);
            Weaver.localPosition=new Vector3(2.25f+Mathf.Sin(clock*0.4f)*0.4f,3.6f+Mathf.Sin(clock*1.2f)*0.16f,1);
            for(int i=0;i<motes.Count;i++) motes[i].localPosition=moteOrigins[i]+new Vector3(Mathf.Sin(clock*0.2f+i)*0.2f,Mathf.Sin(clock*0.5f+i)*0.25f,0);
        }

        void OnDestroy()
        {
            if(RenderSettings.skybox==sky) RenderSettings.skybox=oldSky;
            foreach(Material m in materials) if(m!=null) Destroy(m);
            foreach(Mesh m in meshes) if(m!=null) Destroy(m);
        }
    }
}
