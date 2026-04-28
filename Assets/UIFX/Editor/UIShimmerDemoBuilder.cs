using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UIFX.Demo;
using UIFX.FX;

namespace UIFX.Editor
{
    /// <summary>
    /// Builds the UIShimmer effect demo scene programmatically.
    /// Run via:  UIFX > Create Shimmer Demo Scene
    ///
    /// Prerequisites: run "UIFX > Create UIShimmer Material" first.
    /// The builder will offer to do this automatically if the material is missing.
    /// </summary>
    public static class UIShimmerDemoBuilder
    {
        private const string ScenePath    = "Assets/UIFX/Demo/Scenes/UIShimmerDemo.unity";
        private const string MaterialPath = "Assets/UIFX/Runtime/Materials/UIShimmer.mat";

        // Reference resolution for CanvasScaler — layout values below are in these units
        private static readonly Vector2 RefResolution = new Vector2(1920, 1080);

        // ──────────────────────────────────────────────────────────────
        // Card definitions
        // ──────────────────────────────────────────────────────────────

        private struct CardConfig
        {
            public string label;
            public string detail;          // shown as small subtitle on the card
            public Color  cardColor;       // dark background so additive shimmer pops
            public Color  shimmerColor;
            public float  angle;
            public float  width;
            public float  intensity;
            public float  duration;
        }

        private static readonly CardConfig[] Cards = new CardConfig[]
        {
            new CardConfig
            {
                label = "Classic",   detail = "30°  ·  w 0.15  ·  1.5×",
                cardColor    = new Color(0.094f, 0.125f, 0.157f),
                shimmerColor = Color.white,
                angle = 30f, width = 0.15f, intensity = 1.5f, duration = 0.6f,
            },
            new CardConfig
            {
                label = "Gold Glint",   detail = "25°  ·  w 0.10  ·  2.0×",
                cardColor    = new Color(0.122f, 0.082f, 0.031f),
                shimmerColor = new Color(1.0f, 0.84f, 0.0f),
                angle = 25f, width = 0.10f, intensity = 2.0f, duration = 0.45f,
            },
            new CardConfig
            {
                label = "Wide Glow",   detail = "0°  ·  w 0.28  ·  1.1×",
                cardColor    = new Color(0.031f, 0.094f, 0.118f),
                shimmerColor = new Color(0.0f, 1.0f, 1.0f),
                angle = 0f, width = 0.28f, intensity = 1.1f, duration = 0.8f,
            },
            new CardConfig
            {
                label = "Sharp Cut",   detail = "60°  ·  w 0.06  ·  2.2×",
                cardColor    = new Color(0.094f, 0.031f, 0.094f),
                shimmerColor = Color.white,
                angle = 60f, width = 0.06f, intensity = 2.2f, duration = 0.4f,
            },
            new CardConfig
            {
                label = "Slow Bloom",   detail = "20°  ·  w 0.35  ·  0.9×",
                cardColor    = new Color(0.051f, 0.051f, 0.122f),
                shimmerColor = new Color(0.8f, 0.6f, 1.0f),
                angle = 20f, width = 0.35f, intensity = 0.9f, duration = 1.3f,
            },
            new CardConfig
            {
                label = "Quick Flash",   detail = "45°  ·  w 0.12  ·  2.5×",
                cardColor    = new Color(0.031f, 0.094f, 0.031f),
                shimmerColor = new Color(0.5f, 1.0f, 0.0f),
                angle = 45f, width = 0.12f, intensity = 2.5f, duration = 0.25f,
            },
        };

        // ──────────────────────────────────────────────────────────────
        // Entry point
        // ──────────────────────────────────────────────────────────────

        public static void Build()
        {
            UIFXStyledDemoBuilder.BuildShimmerScene();
        }

