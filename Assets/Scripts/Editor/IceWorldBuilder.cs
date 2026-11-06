#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IceWorldRunner.Editor
{
    public static class IceWorldBuilder
    {
        [MenuItem("Tools/Ice World Runner/Build Complete Playable Scene")]
        public static void BuildPlayableScene()
        {
            // Create a brand new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add the Bootstrap
            GameObject boot = new GameObject("=== ICE WORLD RUNNER BOOTSTRAP ===");
            GameBootstrap bootstrap = boot.AddComponent<GameBootstrap>();
            bootstrap.InitializeGame();

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            string scenePath = "Assets/Scenes/IceWorldRunnerScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.Refresh();

            // Add to Build Settings
            EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
            bool alreadyInBuild = false;
            foreach (var s in originalScenes)
            {
                if (s.path == scenePath) { alreadyInBuild = true; break; }
            }

            if (!alreadyInBuild)
            {
                EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
                for (int i = 0; i < originalScenes.Length; i++)
                {
                    newScenes[i] = originalScenes[i];
                }
                newScenes[originalScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
            }

            EditorUtility.DisplayDialog("Ice World Runner", 
                "Complete Playable Scene created successfully!\n\nLocation: " + scenePath + "\n\nPress PLAY to test the game now!", 
                "Let's Play!");
        }

        [MenuItem("Tools/Ice World Runner/Reset High Scores")]
        public static void ResetHighScores()
        {
            PlayerPrefs.DeleteKey(GameManager.HIGH_SCORE_KEY);
            PlayerPrefs.DeleteKey(GameManager.TOTAL_CRYSTALS_KEY);
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Scores Reset", "High scores have been reset to 0.", "OK");
        }
    }
}
#endif
