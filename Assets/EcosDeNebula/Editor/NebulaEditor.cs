using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDeNebula.Editor
{
    [InitializeOnLoad]
    public static class NebulaEditor
    {
        const string ScenePath = "Assets/EcosDeNebula/Scenes/Nebula.unity";
        static NebulaEditor()
        {
            EditorApplication.delayCall += delegate
            {
                if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
                Configure();
                if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path) && !SceneManager.GetActiveScene().isDirty)
                    EditorSceneManager.OpenScene(ScenePath);
            };
        }

        [MenuItem("Nébula/Abrir aventura")]
        public static void OpenAdventure()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Nébula/Preparar proyecto")]
        public static void Configure()
        {
            PlayerSettings.companyName = "Nebula Atelier";
            PlayerSettings.productName = "Ecos de Nebula";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            // The runtime client permits ONLY 127.0.0.1; the relay uses HTTPS to OpenAI.
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            // Runtime-generated materials still need their shaders preserved in a player build.
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets.Length > 0)
            {
                SerializedObject graphics = new SerializedObject(assets[0]);
                SerializedProperty shaders = graphics.FindProperty("m_AlwaysIncludedShaders");
                if (shaders != null)
                {
                    foreach (string shaderName in new[] { "Standard", "Nebula/GradientSky" })
                    {
                        Shader shader = Shader.Find(shaderName);
                        if (shader == null) throw new Exception("Shader missing: " + shaderName);
                        bool found = false;
                        for (int i = 0; i < shaders.arraySize; i++) if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader) found = true;
                        if (!found) { shaders.InsertArrayElementAtIndex(shaders.arraySize); shaders.GetArrayElementAtIndex(shaders.arraySize - 1).objectReferenceValue = shader; }
                    }
                    graphics.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Nébula/Crear ejecutable de Windows")]
        public static void BuildWindows()
        {
            Configure();
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/EcosDeNebula.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
            Debug.Log("NEBULA_BUILD_OK " + report.summary.totalSize + " bytes");
        }

        // Can be run with -batchmode -executeMethod EcosDeNebula.Editor.NebulaEditor.Validate -quit.
        public static void Validate()
        {
            Configure();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (scene.GetRootGameObjects().Length != 1 || scene.GetRootGameObjects()[0].GetComponent<NebulaGame>() == null)
                throw new Exception("The entry scene must contain the game component.");
            for (int world=0;world<3;world++)
            {
                var ai = new PatternDirector(new LearningProfile(), 123);
                var trial = ai.Create(world,0);
                var attempt = new PatternAttempt(trial.Answer);
                foreach (int symbol in trial.Answer) if (!attempt.Submit(symbol)) throw new Exception("Pattern could not be solved.");
                if (!attempt.Complete) throw new Exception("Completion not recognized.");
            }
            Debug.Log("NEBULA_VALIDATION_OK: scripts, shaders, scene and three pattern modes.");
        }
    }
}
