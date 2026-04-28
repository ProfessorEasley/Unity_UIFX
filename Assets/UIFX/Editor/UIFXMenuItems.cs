using UnityEditor;
using UnityEditor.SceneManagement;

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
        // ── Shimmer (Jing) ───────────────────────────────────────────────────
        [MenuItem("UIFX/Shimmer/Open Demo Scene")]
        static void OpenShimmerDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.ShimmerScene, UIFXStyledDemoBuilder.BuildShimmerScene);

        // ── Path Progress (Vedaant) ──────────────────────────────────────────
        [MenuItem("UIFX/Path Progress/Open Demo Scene")]
        static void OpenPathProgressDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.PathProgressScene, UIFXStyledDemoBuilder.BuildPathProgressScene);

        // ── Shine Shader (Sriram) ────────────────────────────────────────────
        [MenuItem("UIFX/Shine Shader/Open Demo Scene")]
        static void OpenShineShaderDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.ShineShaderScene, UIFXStyledDemoBuilder.BuildShineShaderScene);

        // ── Sparkle Particles (Aryan) ────────────────────────────────────────
        [MenuItem("UIFX/Sparkle Particles/Open Demo Scene")]
        static void OpenSparkleDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.SparkleParticlesScene, UIFXStyledDemoBuilder.BuildSparkleParticlesScene);

        // ── Coin Counter (Vasudha) ───────────────────────────────────────────
        [MenuItem("UIFX/Coin Counter/Open Demo Scene")]
        static void OpenCoinCounterDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.CoinCounterScene, UIFXStyledDemoBuilder.BuildCoinCounterScene);

        // ── Sprite Glow (Aryaman) ────────────────────────────────────────────
        [MenuItem("UIFX/Sprite Glow/Open Demo Scene")]
        static void OpenSpriteGlowDemo() =>
            OpenStyledScene(UIFXStyledDemoBuilder.SpriteGlowScene, UIFXStyledDemoBuilder.BuildSpriteGlowScene);

        // ── Dynamic Order (Naveen) ───────────────────────────────────────────
        [MenuItem("UIFX/Dynamic Order/Open Demo Scene")]
        static void OpenDynamicOrderDemo()
            => OpenStyledScene(UIFXStyledDemoBuilder.DynamicOrderScene, UIFXStyledDemoBuilder.BuildDynamicOrderScene);

        // ── Helpers ──────────────────────────────────────────────────────────

        static void OpenStyledScene(string assetPath, System.Action buildScene)
        {
            if (EditorApplication.isPlaying)
            {
                void OnStateChanged(PlayModeStateChange state)
                {
                    if (state != PlayModeStateChange.EnteredEditMode) return;
                    EditorApplication.playModeStateChanged -= OnStateChanged;
                    OpenStyledScene(assetPath, buildScene);
                }
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            buildScene();
            EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
        }
    }
}
