using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EcosDeNebula
{
    [Serializable] public sealed class AIRequest
    {
        public int world,seal,length;
        public LearningProfile profile;
        public string language = "es";
    }
    [Serializable] sealed class LocalSession { public string url,token; }
    [Serializable] sealed class AIError { public string error; }

    public sealed class NebulaAIClient : MonoBehaviour
    {
        LocalSession session;
        UnityWebRequest active;
        public bool Configured {get{return session!=null;}}
        public string Status {get;private set;}
        public bool Busy {get;private set;}
        public void Configure()
        {
            Status="Modo local";
            if(Environment.GetEnvironmentVariable("NEBULA_DISABLE_AI")=="1")return;
            string url=Environment.GetEnvironmentVariable("NEBULA_AI_URL");
            string token=Environment.GetEnvironmentVariable("NEBULA_AI_TOKEN");
            if(!string.IsNullOrEmpty(url)) TrySession(new LocalSession{url=url,token=token});
            if(session==null)
            {
                // Resolve only project/launcher parents. The provider .env file is NEVER read here.
                DirectoryInfo folder=new DirectoryInfo(Application.dataPath);
                for(int i=0;i<4 && folder!=null;i++,folder=folder.Parent)
                {
                    string file=Path.Combine(folder.FullName,".nebula-session.json");
                    try {if(File.Exists(file)) {TrySession(JsonUtility.FromJson<LocalSession>(File.ReadAllText(file)));if(session!=null)break;}}
                    catch(Exception){ /* An absent or stale local session falls back to offline play. */ }
                }
            }
            if(session!=null)Status="API configurada";
        }
        void TrySession(LocalSession candidate)
        {
            Uri uri;
            if(candidate==null || candidate.token==null || candidate.token.Length!=64 ||
                !Uri.TryCreate(candidate.url,UriKind.Absolute,out uri) || uri.Scheme!="http" ||
                uri.Host!="127.0.0.1" || uri.AbsolutePath!="/" || uri.UserInfo!="" || uri.Query!="" || uri.Fragment!="")return;
            foreach(char c in candidate.token) if(!Uri.IsHexDigit(c))return;
            session=candidate;
        }
        public IEnumerator Generate(int world,int seal,LearningProfile profile,PatternTrial trial,Action<bool> completed)
        {
            if(session==null){completed(false);yield break;}
            Busy=true;Status="El Tejedor está creando...";
            AIRequest payload=new AIRequest{world=world,seal=Math.Min(seal,3),length=trial.Mode==PatternMode.Prediction?trial.MotifLength:trial.Demonstration.Length,profile=profile,language=NebulaLocalization.Language};
            using(UnityWebRequest request=new UnityWebRequest(session.url+"/challenge","POST"))
            {
                active=request;
                request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
                request.downloadHandler=new DownloadHandlerBuffer();request.timeout=23;
                request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Authorization","Bearer "+session.token);
                yield return request.SendWebRequest();
                bool success=false;
                if(request.result==UnityWebRequest.Result.Success)
                {
                    try {var result=JsonUtility.FromJson<GeneratedPattern>(request.downloadHandler.text);success=result!=null&&result.TryApply(trial);if(success)trial.TextLanguage=payload.language;}
                    catch(Exception){success=false;}
                    Status=success?"Generado por OpenAI":"Respuesta inválida · modo local";
                }
                else
                {
                    string code="connection";
                    try {var error=JsonUtility.FromJson<AIError>(request.downloadHandler.text);if(error!=null)code=error.error;}catch(Exception){}
                    Status=code=="quota_or_rate_limit"?"Cuota o límite API · modo local":
                        code=="missing_key"||code=="invalid_key"?"Clave no disponible · modo local":
                        code=="session_limit"?"Límite de sesión · modo local":
                        code=="cooldown"?"API en espera · modo local":"API no disponible · modo local";
                }
                active=null;Busy=false;completed(success);
            }
        }
        public void Cancel(){if(active!=null)active.Abort();Busy=false;}
        void OnDestroy(){Cancel();}
    }
}
