using UnityEditor;
using UnityEngine;

namespace UIFX.Editor
{
    [InitializeOnLoad]
    public static class UIShimmerSetup
    {
        private const string ShaderName   = "UIFX/UIShimmer";
        private const string MaterialPath = "Assets/UIFX/Runtime/Materials/UIShimmer.mat";

        static UIShimmerSetup()
        {
            // Defer until after asset database is ready
            EditorApplication.delayCall += CreateMaterial;
        }

        public static void CreateMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null)
                return;

            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[UIFX] Shader '{ShaderName}' not found. " +
                               "Make sure UIShimmer.shader compiled without errors.");
                return;
            }

            var mat = new Material(shader) { name = "UIShimmer" };
            mat.SetColor("_ShimmerColor",    Color.white);
            mat.SetFloat("_ShimmerProgress", -0.3f);
            mat.SetFloat("_ShimmerWidth",    0.15f);
            mat.SetFloat("_ShimmerAngle",    30f);
            mat.SetFloat("_Intensity",       1.5f);

            System.IO.Directory.CreateDirectory("Assets/UIFX/Runtime/Materials");
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UIFX] Created UIShimmer material at {MaterialPath}");
        }
    }
}
