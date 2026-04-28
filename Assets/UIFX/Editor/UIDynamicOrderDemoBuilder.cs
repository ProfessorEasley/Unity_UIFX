using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UIFX.FX;

namespace UIFX.Editor
{
    public static class UIDynamicOrderDemoBuilder
    {
        const string ScenePath = "Assets/UIFX/Demo/Scenes/DynamicOrderDemo.unity";
        static readonly Vector2 RefResolution = new Vector2(1920, 1080);

        const float ItemWidth = 500f;
        const float ItemHeight = 70f;
        const float ItemSpacing = 14f;
        const float StartY = 210f;

        struct ItemConfig
        {
            public string label;
            public Color bgColor;
        }

        static readonly ItemConfig[] Items = new ItemConfig[]
        {
            new ItemConfig { label = "Alpha",   bgColor = new Color(0.18f, 0.13f, 0.13f) },
            new ItemConfig { label = "Beta",    bgColor = new Color(0.13f, 0.18f, 0.14f) },
            new ItemConfig { label = "Gamma",   bgColor = new Color(0.13f, 0.14f, 0.20f) },
            new ItemConfig { label = "Delta",   bgColor = new Color(0.18f, 0.16f, 0.11f) },
            new ItemConfig { label = "Epsilon", bgColor = new Color(0.15f, 0.13f, 0.19f) },
        };

