using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDeNebula
{
    [Serializable]
    public sealed class NebulaSave
    {
        public int version = 1;
        public int world;
        public int seals;
        public int score;
        public int best;
        public bool completed;
        public LearningProfile learning = new LearningProfile();
        public int perfectSeals;
        public int fragments;
    }

    public enum JourneyPhase { Home, Briefing, Watching, Answering, Success, Retry, Crossing, Ending, Generating, Blessing, Race }

    public sealed partial class NebulaGame : MonoBehaviour
    {
        const string SaveKey = "EcosDeNebula.Journey.v1";
        const string MuteKey = "EcosDeNebula.Muted";
        const string MotionKey = "EcosDeNebula.ReducedMotion";
        public static readonly string[] RuneNames = { "HOJA", "SOL", "MAREA", "ESTRELLA" };
        static readonly string[] WorldNames = { "Bosque de Jade", "Mar de Ámbar", "Observatorio Violeta" };
        static readonly string[] WorldSubtitles = { "Donde los árboles recuerdan", "El tiempo fluye en dos sentidos", "El cielo todavía está por escribir" };
        static readonly string[] WorldRules = { "Repite los cristales en el mismo orden.", "Repite los cristales en orden inverso.", "Descubre el bloque que se repite y escríbelo una vez más." };
        static readonly string[] WorldStories = {
            "El Bosque de Jade ha olvidado su canción. Escucha los cristales y devuelve sus ecos al corazón del santuario.",
            "El Mar de Ámbar guarda recuerdos al revés. Aquí, el último eco que escuchas debe ser el primero que devuelvas.",
            "Las estrellas hablan con patrones. Observa dos repeticiones del mismo bloque y completa la tercera."
        };
        readonly Color ink = new Color(0.045f, 0.085f, 0.12f, 0.96f);
        readonly Color paper = new Color(0.88f, 0.94f, 0.90f);
        readonly Color dim = new Color(0.68f, 0.78f, 0.81f);
        readonly Color mint = new Color(0.65f, 0.91f, 0.75f);
        readonly Color amber = new Color(1f, 0.71f, 0.39f);
        Camera view;
        NebulaWorld world;
        NebulaAIClient ai;
        Coroutine generation;
        int generationId;
        bool showHint;
        bool apiEnabled=true;
        PatternDirector director;
        NebulaSave save;
        PatternTrial trial;
        PatternAttempt attempt;
        JourneyPhase phase = JourneyPhase.Home;
        Font serif, sans;
        GUIStyle textStyle;
        AudioSource voice, ambient;
        readonly List<AudioClip> clips = new List<AudioClip>();
        AudioClip[] notes;
        AudioClip winSound, errorSound;
        bool muted, reducedMotion, paused, assisted, showHelp, endless;
        float phaseClock, noteClock, answerClock, orbit = -18, desiredOrbit = -18;
        float cameraDistance = 26, lastInput = -10, flash;
        int shown, highlighted = -1, selected = -1, lastWrong = -1, scoreGain;
        string toast = "";
        float toastTime;
        Vector3 walkTarget;
        bool walkingToRune;
        float uiScale, uiW, uiH;
        bool hasSave;

        void Awake()
        {
            Application.targetFrameRate = 60;
            muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            reducedMotion = PlayerPrefs.GetInt(MotionKey, 0) == 1;
            LoadJourney();
            LoadProgress();
            serif = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Cambria", "Times New Roman" }, 64);
            sans = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Liberation Sans" }, 24);
            GameObject cameraObject = new GameObject("Cámara de Nébula", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform);
            view = cameraObject.GetComponent<Camera>(); view.tag = "MainCamera";
            view.fieldOfView = 45; view.nearClipPlane = 0.1f; view.farClipPlane = 180;
            view.clearFlags = CameraClearFlags.Skybox; view.allowHDR = true;
            BuildWorld();
            ai=gameObject.AddComponent<NebulaAIClient>();ai.Configure();
            MakeAudio();
            UpdateCamera(1, true);
        }

        void LoadJourney()
        {
            hasSave = PlayerPrefs.HasKey(SaveKey);
            try { save = hasSave ? JsonUtility.FromJson<NebulaSave>(PlayerPrefs.GetString(SaveKey)) : new NebulaSave(); }
            catch (Exception) { save = new NebulaSave(); hasSave = false; }
            if (save == null || save.version != 1) { save = new NebulaSave(); hasSave = false; }
            save.world = Mathf.Clamp(save.world, 0, 2);
            save.seals = Mathf.Clamp(save.seals, 0, 3);
            save.score = Mathf.Max(0, save.score); save.best = Mathf.Max(save.score, save.best);
            save.fragments &= 4095;save.perfectSeals=Mathf.Clamp(save.perfectSeals,0,1000000);
            if (save.learning == null) save.learning = new LearningProfile();
            save.learning.Sanitize();
            director = new PatternDirector(save.learning, Environment.TickCount);
        }

        void SaveJourney()
        {
            SaveProgress();
            save.best = Mathf.Max(save.score, save.best);
            // Endless rounds may affect learning and best score, but never invalidate the campaign checkpoint.
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.SetInt(MotionKey, reducedMotion ? 1 : 0);
            PlayerPrefs.Save(); hasSave = true;
        }

        void BuildWorld()
        {
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            int biome=race!=null?race.Biome:expedition==null?save.world:expedition.Biome;
            GameObject root = new GameObject("Santuario · " + WorldNames[biome]);
            root.transform.SetParent(transform);
            world = root.AddComponent<NebulaWorld>(); world.Build(biome); world.ReducedMotion = reducedMotion;
            if(progress!=null)world.SetCloak(progress.skin);
            if(ambient!=null)ambient.pitch=biome==0?1:biome==1?0.89f:1.12f;
            walkingToRune = false; selected = -1;
        }

        void StartJourney(bool fresh)
        {
            CancelGeneration();showHint=false;
            if (fresh)
            {
                int best = save.best;
                save = new NebulaSave { best = best };
                director = new PatternDirector(save.learning, Environment.TickCount);
                endless = false; BuildWorld();
            }
            if (save.completed || (save.world == 2 && save.seals >= 3))
            {
                save.completed = true; phase = JourneyPhase.Ending; SaveJourney(); return;
            }
            phase = save.seals >= 3 ? JourneyPhase.Crossing : JourneyPhase.Briefing;
            phaseClock = 0; paused = false; showHelp = false;
        }

        void BeginTrial()
        {
            if(expedition!=null){BeginExpeditionRound();return;}
            CancelGeneration();showHint=false;
            CreateLocalTrial();
            assisted = false; lastWrong = -1;
            if(apiEnabled && ai.Configured)
            {
                phase=JourneyPhase.Generating;phaseClock=0;
                generation=StartCoroutine(GenerateTrial(generationId));
            }
            else PlaySequence();
        }
        void CreateLocalTrial()
        {
            trial = director.Create(save.world, endless ? 2 : save.seals);
            trial.Hint=WorldRules[save.world];
            trial.Title=save.world==0?"El canto de las hojas":save.world==1?"Memorias a contraluz":"La danza de las estrellas";
        }

        IEnumerator GenerateTrial(int id)
        {
            yield return ai.Generate(save.world,save.seals,director.Profile,trial,delegate(bool success){});
            while(paused||showHelp||panel!=0)yield return null;
            if(id!=generationId || phase!=JourneyPhase.Generating)yield break;
            generation=null;world.SetConstellation(trial.Constellation);PlaySequence();
        }
        void CancelGeneration()
        {
            generationId++;
            if(ai!=null)ai.Cancel();
            if(generation!=null){StopCoroutine(generation);generation=null;}
        }
        void UseLocalTrial(){CancelGeneration();CreateLocalTrial();world.SetConstellation("orbit");PlaySequence();}

        void PlaySequence()
        {
            phase = JourneyPhase.Watching; shown = 0; highlighted = -1;
            noteClock = -0.65f; phaseClock = 0; answerClock = 0;
            attempt = new PatternAttempt(trial.Answer); walkingToRune = false;
        }

        void Replay()
        {
            if (phase != JourneyPhase.Answering && phase != JourneyPhase.Retry) return;
            assisted = true; PlaySequence();
            toast = "Escucha otra vez. La ayuda no impide avanzar."; toastTime = 3;
        }

        void Submit(int rune)
        {
            if (phase != JourneyPhase.Answering || paused || showHelp || panel!=0 || rune<0 || rune>3 || Time.unscaledTime - lastInput < 0.16f) return;
            lastInput = Time.unscaledTime;
            if(expedition!=null){SubmitExpedition(rune);return;}
            int expected = attempt.Expected;
            bool correct = attempt.Submit(rune);
            director.ObserveSymbol(expected, correct, answerClock);
            answerClock = 0; world.Pulse(rune); Play(notes[rune]);
            if (!correct)
            {
                lastWrong = expected; director.Finish(false, assisted);
                phase = JourneyPhase.Retry; phaseClock = 0; flash = 0.3f;
                Play(errorSound); SaveJourney();
            }
            else if (attempt.Complete)
            {
                if(!assisted)save.perfectSeals++;
                director.Finish(true, assisted);
                scoreGain = trial.Answer.Length * (assisted ? 55 : 100) + Mathf.Min(director.Profile.streak, 5) * 40;
                save.score += scoreGain;
                if (!endless) save.seals++;
                phase = JourneyPhase.Success; phaseClock = 0;
                for (int i = 0; i < 4; i++) world.Pulse(i);
                Play(winSound); SaveJourney();
                world.Celebrate();
                RewardCampaignWin();
            }
        }

        void Advance()
        {
            if(expedition!=null){AdvanceExpedition();return;}
            if (endless || save.seals < 3) { BeginTrial(); return; }
            if (save.world == 2)
            {
                save.completed = true; phase = JourneyPhase.Ending; SaveJourney();
            }
            else phase = JourneyPhase.Crossing;
            phaseClock = 0;
        }

        void CrossWorld()
        {
            save.world++; save.seals = 0; BuildWorld(); SaveJourney();
            phase = JourneyPhase.Briefing; phaseClock = 0;
        }

        void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(panel!=0)panel=0;
                else if (showHelp) showHelp = false;
                else if (phase != JourneyPhase.Home) paused = !paused;
            }
            if(Input.GetKeyDown(KeyCode.Tab)&&!paused&&!showHelp)panel=panel==0?1:0;
            if (paused || showHelp || panel!=0) { UpdateAudio(); return; }
            phaseClock += dt; toastTime -= dt; flash = Mathf.Max(0, flash - dt);
            world.Animate(dt); UpdateCamera(dt, false); UpdateAudio();
            UpdateExpansion(dt);
            if(phase==JourneyPhase.Generating)return;
            if (phase == JourneyPhase.Home || phase == JourneyPhase.Ending || phase == JourneyPhase.Crossing || phase==JourneyPhase.Blessing) return;
            if(phase==JourneyPhase.Race&&(raceCountdown>0||race.Finished)){UpdateRace(dt);return;}
            MoveLuma(dt);
            if(phase==JourneyPhase.Race){UpdateRace(dt);return;}
            if (phase == JourneyPhase.Watching)
            {
                noteClock += dt;
                if (noteClock >= 0 && shown < trial.Demonstration.Length)
                {
                    highlighted = trial.Demonstration[shown]; world.Pulse(highlighted); Play(notes[highlighted]);
                    shown++; noteClock = -trial.BeatSeconds;
                }
                if (shown == trial.Demonstration.Length && noteClock >= 0)
                {
                    phase = JourneyPhase.Answering; phaseClock = 0; answerClock = 0; highlighted = -1;
                }
            }
            else if (phase == JourneyPhase.Answering)
            {
                answerClock += dt;
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Submit(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Submit(1);
                else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Submit(2);
                else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) Submit(3);
                if (Input.GetKeyDown(KeyCode.Space)) Replay();
                if (Input.GetKeyDown(KeyCode.E) && selected >= 0) Submit(selected);
                if (Input.GetMouseButtonDown(0) && !PointerOverUI())
                {
                    RaycastHit hit;
                    if (Physics.Raycast(view.ScreenPointToRay(Input.mousePosition), out hit, 100))
                    {
                        RuneTarget rune = hit.collider.GetComponent<RuneTarget>();
                        if (rune != null)
                        {
                            walkTarget = world.Runes[rune.Index].Position + new Vector3(0, 0.1f, -1.15f);
                            walkingToRune = true; Submit(rune.Index);
                        }
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (phase == JourneyPhase.Briefing) BeginTrial();
                else if (phase == JourneyPhase.Success) Advance();
                else if (phase == JourneyPhase.Retry) Replay();
            }
        }

        bool PointerOverUI()
        {
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            Vector2 mouse = new Vector2(Input.mousePosition.x / scale, (Screen.height - Input.mousePosition.y) / scale);
            float w = Screen.width / scale, h = Screen.height / scale;
            return panel!=0 || mouse.y < 100 || mouse.y > h - 270 || (mouse.x < 315 && mouse.y < 655) || (mouse.x > w - 290 && mouse.y < 580);
        }

        void MoveLuma(float dt)
        {
            Vector3 input = new Vector3((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0), 0,
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
            Vector3 direction = Vector3.zero;
            if (input.sqrMagnitude > 0)
            {
                walkingToRune = false;
                Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = view.transform.right; right.y = 0; right.Normalize();
                direction = (right * input.x + forward * input.z).normalized;
            }
            else if (walkingToRune)
            {
                Vector3 delta = walkTarget - world.Luma.position; delta.y = 0;
                if (delta.magnitude < 0.15f) walkingToRune = false;
                else direction = delta.normalized;
            }
            if(dashTime>0&&direction.sqrMagnitude<0.01f)direction=world.Luma.forward;
            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 next = world.Luma.position + direction * (dashTime>0?13f:Input.GetKey(KeyCode.LeftShift) ? 5.2f : 3.5f) * dt;
                Vector2 plane = Vector2.ClampMagnitude(new Vector2(next.x, next.z), 7.4f);
                // Keep the guardian outside the solid central altar and crystal pedestals.
                if (plane.magnitude < 1.95f) plane = plane.normalized * 1.95f;
                for (int i=0; i<4; i++)
                {
                    Vector2 p = new Vector2(world.Runes[i].Position.x, world.Runes[i].Position.z);
                    Vector2 delta = plane - p;
                    if (delta.magnitude < 1.12f) plane = p + (delta.sqrMagnitude < 0.001f ? Vector2.down : delta.normalized) * 1.12f;
                }
                world.Luma.position = new Vector3(plane.x, 0.1f + (reducedMotion ? 0 : Mathf.Abs(Mathf.Sin(Time.unscaledTime * 10)) * 0.055f), plane.y);
                world.Luma.rotation = Quaternion.Slerp(world.Luma.rotation, Quaternion.LookRotation(direction), dt * 12);
            }
            selected = -1; float closest = 2.6f;
            for(int i=0;i<world.Fragments.Length;i++)
            {
                int bit=1<<(world.Biome*4+i);
                if((save.fragments&bit)!=0){world.Fragments[i].gameObject.SetActive(false);continue;}
                if(Vector3.Distance(world.Luma.position,world.Fragments[i].position)<1.25f)
                {
                    save.fragments|=bit;save.score+=40;world.Fragments[i].gameObject.SetActive(false);
                    toast="Fragmento de memoria encontrado · +40 destellos";toastTime=4;Play(notes[i]);SaveJourney();
                    RewardFragment();
                }
            }
            for (int i=0;i<4;i++)
            {
                float distance = Vector3.Distance(world.Luma.position, world.Runes[i].Position);
                if (distance < closest) { closest = distance; selected = i; }
            }
        }

        void UpdateCamera(float dt, bool immediate)
        {
            if (phase != JourneyPhase.Home && phase != JourneyPhase.Ending)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) desiredOrbit -= dt * 40;
                if (Input.GetKey(KeyCode.RightArrow)) desiredOrbit += dt * 40;
                if (Input.GetMouseButton(1)) desiredOrbit += Input.GetAxis("Mouse X") * 3;
                cameraDistance = Mathf.Clamp(cameraDistance - Input.mouseScrollDelta.y * 1.2f, 21, 33);
            }
            orbit = Mathf.LerpAngle(orbit, desiredOrbit, immediate ? 1 : dt * 5);
            bool menu = phase == JourneyPhase.Home;
            Vector3 target = menu ? new Vector3(-4.8f, 0.6f, 0) : new Vector3(-0.5f, 0.45f, 0);
            float angle = orbit + (menu && !reducedMotion ? Mathf.Sin(Time.unscaledTime * 0.09f) * 3 : 0);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * new Vector3(0, cameraDistance * 0.65f, -cameraDistance);
            Vector3 position = target + offset;
            view.transform.position = Vector3.Lerp(view.transform.position, position, immediate ? 1 : dt * 3);
            view.transform.rotation = Quaternion.Slerp(view.transform.rotation, Quaternion.LookRotation(target - view.transform.position), immediate ? 1 : dt * 4);
        }

        void MakeAudio()
        {
            voice = gameObject.AddComponent<AudioSource>(); voice.spatialBlend = 0; voice.volume = 0.45f;
            ambient = gameObject.AddComponent<AudioSource>(); ambient.spatialBlend = 0; ambient.loop = true; ambient.volume = 0.13f;
            notes = new AudioClip[4];
            float[] frequencies = { 261.63f, 329.63f, 392f, 493.88f };
            for(int i=0;i<4;i++) notes[i] = Tone("Runa " + RuneNames[i], new[] { frequencies[i] }, 0.65f, false);
            winSound = Tone("Acorde de restauración", new[] { 261.63f, 329.63f, 392f, 523.25f }, 1.5f, false);
            errorSound = Tone("Eco interrumpido", new[] { 164.81f, 174.61f }, 0.5f, false);
            ambient.clip = Tone("Viento armónico", new[] { 130f, 195f, 260f, 327f }, 12, true);
            ambient.Play(); UpdateAudio();
        }

        AudioClip Tone(string label, float[] frequencies, float duration, bool pad)
        {
            const int rate = 22050;
            float[] data = new float[(int)(rate * duration)];
            for(int i=0;i<data.Length;i++)
            {
                float t = (float)i/rate;
                float envelope = pad ? Mathf.Pow(Mathf.Sin(Mathf.PI*t/duration),2) * 0.55f : Mathf.Min(t/0.025f,1) * Mathf.Exp(-t*4/duration) * Mathf.Min((duration-t)/0.08f,1);
                float sample = 0;
                foreach(float f in frequencies) sample += Mathf.Sin(t*f*Mathf.PI*2)*0.7f + Mathf.Sin(t*f*2*Mathf.PI*2)*0.15f;
                data[i] = sample / frequencies.Length * envelope * 0.5f;
            }
            AudioClip clip = AudioClip.Create(label,data.Length,1,rate,false); clip.SetData(data,0); clips.Add(clip); return clip;
        }

        void Play(AudioClip clip) { if(!muted && clip!=null) voice.PlayOneShot(clip); }
        void UpdateAudio() { if(ambient!=null) ambient.mute=muted || paused || showHelp || panel!=0; if(voice!=null) voice.mute=muted || paused || showHelp || panel!=0; }

        void OnGUI()
        {
            if(view==null || world==null || save==null) return;
            uiScale = Mathf.Min(Screen.width/1600f,Screen.height/900f);
            uiW = Screen.width/uiScale; uiH=Screen.height/uiScale;
            Matrix4x4 previous=GUI.matrix; GUI.matrix=Matrix4x4.Scale(new Vector3(uiScale,uiScale,1));
            if(textStyle==null) textStyle=new GUIStyle(GUI.skin.label);
            bool previousEnabled=GUI.enabled;GUI.enabled=previousEnabled&&panel==0;
            Header();
            if(phase==JourneyPhase.Home) HomeUI();
            else
            {
                if(phase!=JourneyPhase.Ending) JourneyUI();
                if(phase==JourneyPhase.Briefing) BriefingUI();
                if(phase==JourneyPhase.Watching || phase==JourneyPhase.Answering) TrialUI();
                if(phase==JourneyPhase.Generating) GeneratingUI();
                if(phase==JourneyPhase.Success) ResultUI(true);
                if(phase==JourneyPhase.Retry) ResultUI(false);
                if(phase==JourneyPhase.Crossing) CrossingUI();
                if(phase==JourneyPhase.Ending) EndingUI();
                if(phase==JourneyPhase.Blessing) BlessingUI();
            }
            ExpansionUI();
            if(toastTime>0 && !paused && !showHelp) { Box(new Rect(uiW/2-270,uiH-238,540,34),ink); Label(new Rect(uiW/2-258,uiH-233,516,24),toast,14,mint,TextAnchor.MiddleCenter); }
            if(flash>0) Box(new Rect(0,0,uiW,uiH),new Color(0.7f,0.25f,0.13f,flash*0.12f));
            if(paused) PauseUI();
            if(showHelp) HelpUI();
            GUI.enabled=previousEnabled;
            if(panel!=0) AtlasUI();
            GUI.matrix=previous;
        }

        void Box(Rect rect,Color color)
        {
            Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;
        }
        void Label(Rect rect,string value,int size,Color color,TextAnchor alignment=TextAnchor.UpperLeft,bool display=false)
        {
            textStyle.font=display?serif:sans;textStyle.fontSize=Mathf.Max(14,size);textStyle.normal.textColor=color;textStyle.alignment=alignment;
            textStyle.wordWrap=true;textStyle.richText=false;textStyle.fontStyle=FontStyle.Normal;
            GUI.Label(rect,value,textStyle);
        }
        bool Button(Rect rect,string title,bool primary=false,bool enabled=true)
        {
            bool hover=rect.Contains(Event.current.mousePosition) && !paused && !showHelp;
            Color bg=primary?mint:new Color(0.09f,0.17f,0.21f,0.96f);
            if(hover) bg=Color.Lerp(bg,Color.white,0.1f);
            if(!enabled) bg=new Color(0.08f,0.13f,0.16f,0.8f);
            Box(rect,bg);
            if(!primary) Box(new Rect(rect.x,rect.yMax-1,rect.width,1),new Color(0.55f,0.73f,0.72f,0.3f));
            Label(rect,title,17,enabled?(primary?ink:paper):dim,TextAnchor.MiddleCenter);
            bool previous=GUI.enabled;GUI.enabled=enabled && previous;
            bool pressed=GUI.Button(rect,GUIContent.none,GUIStyle.none);GUI.enabled=previous;return pressed;
        }
        void RuleLine(float x,float y,float width) { Box(new Rect(x,y,width,1),new Color(0.67f,0.85f,0.78f,0.22f)); }

        void Header()
        {
            Box(new Rect(0,0,uiW,86),new Color(0.035f,0.065f,0.10f,0.88f));
            Label(new Rect(38,22,45,40),"✧",31,mint,TextAnchor.MiddleCenter);
            Label(new Rect(92,22,310,27),"ECOS DE NÉBULA",21,paper);
            Label(new Rect(94,51,350,18),"UNA AVENTURA DE PATRONES · 3D",10,dim);
            bool canClick=!paused&&!showHelp&&panel==0; bool previous=GUI.enabled;GUI.enabled=previous&&canClick;
            if(Button(new Rect(uiW-365,22,104,40),muted?"Sonido: no":"Sonido: sí")) {muted=!muted;SaveJourney();}
            if(Button(new Rect(uiW-251,22,105,40),"Cómo jugar")) showHelp=true;
            if(Button(new Rect(uiW-136,22,100,40),phase==JourneyPhase.Home?"Acerca de":"Pausa  II")) {if(phase==JourneyPhase.Home)showHelp=true;else paused=true;}
            GUI.enabled=previous;
        }

        void HomeUI()
        {
            bool previous=GUI.enabled;GUI.enabled=previous&&!showHelp&&!paused;
            Box(new Rect(0,86,580,uiH-86),new Color(0.035f,0.075f,0.105f,0.87f));
            Label(new Rect(72,145,450,24),"01 / EL ARCHIPIÉLAGO DE LOS ECOS",12,mint);
            Label(new Rect(66,194,485,185),"El mundo tiene\nun patrón.",66,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(72,389,434,81),"Encuéntralo. Despierta sus islas.\nUna inteligencia que aprende de ti\nteje cada nuevo desafío.",21,new Color(0.67f,0.79f,0.78f));
            RuleLine(74,500,425);
            Label(new Rect(74,523,416,54),"Eres Luma, la última guardiana de un cielo que ha perdido sus recuerdos.",17,paper);
            bool resume=hasSave&&(save.score>0||save.learning.trials>0||save.completed);
            if(Button(new Rect(74,608,424,61),resume?"Continuar mi viaje   →":"Despertar el primer eco   →",true)) StartJourney(false);
            if(resume)
            {
                Label(new Rect(76,683,420,25),WorldNames[save.world]+"  ·  "+save.score+" destellos",14,dim);
                if(Button(new Rect(74,724,200,43),"Nueva aventura")) {showHelp=false; paused=true;}
            }
            else Label(new Rect(74,689,435,42),"Sin prisas. Sin vidas que perder.\nSolo tú, los cristales y el siguiente descubrimiento.",14,dim);
            Label(new Rect(74,uiH-68,460,28),"3 SANTUARIOS    /    9 SELLOS    /    "+(ai.Configured?"OPENAI + LOCAL":"MODO LOCAL"),11,mint);
            Box(new Rect(uiW-406,uiH-153,360,105),ink);
            Label(new Rect(uiW-385,uiH-137,319,21),"EL TEJEDOR",12,mint);
            Label(new Rect(uiW-385,uiH-109,319,46),"«Cada intento me enseña\na crear tu próximo desafío.»",18,paper,TextAnchor.UpperLeft,true);
            GUI.enabled=previous;
        }

        void JourneyUI()
        {
            if(race!=null){RaceUI();return;}
            if(expedition!=null){ExpeditionHUD();return;}
            Box(new Rect(30,112,272,205),ink);
            Label(new Rect(50,131,232,22),endless?"MODO CONSTELACIÓN":"SANTUARIO 0"+(save.world+1)+" / 03",11,mint);
            Label(new Rect(49,164,233,69),WorldNames[save.world],29,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(50,239,225,22),endless?"Ecos sin fin":"SELLOS RESTAURADOS",10,dim);
            for(int i=0;i<3;i++)
            {
                Box(new Rect(51+i*73,276,60,5),i<save.seals?mint:new Color(0.2f,0.29f,0.32f));
                Label(new Rect(51+i*73,288,60,17),"0"+(i+1),9,i<save.seals?mint:dim);
            }
            Box(new Rect(30,330,272,103),ink);
            Label(new Rect(50,347,110,22),"DESTELLOS",10,dim);
            Label(new Rect(49,374,150,40),save.score.ToString("N0"),30,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(210,349,70,18),"RACHA",9,dim,TextAnchor.MiddleCenter);
            Label(new Rect(210,376,70,30),director.Profile.streak.ToString(),21,mint,TextAnchor.MiddleCenter);
            int found=0;for(int i=0;i<4;i++)if((save.fragments&(1<<(save.world*4+i)))!=0)found++;
            Box(new Rect(30,446,272,55),ink);
            Label(new Rect(50,459,234,29),"MEMORIAS OCULTAS   "+found+" / 4",12,mint);

            Box(new Rect(uiW-278,112,248,338),ink);
            Label(new Rect(uiW-257,131,215,20),"EL TEJEDOR · IA",11,mint);
            Label(new Rect(uiW-257,164,209,71),trial==null?"Estoy listo para escuchar cómo piensas.":trial.Intent,16,paper);
            RuleLine(uiW-257,242,205);
            Label(new Rect(uiW-257,259,205,20),"APRENDIZAJE EN TIEMPO REAL",9,dim);
            float skill=director.Profile.skill;
            Box(new Rect(uiW-257,289,205,3),new Color(0.19f,0.29f,0.32f));
            Box(new Rect(uiW-257,289,205*skill,3),mint);
            Label(new Rect(uiW-257,304,205,19),"Afinidad "+Mathf.RoundToInt(skill*100)+"%  ·  "+director.Profile.trials+" intentos",11,dim);
            RuleLine(uiW-257,338,205);
            string origin=trial!=null&&trial.GeneratedByApi?"OPENAI · "+trial.Model:apiEnabled?ai.Status:"Modo local elegido";
            Label(new Rect(uiW-257,352,205,33),origin,11,trial!=null&&trial.GeneratedByApi?mint:dim);
            Label(new Rect(uiW-257,391,205,43),trial==null?"Cada patrón cuenta una historia.":trial.Title,17,paper,TextAnchor.UpperLeft,true);
            if(phase==JourneyPhase.Watching || phase==JourneyPhase.Answering) RuneLabels();
        }

        void RuneLabels()
        {
            for(int i=0;i<4;i++)
            {
                Vector3 p=view.WorldToScreenPoint(world.Runes[i].Crystal.position+Vector3.up*1.2f);
                if(p.z<0)continue;
                float x=p.x/uiScale,y=(Screen.height-p.y)/uiScale;
                Color c=NebulaWorld.RuneColors[i];
                Rect rect=new Rect(x-57,y-18,114,32);
                Box(rect,new Color(ink.r,ink.g,ink.b,0.9f));
                Box(new Rect(rect.x,rect.y,3,32),c);
                Label(rect,(i+1)+"  "+RuneNames[i],12,c,TextAnchor.MiddleCenter);
                if(i==selected && phase==JourneyPhase.Answering)Label(new Rect(x-66,y+19,132,24),"E · ACTIVAR",11,paper,TextAnchor.MiddleCenter);
            }
        }

        void BriefingUI()
        {
            if(expedition!=null){ExpeditionBriefingUI();return;}
            float x=uiW/2-340;
            Box(new Rect(x,uiH-326,680,286),ink);
            Label(new Rect(x+30,uiH-304,620,22),"0"+(save.world+1)+"  /  "+WorldSubtitles[save.world].ToUpperInvariant(),11,mint);
            Label(new Rect(x+29,uiH-270,622,50),save.world==0?"Escucha. Recuerda. Responde.":save.world==1?"El final es el principio.":"Imagina el siguiente eco.",32,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+30,uiH-206,620,57),WorldStories[save.world],17,paper);
            Label(new Rect(x+30,uiH-134,290,54),"Activa con clic o teclas 1–4.\nTambién puedes caminar con WASD.",13,dim);
            if(Button(new Rect(x+352,uiH-123,296,55),"Escuchar el patrón   →",true,!paused&&!showHelp))BeginTrial();
        }

        void TrialUI()
        {
            bool watching=phase==JourneyPhase.Watching;
            float x=uiW/2-435, y=uiH-205;
            Box(new Rect(x,y,870,172),ink);
            Box(new Rect(x,y,870,2),watching?amber:mint);
            Label(new Rect(x+26,y+14,590,42),watching?"ESCUCHA EL PATRÓN":"TU TURNO  /  "+CurrentRule.ToUpperInvariant(),watching?13:11,watching?amber:mint);
            int slots=watching?trial.Demonstration.Length:trial.Answer.Length;
            float chip=39,gap=10;
            for(int i=0;i<slots;i++)
            {
                Rect r=new Rect(x+27+i*(chip+gap),y+62,chip,39);
                bool filled=watching?i<shown:i<attempt.Position;
                Box(r,filled?new Color(0.2f,0.34f,0.34f):new Color(0.1f,0.18f,0.22f));
                if(watching&&i==shown-1)Box(new Rect(r.x,r.yMax-2,r.width,2),NebulaWorld.RuneColors[trial.Demonstration[i]]);
                Label(r,watching&&i<shown?(trial.Demonstration[i]+1).ToString():filled?"✓":"·",20,filled?mint:dim,TextAnchor.MiddleCenter);
            }
            Label(new Rect(x+27,y+120,550,24),watching?"Mira qué cristales se iluminan. Todavía no tienes que responder.":"1 2 3 4 · runas     WASD · caminar     E · activar     ← → · cámara",12,dim);
            if(watching)Label(new Rect(x+647,y+60,190,48),shown+" / "+trial.Demonstration.Length,30,paper,TextAnchor.MiddleCenter,true);
            else if(Button(new Rect(x+633,y+57,211,47),"Volver a escuchar",false,!paused&&!showHelp))Replay();
            if(!watching && trial.Mode==PatternMode.Prediction)
            {
                Box(new Rect(uiW/2-223,y-116,446,42),ink);
                string symbols="";
                for(int i=0;i<trial.Demonstration.Length;i++) symbols+=(trial.Demonstration[i]+1)+(i==trial.MotifLength-1?"   /   ":"  ");
                Label(new Rect(uiW/2-214,y-109,428,27),symbols+" /   ?",19,mint,TextAnchor.MiddleCenter);
            }
            if(!watching)
            {
                bool enabled=!paused&&!showHelp;
                for(int i=0;i<4;i++)
                {
                    Rect r=new Rect(x+28+i*130,y-55,121,42);
                    Box(new Rect(r.x,r.y-3,r.width,3),NebulaWorld.RuneColors[i]);
                    if(Button(r,(i+1)+" · "+RuneNames[i],false,enabled))Submit(i);
                }
                if(Button(new Rect(x+633,y+115,211,36),showHint?"Ocultar pista":"Pedir una pista",false,enabled)){showHint=!showHint;if(showHint)assisted=true;}
                if(showHint)
                {
                    Box(new Rect(uiW-278,464,248,105),ink);
                    Label(new Rect(uiW-257,480,207,78),trial.Hint,15,mint);
                }
            }
        }

        void GeneratingUI()
        {
            float x=uiW/2-350,y=uiH-265;
            Box(new Rect(x,y,700,225),ink);Box(new Rect(x,y,700,2),mint);
            Label(new Rect(x+28,y+22,640,54),"El Tejedor imagina tu próximo eco"+new string('.',(int)(phaseClock*2)%4),28,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+28,y+92,640,52),"OpenAI está creando el patrón y una pista para ti. Puedes continuar con un reto local en cualquier momento.",17,dim);
            if(Button(new Rect(x+28,y+164,644,42),"Continuar ahora con un patrón local",false,!paused&&!showHelp))UseLocalTrial();
        }

        void ResultUI(bool success)
        {
            if(expedition!=null){ExpeditionResultUI(success);return;}
            float x=uiW/2-332,y=uiH-284;
            Box(new Rect(x,y,664,247),ink);Box(new Rect(x,y,664,2),success?mint:amber);
            Label(new Rect(x+28,y+21,600,23),success?"EL SANTUARIO TE RECUERDA":"EL ECO SE DESVANECIÓ",11,success?mint:amber);
            Label(new Rect(x+27,y+55,610,53),success?"Un poco más de luz.":"Volvamos a escucharlo.",36,paper,TextAnchor.UpperLeft,true);
            string message=success?"+"+scoreGain+" destellos  ·  "+(assisted?"Sello restaurado con ayuda.":"El Tejedor ha aprendido de tu respuesta."):
                "Se esperaba "+(lastWrong+1)+" · "+RuneNames[Mathf.Max(0,lastWrong)]+". No pierdes sellos; puedes intentarlo de nuevo.";
            Label(new Rect(x+28,y+117,603,48),message,16,dim);
            if(Button(new Rect(x+28,y+181,608,44),success?(save.seals>=3&&!endless?"Completar santuario   →":"Tejer el siguiente patrón   →"):"Escuchar y reintentar   →",true,!paused&&!showHelp))
            { if(success)Advance();else Replay(); }
        }

        void CrossingUI()
        {
            float x=uiW/2-340,y=uiH-310;
            Box(new Rect(x,y,680,271),ink);
            Label(new Rect(x+29,y+24,620,23),"TRES SELLOS · UN MUNDO DESPIERTO",11,mint);
            Label(new Rect(x+28,y+60,620,55),"El portal está abierto.",40,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+30,y+126,620,55),"Luma ha devuelto la memoria a "+WorldNames[save.world]+". El siguiente santuario guarda una nueva regla.",17,dim);
            if(Button(new Rect(x+30,y+200,620,48),"Viajar a "+WorldNames[Mathf.Min(save.world+1,2)]+"   →",true,!paused&&!showHelp)) CrossWorld();
        }

        void EndingUI()
        {
            if(expedition!=null){ExpeditionEndingUI();return;}
            float x=uiW/2-340;
            Box(new Rect(x,177,680,571),ink);
            Label(new Rect(x+40,209,600,25),"LOS NUEVE SELLOS HAN DESPERTADO",12,mint,TextAnchor.MiddleCenter);
            Label(new Rect(x+35,264,610,112),"El cielo vuelve\na recordar.",53,paper,TextAnchor.MiddleCenter,true);
            Label(new Rect(x+60,405,560,70),"Luma aprendió a escuchar. El Tejedor aprendió de Luma.\nY juntos encontraron un patrón nuevo: seguir intentándolo.",19,dim,TextAnchor.MiddleCenter);
            Label(new Rect(x+40,492,600,47),save.score.ToString("N0")+" destellos",35,mint,TextAnchor.MiddleCenter,true);
            if(Button(new Rect(x+40,564,600,54),"Seguir explorando · modo Constelación",true,!paused&&!showHelp)) {endless=true;BeginTrial();}
            if(Button(new Rect(x+40,637,600,43),"Volver al inicio",false,!paused&&!showHelp)) {phase=JourneyPhase.Home;endless=false;}
        }

        void PauseUI()
        {
            Box(new Rect(0,0,uiW,uiH),new Color(0.015f,0.035f,0.06f,0.78f));
            float x=uiW/2-250,y=uiH/2-240;
            Box(new Rect(x,y,500,480),ink);
            Label(new Rect(x+30,y+30,440,60),phase==JourneyPhase.Home?"Un nuevo comienzo":"Un momento de calma",34,paper,TextAnchor.MiddleCenter,true);
            if(phase==JourneyPhase.Home)
            {
                Label(new Rect(x+40,y+121,420,94),"Comenzarás desde el Bosque de Jade con un nuevo aprendizaje. Tu récord se conserva.",20,dim,TextAnchor.MiddleCenter);
                if(Button(new Rect(x+40,y+262,420,55),"Comenzar de nuevo",true)) {paused=false;StartJourney(true);SaveJourney();}
                if(Button(new Rect(x+40,y+334,420,50),"Conservar mi viaje"))paused=false;
            }
            else
            {
                bool localActivity=expedition!=null||race!=null;
                if(Button(new Rect(x+40,y+100,420,35),localActivity?"Actividad local · no consume API":apiEnabled?"Generación API: activada":"Generación API: desactivada",false,!localActivity))
                {apiEnabled=!apiEnabled;if(!apiEnabled&&phase==JourneyPhase.Generating)UseLocalTrial();}
                if(Button(new Rect(x+40,y+159,420,55),"Continuar",true))paused=false;
                if(Button(new Rect(x+40,y+231,420,48),reducedMotion?"Movimiento reducido: sí":"Movimiento reducido: no")) {reducedMotion=!reducedMotion;world.ReducedMotion=reducedMotion;SaveJourney();}
                if(Button(new Rect(x+40,y+296,420,48),muted?"Activar sonido":"Silenciar sonido")) {muted=!muted;SaveJourney();}
                if(Button(new Rect(x+40,y+361,420,48),expedition!=null?"Retirarse · conservar recompensas":"Guardar y volver al inicio")) ReturnHome();
            }
        }

        void HelpUI()
        {
            Box(new Rect(0,0,uiW,uiH),new Color(0.015f,0.035f,0.06f,0.85f));
            float x=uiW/2-380,y=uiH/2-330;
            Box(new Rect(x,y,760,660),ink);
            Label(new Rect(x+35,y+27,690,59),"Todo empieza por escuchar.",39,paper,TextAnchor.UpperLeft,true);
            Label(new Rect(x+36,y+103,680,66),"Observa la secuencia de luces y sonidos. Después, activa los cristales siguiendo la regla del santuario. Restaura tres sellos para viajar al siguiente mundo.",18,paper);
            RuleLine(x+36,y+194,688);
            Label(new Rect(x+36,y+220,320,155),"1–4 o clic: activar runas\nWASD: mover a Luma\nE: activar cristal cercano\nMayús: caminar más rápido",17,dim);
            Label(new Rect(x+388,y+220,338,155),"← → o arrastre derecho: cámara\nRueda: acercar o alejar\nEspacio: escuchar de nuevo\nEsc: pausa / cerrar ayuda",17,dim);
            RuleLine(x+36,y+370,688);
            Label(new Rect(x+36,y+393,688,27),"OPENAI GENERATIVA + APRENDIZAJE LOCAL",12,mint);
            Label(new Rect(x+36,y+436,688,104),"Con Jugar-con-IA, OpenAI crea patrones, nombres y pistas a partir de tus estadísticas de runas. El juego valida cada reto. La clave queda en el puente local, nunca en el ejecutable. Si la API falla, sigues jugando en modo local; el panel indica el origen real del patrón. La API puede consumir saldo de tu cuenta.",16,dim);
            if(Button(new Rect(x+36,y+559,688,55),"Entendido   →",true))showHelp=false;
        }

        void OnApplicationPause(bool isPaused) { if(isPaused && save!=null)SaveJourney(); }
        void OnApplicationQuit() { if(save!=null)SaveJourney(); }
        void OnDestroy()
        {
            CancelGeneration();
            foreach(AudioClip c in clips)if(c!=null)Destroy(c);
            if(serif!=null)Destroy(serif);if(sans!=null)Destroy(sans);
        }
    }
}
