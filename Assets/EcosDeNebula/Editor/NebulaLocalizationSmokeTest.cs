using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EcosDeNebula.Editor
{
    [InitializeOnLoad]
    public static class NebulaLocalizationSmokeTest
    {
        const string Flag="Nebula.LocalizationTest.Active",Key="EcosDeNebula.Language.v1";
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static double started;
        static int checks;
        static readonly string[] StringKeys={Key,"EcosDeNebula.Journey.v1","EcosDeNebula.Progress.v1"};
        static readonly string[] IntKeys={"EcosDeNebula.Difficulty.v1","EcosDeNebula.Muted","EcosDeNebula.ReducedMotion"};
        static NebulaLocalizationSmokeTest() { if(SessionState.GetBool(Flag,false))Register(); }
        public static void BuildAndRun() {NebulaEditor.BuildWindows();Run();}
        public static void Run()
        {
            Environment.SetEnvironmentVariable("NEBULA_DISABLE_AI","1");
            foreach(string key in StringKeys) {SessionState.SetBool("Nebula.LocalizationTest.Had."+key,PlayerPrefs.HasKey(key));SessionState.SetString("Nebula.LocalizationTest.Value."+key,PlayerPrefs.GetString(key,""));}
            foreach(string key in IntKeys) {SessionState.SetBool("Nebula.LocalizationTest.Had."+key,PlayerPrefs.HasKey(key));SessionState.SetInt("Nebula.LocalizationTest.Value."+key,PlayerPrefs.GetInt(key,0));}
            SessionState.SetBool(Flag,true);NebulaEditor.Configure();NebulaEditor.OpenAdventure();Register();
            EditorApplication.isPlaying=true;
        }
        static void Register() {started=EditorApplication.timeSinceStartup;EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.playModeStateChanged-=OnMode;EditorApplication.playModeStateChanged+=OnMode;}
        static void Check(bool condition,string message) {checks++;if(!condition)throw new Exception(message);}
        static object Call(NebulaGame game,string method,params object[] args) {return typeof(NebulaGame).GetMethod(method,Private).Invoke(game,args);}
        static T Field<T>(NebulaGame game,string name) {return (T)typeof(NebulaGame).GetField(name,Private).GetValue(game);}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup-started>90){Finish("Timeout");return;}
            if(!EditorApplication.isPlaying)return;
            var game=UnityEngine.Object.FindFirstObjectByType<NebulaGame>();
            if(game==null||EditorApplication.timeSinceStartup-started<2)return;
            try
            {
                Check(NebulaLocalization.EntryCount>500,"English resource not loaded");
                string savedJourney=PlayerPrefs.GetString("EcosDeNebula.Journey.v1","");
                Call(game,"ChangeLanguage","en");
                Check(PlayerPrefs.GetString(Key)=="en","English preference not persisted");
                Check(NebulaLocalization.Text("Cómo jugar")=="How to play","English menu translation missing");
                Call(game,"OpenInformation",false);
                Check(Field<bool>(game,"showHelp")&&!Field<bool>(game,"showAbout"),"Help opens wrong panel");
                string[] help=(string[])Call(game,"InformationContent");
                Check(help.Length==16&&string.Join(" ",help).Contains("WASD"),"Incomplete help");
                Check(help[0]=="1 · Listen and remember","English help missing");
                Call(game,"OpenInformation",true);
                string[] about=(string[])Call(game,"InformationContent");
                Check(Field<bool>(game,"showAbout"),"About mode not selected");
                Check(string.Join(" ",about).Contains("Josué Guerrero"),"About credits missing");
                Check(string.Join(" ",about)!=string.Join(" ",help),"Help and About are identical");
                typeof(NebulaGame).GetField("showHelp",Private).SetValue(game,false);
                Call(game,"BeginTrial");
                var trial=Field<PatternTrial>(game,"trial");
                var attempt=Field<PatternAttempt>(game,"attempt");
                string code=string.Join(",",trial.Answer);
                Call(game,"ChangeLanguage","es");
                Check(ReferenceEquals(trial,Field<PatternTrial>(game,"trial")),"Language switch replaced active trial");
                Check(ReferenceEquals(attempt,Field<PatternAttempt>(game,"attempt")),"Language switch reset answer progress");
                Check(code==string.Join(",",trial.Answer),"Language switch changed the solution");
                Check(PlayerPrefs.GetString("EcosDeNebula.Journey.v1","")==savedJourney,"Language switch changed campaign save");
                NebulaLocalization.SetLanguage("en");Call(game,"LoadLanguage");
                Check(NebulaLocalization.Language=="es","Reload did not restore saved Spanish preference");
                Call(game,"OpenInformation",false);
                Check(((string[])Call(game,"InformationContent"))[0]=="1 · Escucha y recuerda","Spanish help missing");
                trial.GeneratedByApi=true;trial.TextLanguage="en";
                Check((string)Call(game,"TrialText","An English title","Un nuevo eco")=="Un nuevo eco","Language switch leaves stale generated prose");
                Call(game,"ChangeLanguage","en");
                Check((string)Call(game,"TrialText","An English title","Un nuevo eco")=="An English title","Current-language generated prose not preserved");
                Finish(null);
            }
            catch(Exception ex){Finish(ex.ToString());}
        }
        static void Finish(string failure)
        {
            EditorApplication.update-=Tick;
            Directory.CreateDirectory("TestResults");
            string result=failure==null?"PASS: "+checks+" Unity localization checks; language preference restored.":"FAIL: "+failure;
            File.WriteAllText("TestResults/Unity-localization-result.txt",result);
            if(failure==null)Debug.Log(result);else Debug.LogError(result);
            SessionState.SetInt("Nebula.LocalizationTest.Exit",failure==null?0:1);
            if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;else RestoreAndExit();
        }
        static void OnMode(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Flag,false))RestoreAndExit();
        }
        static void RestoreAndExit()
        {
            foreach(string key in StringKeys)
                if(SessionState.GetBool("Nebula.LocalizationTest.Had."+key,false))PlayerPrefs.SetString(key,SessionState.GetString("Nebula.LocalizationTest.Value."+key,""));else PlayerPrefs.DeleteKey(key);
            foreach(string key in IntKeys)
                if(SessionState.GetBool("Nebula.LocalizationTest.Had."+key,false))PlayerPrefs.SetInt(key,SessionState.GetInt("Nebula.LocalizationTest.Value."+key,0));else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();SessionState.SetBool(Flag,false);
            EditorApplication.Exit(SessionState.GetInt("Nebula.LocalizationTest.Exit",1));
        }
    }
}