        [MenuItem("UIFX/Dynamic Order/Create Demo Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            SetupCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            CreateLabel(canvas.transform, "Dynamic Ordering System",
                new Vector2(0, 440), new Vector2(800, 60), 36, Color.white, FontStyles.Bold);
            CreateLabel(canvas.transform, "Select an item, then choose a position",
                new Vector2(0, 390), new Vector2(700, 35), 20, new Color(0.6f, 0.6f, 0.6f));

            var systemRoot = new GameObject("DynamicOrderSystem");
            systemRoot.transform.SetParent(canvas.transform, false);
            systemRoot.AddComponent<RectTransform>();
            var system = systemRoot.AddComponent<UIDynamicOrderSystem>();

            var itemComponents = new UIDynamicOrderItem[Items.Length];
            for (int i = 0; i < Items.Length; i++)
            {
                float y = StartY - i * (ItemHeight + ItemSpacing);
                itemComponents[i] = CreateItem(systemRoot.transform, Items[i], i,
                    new Vector2(0, y), new Vector2(ItemWidth, ItemHeight));
            }

            WireSystem(system, itemComponents);

            CreateLabel(canvas.transform, "Move to position:",
                new Vector2(0, -230), new Vector2(300, 30), 18, new Color(0.55f, 0.55f, 0.55f));

            CreatePositionButtons(canvas.transform, system);
            CreateResetButton(canvas.transform, system);

            Directory.CreateDirectory("Assets/UIFX/Demo/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[UIFX] Dynamic Order demo scene saved to {ScenePath}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ScenePath));
        }

        static void SetupCamera()
        {
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;
            cam.backgroundColor = new Color(0.055f, 0.055f, 0.071f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputModuleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            if (inputModuleType != null)
                go.AddComponent(inputModuleType);
            else
                go.AddComponent<StandaloneInputModule>();
        }

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static void CreateBackground(Transform parent)
        {
            var go = new GameObject("Background");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.055f, 0.055f, 0.071f);
            img.raycastTarget = false;
        }

        static TextMeshProUGUI CreateLabel(
            Transform parent, string text,
            Vector2 anchoredPos, Vector2 size,
            float fontSize, Color color,
            FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(text.Length > 24 ? text.Substring(0, 24) : text);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return tmp;
        }

        static UIDynamicOrderItem CreateItem(
            Transform parent, ItemConfig cfg, int index,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject($"Item_{cfg.label}");
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var bg = go.AddComponent<Image>();
            bg.color = cfg.bgColor;

            var group = go.AddComponent<CanvasGroup>();
            var item = go.AddComponent<UIDynamicOrderItem>();

            // Highlight overlay
            var hlGo = new GameObject("Highlight");
            hlGo.transform.SetParent(go.transform, false);
            hlGo.layer = LayerMask.NameToLayer("UI");

            var hlRt = hlGo.AddComponent<RectTransform>();
            hlRt.anchorMin = Vector2.zero;
            hlRt.anchorMax = Vector2.one;
            hlRt.offsetMin = hlRt.offsetMax = Vector2.zero;

            var hlImg = hlGo.AddComponent<Image>();
            hlImg.color = Color.clear;
            hlImg.raycastTarget = false;

            // Index label
            var idxTmp = CreateLabel(go.transform, (index + 1).ToString(),
                new Vector2(-size.x * 0.5f + 30f, 0), new Vector2(40, 40),
                24, Color.white, FontStyles.Bold);
            idxTmp.alignment = TextAlignmentOptions.Center;

            // Content label
            var lblTmp = CreateLabel(go.transform, cfg.label,
                new Vector2(20f, 0), new Vector2(size.x - 100f, 36),
                20, new Color(0.85f, 0.85f, 0.85f));
            lblTmp.alignment = TextAlignmentOptions.Left;

            // Wire serialized fields
            var so = new SerializedObject(item);
            so.FindProperty("background").objectReferenceValue = bg;
            so.FindProperty("highlightOverlay").objectReferenceValue = hlImg;
            so.FindProperty("indexLabel").objectReferenceValue = idxTmp;
            so.FindProperty("contentLabel").objectReferenceValue = lblTmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        static void WireSystem(UIDynamicOrderSystem system, UIDynamicOrderItem[] itemComponents)
        {
            var so = new SerializedObject(system);

            var itemsProp = so.FindProperty("items");
            itemsProp.arraySize = itemComponents.Length;
            for (int i = 0; i < itemComponents.Length; i++)
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemComponents[i];

            so.FindProperty("itemHeight").floatValue = ItemHeight;
            so.FindProperty("itemSpacing").floatValue = ItemSpacing;
            so.FindProperty("startY").floatValue = StartY;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CreatePositionButtons(Transform canvasParent, UIDynamicOrderSystem system)
        {
            string[] methods =
            {
                "OnPositionButton1", "OnPositionButton2", "OnPositionButton3",
                "OnPositionButton4", "OnPositionButton5"
            };

            const float btnW = 65f;
            const float btnH = 50f;
            const float gap = 12f;
            float totalW = Items.Length * btnW + (Items.Length - 1) * gap;
            float startX = -totalW * 0.5f + btnW * 0.5f;

            for (int i = 0; i < Items.Length; i++)
            {
                float x = startX + i * (btnW + gap);
                CreateWiredButton(canvasParent, (i + 1).ToString(),
                    new Vector2(x, -280), new Vector2(btnW, btnH),
                    new Color(0.20f, 0.20f, 0.25f),
                    new Color(0.30f, 0.30f, 0.38f),
                    new Color(0.14f, 0.14f, 0.18f),
                    system, methods[i]);
            }
        }

        static void CreateResetButton(Transform canvasParent, UIDynamicOrderSystem system)
        {
            CreateWiredButton(canvasParent, "Reset",
                new Vector2(0, -360), new Vector2(160, 45),
                new Color(0.22f, 0.14f, 0.14f),
                new Color(0.34f, 0.20f, 0.20f),
                new Color(0.16f, 0.10f, 0.10f),
                system, "ResetOrder");
        }

        static void CreateWiredButton(
            Transform parent, string label,
            Vector2 pos, Vector2 size,
            Color normalCol, Color hoverCol, Color pressCol,
            Object target, string methodName)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = normalCol;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = normalCol;
            colors.highlightedColor = hoverCol;
            colors.pressedColor = pressCol;
            colors.selectedColor = hoverCol;
            btn.colors = colors;

            var btnSo = new SerializedObject(btn);
            var onClick = btnSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            onClick.arraySize = 1;
            var call = onClick.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = target;
            call.FindPropertyRelative("m_MethodName").stringValue = methodName;
            call.FindPropertyRelative("m_Mode").intValue = 1;
            call.FindPropertyRelative("m_CallState").intValue = 2;
            btnSo.ApplyModifiedPropertiesWithoutUndo();

            CreateLabel(go.transform, label,
                Vector2.zero, size,
                18, Color.white, FontStyles.Bold);
        }
    }
}
