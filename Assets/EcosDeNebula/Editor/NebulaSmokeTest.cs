using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EcosDeNebula.Editor
{
    // Real Play Mode integration run, including procedural world creation, input-state
    // gating, pause, failed attempts, assistance, all nine seals and save/resume.
    [InitializeOnLoad]
    public static partial class NebulaSmokeTest
    {
        const string Flag="Nebula.Smoke.Active";
        const string Key="EcosDeNebula.Journey.v1";
        static NebulaGame game;
        static int stage, completions, checks;
        static double started, nextAction, pauseUntil;
        static float frozenClock;
        static string failure;
        static bool finishing;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;

        static NebulaSmokeTest()
        {
            if(SessionState.GetBool(Flag,false)) Register();
        }

        public static void Run()
        {
            Environment.SetEnvironmentVariable("NEBULA_DISABLE_AI",SessionState.GetBool("Nebula.Smoke.ApiOnce",false)?"0":"1");
            NebulaEditor.Configure();NebulaEditor.OpenAdventure();
            SessionState.SetBool("Nebula.Smoke.HadSave",PlayerPrefs.HasKey(Key));
            SessionState.SetString("Nebula.Smoke.Backup",PlayerPrefs.GetString(Key,""));
            SessionState.SetBool("Nebula.Smoke.HadProgress",PlayerPrefs.HasKey("EcosDeNebula.Progress.v1"));
            SessionState.SetString("Nebula.Smoke.ProgressBackup",PlayerPrefs.GetString("EcosDeNebula.Progress.v1",""));
            SessionState.SetBool(Flag,true);
            Register();
            EditorApplication.isPlaying=true;
        }

        public static void ApiOnce()
        {
            SessionState.SetBool("Nebula.Smoke.ApiOnce",true);
            Run();
        }

        public static void RenderWorlds()
        {
            SessionState.SetBool("Nebula.Smoke.RenderOnly",true);
            Run();
        }

        public static void BuildAndRender()
        {
            NebulaEditor.BuildWindows();
            RenderWorlds();
        }

        static void RenderWorld(int index)
        {
            Get<NebulaSave>("save").world=index;Call("BuildWorld");Call("UpdateCamera",1f,true);
            Camera camera=Get<Camera>("view");
            RenderTexture target=new RenderTexture(1600,900,24);
            RenderTexture previous=RenderTexture.active;
            camera.targetTexture=target;
            try
            {
                camera.Render();RenderTexture.active=target;
                Texture2D png=new Texture2D(1600,900,TextureFormat.RGB24,false);
                png.ReadPixels(new Rect(0,0,1600,900),0,0);png.Apply();
                Directory.CreateDirectory("Capturas");
                File.WriteAllBytes("Capturas/Santuario-"+(index+1)+".png",png.EncodeToPNG());
                UnityEngine.Object.Destroy(png);
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=previous;
                target.Release();UnityEngine.Object.Destroy(target);
            }
        }

        static void Register()
        {
            started=EditorApplication.timeSinceStartup;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            EditorApplication.playModeStateChanged-=OnMode;EditorApplication.playModeStateChanged+=OnMode;
            Application.logMessageReceived-=OnLog;Application.logMessageReceived+=OnLog;
        }

        static void OnLog(string message,string trace,LogType type)
        {
            // Unity 6000.6 can race its editor-only search index on a fresh batch import.
            // This service is absent from the game; retain its log but do not classify it as a gameplay failure.
            if(trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            if(type==LogType.Exception || type==LogType.Error) failure=message+"\n"+trace;
        }
        static void Check(bool condition,string message)
        {
            checks++;if(!condition)throw new Exception(message);
        }
        static T Get<T>(string field){return (T)typeof(NebulaGame).GetField(field,Private).GetValue(game);}
        static void Set(string field,object value){typeof(NebulaGame).GetField(field,Private).SetValue(game,value);}
        static void Call(string method,params object[] args){typeof(NebulaGame).GetMethod(method,Private).Invoke(game,args);}

        static void Tick()
        {
            if(finishing)return;
            double now=EditorApplication.timeSinceStartup;
            if(now-started>180){Finish("Timeout waiting for game progression");return;}
            if(failure!=null){Finish(failure);return;}
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling)return;
            if(game==null)game=UnityEngine.Object.FindAnyObjectByType<NebulaGame>();
            if(game==null || now<nextAction)return;
            nextAction=now+0.23;
            try
            {
                if(SessionState.GetBool("Nebula.Smoke.Expansion",false)){ExpansionTick(now);return;}
                JourneyPhase phase=Get<JourneyPhase>("phase");
                if(stage==0)
                {
                    Check(phase==JourneyPhase.Home,"Starts at title");
                    Check(UnityEngine.Object.FindObjectsByType<MeshRenderer>().Length>100,"3D world is populated");
                    Check(Get<NebulaWorld>("world").Runes.Length==4,"Four interactive crystals");
                    if(SessionState.GetBool("Nebula.Smoke.RenderOnly",false))
                    {
                        Call("StartJourney",true);
                        for(int i=0;i<3;i++)RenderWorld(i);
                        Finish(null);return;
                    }
                    Call("StartJourney",true);Call("BeginTrial");
                    Set("paused",true);frozenClock=Get<float>("phaseClock");pauseUntil=now+0.7;stage=1;
                }
                else if(stage==1 && now>=pauseUntil)
                {
                    Check(Math.Abs(Get<float>("phaseClock")-frozenClock)<0.001f,"Pause freezes gameplay clock");
                    Set("paused",false);stage=2;
                }
                else if(stage==2 && phase==JourneyPhase.Answering)
                {
                    if(SessionState.GetBool("Nebula.Smoke.ApiOnce",false))
                    {
                        Check(Get<PatternTrial>("trial").GeneratedByApi,"Unity must receive a validated API-contract pattern");
                        Check(Get<PatternTrial>("trial").Model.Length>0,"Provider model is visible");
                        SessionState.SetString("Nebula.Smoke.Provider",Get<PatternTrial>("trial").Model);
                        Call("BeginTrial");Call("UseLocalTrial");
                        Check(!Get<PatternTrial>("trial").GeneratedByApi,"Skipping API returns a truly local pattern");
                        pauseUntil=now+2;stage=5;return;
                    }
                    int expected=Get<PatternAttempt>("attempt").Expected;
                    Call("Submit",(expected+1)%4);
                    Check(Get<JourneyPhase>("phase")==JourneyPhase.Retry,"Incorrect response reaches retry");
                    Check(Get<NebulaSave>("save").seals==0,"Error never awards a seal");
                    Call("Replay");Check(Get<bool>("assisted"),"Replay marks assistance");stage=3;
                }
                else if(stage==3)
                {
                    if(phase==JourneyPhase.Answering)
                    {
                        Call("Submit",Get<PatternAttempt>("attempt").Expected);
                    }
                    else if(phase==JourneyPhase.Success)
                    {
                        completions++;
                        Check(Get<NebulaSave>("save").score>0,"Correct answer awards score");
                        Check(Get<NebulaSave>("save").seals>=1,"Correct answer restores seal");
                        Call("Advance");
                    }
                    else if(phase==JourneyPhase.Crossing)
                    {
                        int previous=Get<NebulaSave>("save").world;
                        Call("CrossWorld");
                        Check(Get<NebulaWorld>("world").Biome==previous+1,"New 3D biome loaded");
                        Call("BeginTrial");
                    }
                    else if(phase==JourneyPhase.Ending)
                    {
                        Check(completions==9,"Exactly nine successful trials complete the campaign");
                        Check(Get<NebulaSave>("save").completed,"Campaign completion persists");
                        Call("LoadJourney");Call("StartJourney",false);
                        Check(Get<JourneyPhase>("phase")==JourneyPhase.Ending,"Completed save resumes at ending");
                        Set("endless",true);Call("BeginTrial");stage=4;
                    }
                }
                else if(stage==4 && phase==JourneyPhase.Answering)
                {
                    Call("Submit",Get<PatternAttempt>("attempt").Expected);
                    if(Get<JourneyPhase>("phase")==JourneyPhase.Success)
                    {
                        Check(Get<NebulaSave>("save").seals==3,"Endless mode preserves campaign checkpoint");
                        Finish(null);
                    }
                }
                else if(stage==5 && now>=pauseUntil)
                {
                    Check(!Get<PatternTrial>("trial").GeneratedByApi,"Late API responses cannot replace a local pattern");
                    Finish(null);
                }
            }
            catch(Exception e){Finish(e.ToString());}
        }

        static void Finish(string error)
        {
            finishing=true;
            bool renderOnly=SessionState.GetBool("Nebula.Smoke.RenderOnly",false);
            bool apiOnce=SessionState.GetBool("Nebula.Smoke.ApiOnce",false);
            string result=error==null?(apiOnce?"PASS: Unity -> local bridge -> validated playable pattern; cancellation/fallback verified; model="+SessionState.GetString("Nebula.Smoke.Provider",""):renderOnly?"PASS: three offscreen 3D world renders (no HUD).":"PASS: Unity Play Mode; "+checks+" integration checks; all nine seals, three 3D worlds, pause, failure, replay, save/resume and endless mode."):"FAIL: "+error;
            if(error==null&&SessionState.GetBool("Nebula.Smoke.Expansion",false))result="PASS: expansion Play Mode; "+checks+" checks; six encounters, guardians, shield, blessings, daily defeat, checkpoint isolation, progression, collectibles, wardrobe, atlas pause and three spatial races.";
            Directory.CreateDirectory("TestResults");File.WriteAllText(apiOnce?"TestResults/Unity-api-result.txt":renderOnly?"TestResults/Unity-render-result.txt":"TestResults/Unity-smoke-result.txt",result);
            if(SessionState.GetBool("Nebula.Smoke.Expansion",false))File.WriteAllText("TestResults/Unity-expansion-result.txt",result);
            SessionState.SetInt("Nebula.Smoke.ExitCode",error==null?0:1);
            Debug.Log(result);EditorApplication.isPlaying=false;
        }

        static void OnMode(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Flag,false))return;
            if(SessionState.GetBool("Nebula.Smoke.HadSave",false))PlayerPrefs.SetString(Key,SessionState.GetString("Nebula.Smoke.Backup",""));
            else PlayerPrefs.DeleteKey(Key);
            if(SessionState.GetBool("Nebula.Smoke.HadProgress",false))PlayerPrefs.SetString("EcosDeNebula.Progress.v1",SessionState.GetString("Nebula.Smoke.ProgressBackup",""));
            else PlayerPrefs.DeleteKey("EcosDeNebula.Progress.v1");
            PlayerPrefs.Save();SessionState.SetBool(Flag,false);SessionState.SetBool("Nebula.Smoke.RenderOnly",false);
            SessionState.SetBool("Nebula.Smoke.ApiOnce",false);
            SessionState.SetBool("Nebula.Smoke.Expansion",false);
            EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
            EditorApplication.Exit(SessionState.GetInt("Nebula.Smoke.ExitCode",1));
        }
    }
}
