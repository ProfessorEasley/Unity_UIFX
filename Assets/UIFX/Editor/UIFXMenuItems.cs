using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UIFX.Editor
{
    /// <summary>
    /// Registers UIFX top-level menu entries for every team member's feature.
    /// Each entry opens that feature's demo scene so it is immediately playable.
    ///
    /// Shimmer entries live in UIShimmerSetup / UIShimmerDemoBuilder under UIFX/Shimmer/.
    /// </summary>
    public static class UIFXMenuItems
    {
        // ── Path Progress (Vedaant) ──────────────────────────────────────────
        [MenuItem("UIFX/Path Progress/Open Demo Scene")]
        static void OpenPathProgressDemo() =>
            OpenScene("Assets/UIFX/Demo/Scenes/Vedaant_Demo.unity");

        // ── Shine Shader (Sriram) ────────────────────────────────────────────
        [MenuItem("UIFX/Shine Shader/Open Demo Scene")]
        static void OpenShineShaderDemo() =>
            OpenScene("Assets/SriramShineShader/Scenes/SampleScene.unity");

        // ── Sparkle Particles (Aryan) ────────────────────────────────────────
        [MenuItem("UIFX/Sparkle Particles/Open Demo Scene")]
        static void OpenSparkleDemo() =>
            OpenScene("Assets/Aryan Particle System/Scenes/SampleScene.unity");

        // ── Coin Counter (Vasudha) ───────────────────────────────────────────
        [MenuItem("UIFX/Coin Counter/Open Demo Scene")]
        static void OpenCoinCounterDemo() =>
            OpenScene("Assets/VasudhaCoinCounter/CoinCounterPrefab.unity");

        // ── Sprite Glow (Aryaman) ────────────────────────────────────────────
        [MenuItem("UIFX/Sprite Glow/Open Demo Scene")]
        static void OpenSpriteGlowDemo() =>
            OpenScene("Assets/Scenes/TestBehaviour.unity");

        // ── Dynamic Order (Naveen) ───────────────────────────────────────────
        [MenuItem("UIFX/Dynamic Order/Open Demo Scene")]
        static void OpenDynamicOrderDemo() =>
            OpenScene("Assets/UIFX/Demo/Scenes/DynamicOrderDemo.unity");

        // ── Helpers ──────────────────────────────────────────────────────────

        static void OpenScene(string assetPath)
        {
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
            {
                Debug.LogWarning($"[UIFX] Scene not found: {assetPath}");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                // Exit play mode first; load the scene once the editor is back in edit mode.
                void OnStateChanged(PlayModeStateChange state)
                {
                    if (state != PlayModeStateChange.EnteredEditMode) return;
                    EditorApplication.playModeStateChanged -= OnStateChanged;
                    LoadScene(assetPath);
                }
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.isPlaying = false;
            }
            else
            {
                LoadScene(assetPath);
            }
        }

        static void LoadScene(string assetPath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
        }
    }
}