        static void BuildLegacyScene()
        {
            var shimmerMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            if (shimmerMat == null)
            {
                bool create = EditorUtility.DisplayDialog(
                    "UIShimmer Material Missing",
                    $"'{MaterialPath}' not found.\nCreate it now?",
                    "Create", "Cancel");

                if (!create) return;

                UIShimmerSetup.CreateMaterial();
                shimmerMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

                if (shimmerMat == null)
                {
                    Debug.LogError("[UIFX] Could not create UIShimmer material. Aborting demo build.");
                    return;
                }
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            SetupCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            CreateBackground(canvas.transform);
            CreateLabel(canvas.transform, "UIShimmer Effect Demo",
                new Vector2(0, 230), new Vector2(900, 70), 38, Color.white, FontStyles.Bold);
            CreateLabel(canvas.transform, "Click any card  ·  or use Trigger All",
                new Vector2(0, 162), new Vector2(700, 40), 20, new Color(0.65f, 0.65f, 0.65f));

            var cardObjects = CreateCardGrid(canvas.transform, shimmerMat);
            CreateTriggerAllButton(canvas.transform, cardObjects, shimmerMat);

            Directory.CreateDirectory("Assets/UIFX/Demo/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[UIFX] Demo scene saved to {ScenePath}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ScenePath));
        }

        // ──────────────────────────────────────────────────────────────
        // Scene setup helpers
        // ──────────────────────────────────────────────────────────────

        static void SetupCamera()
        {
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;
            cam.backgroundColor = new Color(0.055f, 0.055f, 0.071f);
            cam.clearFlags      = CameraClearFlags.SolidColor;
        }

        static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            // Prefer InputSystemUIInputModule when the Input System package is present
            var inputModuleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            if (inputModuleType != null)
                go.AddComponent(inputModuleType);
            else
                go.AddComponent<StandaloneInputModule>();
        }

        static Canvas CreateCanvas()
        {
            var go     = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefResolution;
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ──────────────────────────────────────────────────────────────
        // UI element builders
        // ──────────────────────────────────────────────────────────────

        static void CreateBackground(Transform parent)
        {
            var go = new GameObject("Background");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img   = go.AddComponent<Image>();
            img.color = new Color(0.055f, 0.055f, 0.071f);
        }

        static TextMeshProUGUI CreateLabel(
            Transform parent, string text,
            Vector2 anchoredPos, Vector2 size,
            float fontSize, Color color,
            FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(text.Length > 20 ? text.Substring(0, 20) : text);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text       = text;
            tmp.fontSize   = fontSize;
            tmp.color      = color;
            tmp.fontStyle  = style;
            tmp.alignment  = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return tmp;
        }

        // ──────────────────────────────────────────────────────────────
        // Card grid
        // ──────────────────────────────────────────────────────────────

        static UIShimmerEffect[] CreateCardGrid(Transform parent, Material shimmerMat)
        {
            const float cardW   = 220f;
            const float cardH   = 150f;
            const float hGap    = 30f;
            const float vGap    = 30f;
            const int   columns = 3;

            float colStep = cardW + hGap;
            float rowStep = cardH + vGap;

            // Centre the 3-column grid horizontally
            float gridLeft = -(columns - 1) * colStep * 0.5f;

            var effects = new UIShimmerEffect[Cards.Length];

            for (int i = 0; i < Cards.Length; i++)
            {
                int col = i % columns;
                int row = i / columns;

                float x = gridLeft + col * colStep;
                float y = 60f - row * rowStep;     // top row near subtitle

                effects[i] = CreateCard(parent, Cards[i], shimmerMat,
                    new Vector2(x, y), new Vector2(cardW, cardH));
            }

            return effects;
        }

        static UIShimmerEffect CreateCard(
            Transform parent, CardConfig cfg, Material shimmerMat,
            Vector2 anchoredPos, Vector2 size)
        {
            // Root: background image + shimmer effect
            var go = new GameObject(cfg.label);
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;

            var img   = go.AddComponent<Image>();
            img.color = cfg.cardColor;

            var fx = go.AddComponent<UIShimmerEffect>();
            // Assign via SerializedObject so Unity tracks the asset reference
            var so   = new SerializedObject(fx);
            var prop = so.FindProperty("shimmerMaterialSource");
            prop.objectReferenceValue = shimmerMat;
            so.FindProperty("shimmerColor").colorValue   = cfg.shimmerColor;
            so.FindProperty("shimmerAngle").floatValue   = cfg.angle;
            so.FindProperty("shimmerWidth").floatValue   = cfg.width;
            so.FindProperty("intensity").floatValue      = cfg.intensity;
            so.FindProperty("duration").floatValue       = cfg.duration;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Label
            CreateLabel(go.transform, cfg.label,
                new Vector2(0, 28), new Vector2(size.x - 20, 36),
                17, Color.white, FontStyles.Bold);

            // Detail line (angle · width · intensity)
            CreateLabel(go.transform, cfg.detail,
                new Vector2(0, -20), new Vector2(size.x - 20, 28),
                13, new Color(0.55f, 0.55f, 0.55f));

            // Shimmer colour swatch — small square at bottom-right
            CreateSwatch(go.transform, cfg.shimmerColor,
                new Vector2(size.x * 0.5f - 24, -size.y * 0.5f + 16),
                new Vector2(16, 16));

            return fx;
        }

        static void CreateSwatch(Transform parent, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Swatch");
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;

            var img = go.AddComponent<Image>();
            img.color         = color;
            img.raycastTarget = false;
        }

        // ──────────────────────────────────────────────────────────────
        // "Trigger All" button
        // ──────────────────────────────────────────────────────────────

        static void CreateTriggerAllButton(
            Transform canvasParent, UIShimmerEffect[] cards, Material shimmerMat)
        {
            // Container root — holds the controller script
            var root = new GameObject("DemoController");
            root.transform.SetParent(canvasParent, false);
            root.AddComponent<RectTransform>();

            var controller = root.AddComponent<UIShimmerDemoController>();
            var so         = new SerializedObject(controller);
            var cardsProp  = so.FindProperty("cards");
            cardsProp.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++)
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // Button background
            var btnGo = new GameObject("TriggerAllButton");
            btnGo.transform.SetParent(canvasParent, false);
            btnGo.layer = LayerMask.NameToLayer("UI");

            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -285);
            rt.sizeDelta        = new Vector2(220, 52);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.22f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor      = new Color(0.18f, 0.18f, 0.22f);
            colors.highlightedColor = new Color(0.26f, 0.26f, 0.34f);
            colors.pressedColor     = new Color(0.12f, 0.12f, 0.16f);
            btn.colors = colors;

            // Wire onClick → DemoController.TriggerAll via SerializedObject
            var btnSo       = new SerializedObject(btn);
            var onClick     = btnSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            onClick.arraySize = 1;
            var call = onClick.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = controller;
            call.FindPropertyRelative("m_MethodName").stringValue      = "TriggerAll";
            call.FindPropertyRelative("m_Mode").intValue               = 1;  // Void
            call.FindPropertyRelative("m_CallState").intValue          = 2;  // RuntimeOnly
            btnSo.ApplyModifiedPropertiesWithoutUndo();

            // Also shimmer the button itself on click
            var btnFx = btnGo.AddComponent<UIShimmerEffect>();
            var fxSo  = new SerializedObject(btnFx);
            fxSo.FindProperty("shimmerMaterialSource").objectReferenceValue = shimmerMat;
            fxSo.FindProperty("shimmerColor").colorValue  = Color.white;
            fxSo.FindProperty("shimmerAngle").floatValue  = 30f;
            fxSo.FindProperty("shimmerWidth").floatValue  = 0.2f;
            fxSo.FindProperty("intensity").floatValue     = 1.8f;
            fxSo.FindProperty("duration").floatValue      = 0.5f;
            fxSo.ApplyModifiedPropertiesWithoutUndo();

            CreateLabel(btnGo.transform, "Trigger All",
                Vector2.zero, new Vector2(200, 52),
                18, Color.white, FontStyles.Bold);
        }
    }
}
