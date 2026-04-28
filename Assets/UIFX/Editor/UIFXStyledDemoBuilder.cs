using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIFX.Demo;
using UIFX.FX;

namespace UIFX.Editor
{
    public static class UIFXStyledDemoBuilder
    {
        public const string ShimmerScene = "Assets/UIFX/Demo/Scenes/UIShimmerDemo.unity";
        public const string PathProgressScene = "Assets/UIFX/Demo/Scenes/PathProgressDemo.unity";
        public const string ShineShaderScene = "Assets/UIFX/Demo/Scenes/ShineShaderDemo.unity";
        public const string SparkleParticlesScene = "Assets/UIFX/Demo/Scenes/SparkleParticlesDemo.unity";
        public const string CoinCounterScene = "Assets/UIFX/Demo/Scenes/CoinCounterDemo.unity";
        public const string SpriteGlowScene = "Assets/UIFX/Demo/Scenes/SpriteGlowDemo.unity";
        public const string DynamicOrderScene = "Assets/UIFX/Demo/Scenes/DynamicOrderDemo.unity";

        const string ShimmerMaterialPath = "Assets/UIFX/Runtime/Materials/UIShimmer.mat";
        const string ShineMaterialPath = "Assets/SriramShineShader/Diag_UI_ShinyButton_DiagonalFlash.mat";

        static readonly Vector2 RefResolution = new Vector2(1280f, 720f);
        static readonly Color StageBg = new Color(0.045f, 0.065f, 0.09f);
        static readonly Color Panel = new Color(0.075f, 0.105f, 0.145f);
        static readonly Color MutedText = new Color(0.66f, 0.72f, 0.80f);
        static readonly Color Line = new Color(0.18f, 0.25f, 0.34f);

        [MenuItem("UIFX/Rebuild Styled Demo Scenes")]
        public static void BuildAll()
        {
            UIShimmerSetup.CreateMaterial();
            Directory.CreateDirectory("Assets/UIFX/Demo/Scenes");

            BuildShimmerScene();
            BuildPathProgressScene();
            BuildShineShaderScene();
            BuildSparkleParticlesScene();
            BuildCoinCounterScene();
            BuildSpriteGlowScene();
            BuildDynamicOrderScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIFX] Rebuilt styled demo scenes.");
        }

        public static void BuildShimmerScene()
        {
            var canvas = BeginScene("UIShimmer", "click-triggered card sweep");
            var shimmerMat = AssetDatabase.LoadAssetAtPath<Material>(ShimmerMaterialPath);

            var cards = new[]
            {
                new ShimmerCard("Classic", "30 deg / narrow", new Color(0.10f, 0.14f, 0.20f), Color.white, 30f, 0.15f, 1.55f),
                new ShimmerCard("Gold Glint", "gold / fast", new Color(0.30f, 0.19f, 0.04f), new Color(1f, 0.84f, 0.20f), 25f, 0.10f, 2.05f),
                new ShimmerCard("Wide Glow", "wide cyan glow", new Color(0.05f, 0.25f, 0.27f), new Color(0.45f, 1f, 1f), 0f, 0.28f, 1.15f),
                new ShimmerCard("Sharp Cut", "sharp diagonal", new Color(0.25f, 0.09f, 0.25f), Color.white, 60f, 0.06f, 2.2f),
                new ShimmerCard("Slow Bloom", "slow bloom", new Color(0.12f, 0.13f, 0.33f), new Color(0.70f, 0.55f, 1f), 20f, 0.35f, 0.95f),
                new ShimmerCard("Quick Flash", "quick flash", new Color(0.06f, 0.25f, 0.12f), new Color(0.55f, 1f, 0.34f), 45f, 0.12f, 2.5f),
            };

            var effects = new UIShimmerEffect[cards.Length];
            Vector2 start = new Vector2(-330f, 125f);
            for (int i = 0; i < cards.Length; i++)
            {
                int col = i % 3;
                int row = i / 3;
                effects[i] = CreateShimmerCard(canvas, cards[i], shimmerMat,
                    start + new Vector2(col * 260f, -row * 145f));
            }

            CreateCommandButton(canvas, "Trigger All", new Vector2(0f, -252f), new Vector2(220f, 50f),
                new Color(0.11f, 0.18f, 0.24f), new Color(0.42f, 0.82f, 1f));

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.Shimmer);
            var so = new SerializedObject(controller);
            var cardsProp = so.FindProperty("shimmerEffects");
            cardsProp.arraySize = effects.Length;
            for (int i = 0; i < effects.Length; i++)
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = effects[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(ShimmerScene);
        }

        public static void BuildPathProgressScene()
        {
            var canvas = BeginScene("UIPathProgress", "distance-normalized node path");
            var points = new[]
            {
                new Vector2(-470f, -110f),
                new Vector2(-305f, 45f),
                new Vector2(-125f, -35f),
                new Vector2(70f, 120f),
                new Vector2(260f, 30f),
                new Vector2(470f, 150f),
                new Vector2(545f, -95f),
            };

            var fills = new RectTransform[points.Length - 1];
            var lengths = new float[points.Length - 1];
            for (int i = 0; i < points.Length - 1; i++)
            {
                fills[i] = CreatePathSegment(canvas, points[i], points[i + 1], out lengths[i]);
                fills[i].sizeDelta = new Vector2(lengths[i] * 0.28f, fills[i].sizeDelta.y);
            }

            var nodeFills = new Image[points.Length];
            for (int i = 0; i < points.Length; i++)
                nodeFills[i] = CreatePathNode(canvas, points[i], i + 1, i < 2);

            var head = CreateGlowDot(canvas, "LeadHead", new Vector2(-350f, 5f), 22f, new Color(0.38f, 1f, 0.86f));
            CreateRangeSlider(canvas, new Vector2(0f, -270f));

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.PathProgress);
            var so = new SerializedObject(controller);
            AssignArray(so.FindProperty("pathFillRects"), fills);
            AssignArray(so.FindProperty("pathNodeFills"), nodeFills);
            AssignVectorArray(so.FindProperty("pathPoints"), points);
            AssignFloatArray(so.FindProperty("pathLengths"), lengths);
            so.FindProperty("pathHead").objectReferenceValue = head;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(PathProgressScene);
        }

        public static void BuildShineShaderScene()
        {
            var canvas = BeginScene("Button Shine/Ripple", "hover shine and click ripple");
            var material = AssetDatabase.LoadAssetAtPath<Material>(ShineMaterialPath);

            var button = CreateImage(canvas, "ShineButton", new Vector2(0f, 15f), new Vector2(570f, 190f),
                new Color(0.09f, 0.13f, 0.18f));
            button.raycastTarget = true;
            button.material = material;
            button.gameObject.AddComponent<ButtonHoverRipple>();
            AddOutline(button.gameObject, new Color(1f, 0.68f, 0.18f), new Vector2(3f, -3f));
            AddShadow(button.gameObject, new Color(1f, 0.55f, 0.10f, 0.45f), new Vector2(0f, -7f));

            var ripple = CreateImage(button.transform, "RipplePreview", new Vector2(185f, -25f), Vector2.one * 70f,
                new Color(0.46f, 0.84f, 1f, 0.24f));
            AddOutline(ripple.gameObject, new Color(0.52f, 0.88f, 1f, 0.7f), new Vector2(3f, -3f));
            ripple.raycastTarget = false;

            CreateLabel(button.transform, "Play Now", new Vector2(0f, 28f), new Vector2(420f, 48f),
                38f, Color.white, FontStyles.Bold);
            CreateLabel(button.transform, "diagonal shine material + ripple pulse", new Vector2(0f, -28f), new Vector2(470f, 30f),
                17f, MutedText);

            var cursor = CreateCursor(canvas, new Vector2(185f, -135f));

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.ShineShader);
            var so = new SerializedObject(controller);
            so.FindProperty("shineButton").objectReferenceValue = button;
            so.FindProperty("shineRipple").objectReferenceValue = ripple;
            so.FindProperty("shineCursor").objectReferenceValue = cursor;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(ShineShaderScene);
        }

        public static void BuildSparkleParticlesScene()
        {
            var canvas = BeginScene("Sparkle Particles", "UGUI particle collection");

            var percent = CreateLabel(canvas, "14%", new Vector2(0f, -18f), new Vector2(220f, 48f),
                40f, Color.white, FontStyles.Bold);

            var barRoot = CreateImage(canvas, "FillBar", new Vector2(0f, -85f), new Vector2(760f, 44f),
                new Color(0.11f, 0.15f, 0.20f));
            AddOutline(barRoot.gameObject, Line, new Vector2(2f, -2f));

            var fill = CreateImage(barRoot.transform, "Fill", Vector2.zero, new Vector2(760f, 44f),
                new Color(0.14f, 0.82f, 1f));
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0.14f;

            var particleRoot = new GameObject("SparkleLoop", typeof(RectTransform));
            particleRoot.transform.SetParent(canvas, false);
            var rootRt = particleRoot.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

            var sparkle = particleRoot.AddComponent<UISparkleLoop>();
            sparkle.sparkleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sparkle.healthBarFill = fill;
            sparkle.poolSize = 60;
            sparkle.emissionRate = 28f;
            sparkle.flySpeed = 420f;
            sparkle.wobble = 46f;
            sparkle.minSize = 7f;
            sparkle.maxSize = 22f;
            sparkle.fillPerClick = 0.18f;
            sparkle.fillPerParticle = 0.015f;
            sparkle.sparkleColor = new Color(0.38f, 1f, 0.86f);

            CreateSwatches(canvas, new Vector2(-120f, -145f));

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.SparkleParticles);
            var so = new SerializedObject(controller);
            so.FindProperty("sparkleLoop").objectReferenceValue = sparkle;
            so.FindProperty("sparkleBarFill").objectReferenceValue = fill;
            so.FindProperty("sparklePercentLabel").objectReferenceValue = percent;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(SparkleParticlesScene);
        }

        public static void BuildCoinCounterScene()
        {
            var canvas = BeginScene("Animated Coin Counter", "ease-out number punch");

            var panel = CreateImage(canvas, "CoinPanel", Vector2.zero, new Vector2(600f, 250f),
                new Color(0.08f, 0.13f, 0.19f));
            AddOutline(panel.gameObject, new Color(0.26f, 0.34f, 0.43f), new Vector2(2f, -2f));
            AddShadow(panel.gameObject, new Color(1f, 0.74f, 0.18f, 0.30f), new Vector2(0f, -9f));

            var coinIcon = CreateCoin(panel.transform, new Vector2(-155f, 0f), 62f);
            var value = CreateLabel(panel.transform, "2,614", new Vector2(65f, 2f), new Vector2(330f, 72f),
                60f, new Color(1f, 0.96f, 0.78f), FontStyles.Bold);
            value.alignment = TextAlignmentOptions.Left;

            var counter = panel.gameObject.AddComponent<UIFX_CoinCounter>();
            var counterSo = new SerializedObject(counter);
            counterSo.FindProperty("valueText").objectReferenceValue = value;
            counterSo.FindProperty("target").objectReferenceValue = panel.rectTransform;
            counterSo.FindProperty("duration").floatValue = 0.8f;
            counterSo.FindProperty("punchScale").floatValue = 1.10f;
            counterSo.ApplyModifiedPropertiesWithoutUndo();

            var burstCoins = new RectTransform[18];
            for (int i = 0; i < burstCoins.Length; i++)
                burstCoins[i] = CreateCoin(canvas, new Vector2(-430f + (i % 7) * 120f, -300f), 16f + (i % 3) * 3f).rectTransform;

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.CoinCounter);
            var so = new SerializedObject(controller);
            so.FindProperty("coinCounter").objectReferenceValue = counter;
            so.FindProperty("coinTarget").objectReferenceValue = panel.rectTransform;
            AssignArray(so.FindProperty("coinBursts"), burstCoins);
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(CoinCounterScene);
        }

        public static void BuildSpriteGlowScene()
        {
            var canvas = BeginScene("Sprite Glow Outline", "pulsing sprite outline");

            string[] labels = { "Cyan", "Green", "Gold", "Pink" };
            string[] glyphs = { "◇", "□", "★", "○" };
            Color[] colors =
            {
                new Color(0.20f, 0.85f, 1f),
                new Color(0.45f, 1f, 0.52f),
                new Color(1f, 0.82f, 0.30f),
                new Color(1f, 0.38f, 0.78f),
            };

            var targets = new RectTransform[labels.Length];
            var glows = new Graphic[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                float x = -300f + i * 200f;
                var glow = CreateLabel(canvas, glyphs[i], new Vector2(x, 15f), new Vector2(150f, 150f),
                    94f, new Color(colors[i].r, colors[i].g, colors[i].b, 0.48f), FontStyles.Bold);
                AddShadow(glow.gameObject, colors[i], new Vector2(0f, 0f));
                var shape = CreateLabel(canvas, glyphs[i], new Vector2(x, 15f), new Vector2(130f, 130f),
                    74f, colors[i], FontStyles.Bold);
                CreateLabel(canvas, labels[i], new Vector2(x, -112f), new Vector2(150f, 28f),
                    16f, MutedText, FontStyles.Bold);
                targets[i] = glow.rectTransform;
                glows[i] = glow;
            }

            CreateGlowMeter(canvas, new Vector2(-310f, -205f), colors);

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.SpriteGlow);
            var so = new SerializedObject(controller);
            AssignArray(so.FindProperty("glowTargets"), targets);
            AssignArray(so.FindProperty("glowGraphics"), glows);
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(SpriteGlowScene);
        }

        public static void BuildDynamicOrderScene()
        {
            var canvas = BeginScene("Dynamic Ordering System", "animated list reordering");

            var systemRoot = new GameObject("DynamicOrderSystem", typeof(RectTransform));
            systemRoot.transform.SetParent(canvas, false);
            var system = systemRoot.AddComponent<UIDynamicOrderSystem>();

            var items = new[]
            {
                new DynamicItem("Alpha", new Color(0.36f, 0.20f, 0.22f)),
                new DynamicItem("Beta", new Color(0.20f, 0.36f, 0.25f)),
                new DynamicItem("Gamma", new Color(0.20f, 0.26f, 0.42f)),
                new DynamicItem("Delta", new Color(0.40f, 0.32f, 0.16f)),
                new DynamicItem("Epsilon", new Color(0.32f, 0.22f, 0.40f)),
            };

            var components = new UIDynamicOrderItem[items.Length];
            for (int i = 0; i < items.Length; i++)
                components[i] = CreateDynamicItem(systemRoot.transform, items[i], i, new Vector2(0f, 170f - i * 84f));

            var systemSo = new SerializedObject(system);
            AssignArray(systemSo.FindProperty("items"), components);
            systemSo.FindProperty("itemHeight").floatValue = 70f;
            systemSo.FindProperty("itemSpacing").floatValue = 14f;
            systemSo.FindProperty("startY").floatValue = 170f;
            systemSo.ApplyModifiedPropertiesWithoutUndo();

            CreateLabel(canvas, "Move to position:", new Vector2(0f, -260f), new Vector2(260f, 30f),
                18f, MutedText, FontStyles.Bold);
            CreatePositionButtons(canvas, system, new Vector2(-164f, -315f));

            var controller = CreateController(canvas, UIFXStyledDemoController.DemoKind.DynamicOrder);
            var so = new SerializedObject(controller);
            so.FindProperty("dynamicOrderSystem").objectReferenceValue = system;
            AssignArray(so.FindProperty("dynamicItems"), components);
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(DynamicOrderScene);
        }

        static RectTransform BeginScene(string title, string subtitle)
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = StageBg;
                cam.clearFlags = CameraClearFlags.SolidColor;
            }

            CreateEventSystem();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.GetComponent<RectTransform>();
            CreateBackground(root);
            CreateLabel(root, title, new Vector2(-560f, 300f), new Vector2(560f, 45f), 34f, Color.white, FontStyles.Bold)
                .alignment = TextAlignmentOptions.Left;
            CreateLabel(root, subtitle, new Vector2(-560f, 268f), new Vector2(560f, 26f), 17f, MutedText)
                .alignment = TextAlignmentOptions.Left;

            return root;
        }

        static void CreateBackground(RectTransform parent)
        {
            var bg = CreateImage(parent, "Background", Vector2.zero, RefResolution, StageBg);
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;

            for (int i = 0; i < 12; i++)
            {
                float x = -560f + i * 105f;
                var line = CreateImage(parent, "SubtleLine", new Vector2(x, 0f), new Vector2(1f, 720f),
                    new Color(1f, 1f, 1f, 0.035f));
                line.raycastTarget = false;
            }
        }

        static UIFXStyledDemoController CreateController(Transform parent, UIFXStyledDemoController.DemoKind kind)
        {
            var go = new GameObject("StyledDemoController", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var controller = go.AddComponent<UIFXStyledDemoController>();
            var so = new SerializedObject(controller);
            so.FindProperty("demoKind").enumValueIndex = (int)kind;
            so.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        static UIShimmerEffect CreateShimmerCard(Transform parent, ShimmerCard card, Material shimmerMat, Vector2 pos)
        {
            var image = CreateImage(parent, card.label, pos, new Vector2(232f, 112f), card.background);
            AddShadow(image.gameObject, new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, -8f));
            AddOutline(image.gameObject, new Color(1f, 1f, 1f, 0.10f), new Vector2(2f, -2f));

            var fx = image.gameObject.AddComponent<UIShimmerEffect>();
            var so = new SerializedObject(fx);
            so.FindProperty("shimmerMaterialSource").objectReferenceValue = shimmerMat;
            so.FindProperty("shimmerColor").colorValue = card.shimmer;
            so.FindProperty("shimmerAngle").floatValue = card.angle;
            so.FindProperty("shimmerWidth").floatValue = card.width;
            so.FindProperty("intensity").floatValue = card.intensity;
            so.FindProperty("duration").floatValue = 0.6f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var label = CreateLabel(image.transform, card.label, new Vector2(-88f, 22f), new Vector2(160f, 28f),
                18f, Color.white, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Left;
            var detail = CreateLabel(image.transform, card.detail, new Vector2(-88f, -14f), new Vector2(160f, 22f),
                12f, MutedText);
            detail.alignment = TextAlignmentOptions.Left;

            CreateImage(image.transform, "Swatch", new Vector2(88f, -34f), new Vector2(18f, 18f), card.shimmer);
            return fx;
        }

        static RectTransform CreatePathSegment(Transform parent, Vector2 a, Vector2 b, out float length)
        {
            Vector2 dir = b - a;
            length = dir.magnitude;

            var root = new GameObject("PathSegment", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = a;
            rt.sizeDelta = new Vector2(length, 18f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            var track = root.AddComponent<Image>();
            track.color = new Color(0.13f, 0.18f, 0.25f);
            track.raycastTarget = false;

            var fill = CreateImage(root.transform, "Fill", Vector2.zero, new Vector2(length, 18f),
                new Color(0.20f, 0.86f, 0.72f));
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.anchoredPosition = Vector2.zero;
            fillRt.sizeDelta = new Vector2(0f, 0f);
            fill.raycastTarget = false;
            return fillRt;
        }

        static Image CreatePathNode(Transform parent, Vector2 pos, int index, bool active)
        {
            var outer = CreateImage(parent, $"Node_{index}", pos, Vector2.one * 58f,
                active ? new Color(0.20f, 0.88f, 0.74f) : new Color(0.16f, 0.20f, 0.29f));
            AddOutline(outer.gameObject, active ? Color.white : new Color(0.42f, 0.48f, 0.60f), new Vector2(3f, -3f));
            AddShadow(outer.gameObject, new Color(0.26f, 0.93f, 0.82f, active ? 0.45f : 0.10f), new Vector2(0f, -4f));
            CreateLabel(outer.transform, index.ToString(), Vector2.zero, Vector2.one * 32f,
                18f, active ? new Color(0.02f, 0.08f, 0.08f) : MutedText, FontStyles.Bold);
            return outer;
        }

        static RectTransform CreateGlowDot(Transform parent, string name, Vector2 pos, float size, Color color)
        {
            var dot = CreateImage(parent, name, pos, Vector2.one * size, Color.white);
            AddShadow(dot.gameObject, color, new Vector2(0f, 0f));
            return dot.rectTransform;
        }

        static void CreateRangeSlider(Transform parent, Vector2 pos)
        {
            var track = CreateImage(parent, "RangeSlider", pos, new Vector2(650f, 10f), new Color(0.13f, 0.18f, 0.25f));
            var fill = CreateImage(track.transform, "Range", new Vector2(-210f, 0f), new Vector2(430f, 10f), new Color(0.20f, 0.86f, 0.72f));
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            CreateImage(track.transform, "StartHandle", new Vector2(-260f, 0f), Vector2.one * 26f, Color.white);
            CreateImage(track.transform, "EndHandle", new Vector2(170f, 0f), Vector2.one * 26f, Color.white);
        }

        static Image CreateCommandButton(Transform parent, string text, Vector2 pos, Vector2 size, Color fill, Color accent)
        {
            var image = CreateImage(parent, text, pos, size, fill);
            AddOutline(image.gameObject, accent, new Vector2(2f, -2f));
            AddShadow(image.gameObject, new Color(accent.r, accent.g, accent.b, 0.35f), new Vector2(0f, -4f));
            CreateLabel(image.transform, text, Vector2.zero, size, 17f, Color.white, FontStyles.Bold);
            return image;
        }

        static RectTransform CreateCursor(Transform parent, Vector2 pos)
        {
            var root = new GameObject("Cursor", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(52f, 72f);

            var pointer = CreateLabel(root.transform, ">", Vector2.zero, new Vector2(60f, 60f),
                52f, Color.white, FontStyles.Bold);
            pointer.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            AddOutline(pointer.gameObject, new Color(0.10f, 0.14f, 0.20f), new Vector2(2f, -2f));
            return rt;
        }

        static void CreateSwatches(Transform parent, Vector2 pos)
        {
            Color[] colors =
            {
                new Color(0.15f, 0.84f, 1f),
                new Color(0.55f, 1f, 0.42f),
                new Color(1f, 0.82f, 0.30f),
                new Color(1f, 0.42f, 0.76f),
            };

            for (int i = 0; i < colors.Length; i++)
            {
                var swatch = CreateImage(parent, $"Swatch_{i}", pos + new Vector2(i * 58f, 0f),
                    new Vector2(34f, 28f), colors[i]);
                AddOutline(swatch.gameObject, i == 0 ? Color.white : Line, new Vector2(2f, -2f));
            }
        }

        static Image CreateCoin(Transform parent, Vector2 pos, float size)
        {
            var coin = CreateImage(parent, "Coin", pos, Vector2.one * (size * 2f), new Color(1f, 0.78f, 0.20f));
            AddOutline(coin.gameObject, new Color(1f, 0.94f, 0.56f), new Vector2(2f, -2f));
            CreateLabel(coin.transform, "$", Vector2.zero, Vector2.one * size, size * 0.95f,
                new Color(0.48f, 0.30f, 0.04f), FontStyles.Bold);
            return coin;
        }

        static void CreateGlowMeter(Transform parent, Vector2 pos, Color[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                CreateImage(parent, $"GlowBarTrack_{i}", pos + new Vector2(i * 160f, 0f), new Vector2(115f, 9f),
                    new Color(0.13f, 0.18f, 0.25f));
                CreateImage(parent, $"GlowBar_{i}", pos + new Vector2(i * 160f - 26f, 0f), new Vector2(62f + i * 8f, 9f),
                    colors[i]);
            }
        }

        static UIDynamicOrderItem CreateDynamicItem(Transform parent, DynamicItem cfg, int index, Vector2 pos)
        {
            var image = CreateImage(parent, $"Item_{cfg.label}", pos, new Vector2(500f, 70f), cfg.color);
            image.raycastTarget = true;
            AddShadow(image.gameObject, new Color(0f, 0f, 0f, 0.28f), new Vector2(0f, -6f));
            AddOutline(image.gameObject, new Color(1f, 1f, 1f, 0.12f), new Vector2(2f, -2f));

            var group = image.gameObject.AddComponent<CanvasGroup>();
            var item = image.gameObject.AddComponent<UIDynamicOrderItem>();

            var highlight = CreateImage(image.transform, "Highlight", Vector2.zero, image.rectTransform.sizeDelta, Color.clear);
            highlight.rectTransform.anchorMin = Vector2.zero;
            highlight.rectTransform.anchorMax = Vector2.one;
            highlight.rectTransform.offsetMin = highlight.rectTransform.offsetMax = Vector2.zero;
            highlight.raycastTarget = false;

            var idx = CreateLabel(image.transform, (index + 1).ToString(), new Vector2(-212f, 0f), new Vector2(46f, 44f),
                21f, Color.white, FontStyles.Bold);
            var label = CreateLabel(image.transform, cfg.label, new Vector2(-105f, 0f), new Vector2(280f, 44f),
                22f, Color.white);
            label.alignment = TextAlignmentOptions.Left;

            var so = new SerializedObject(item);
            so.FindProperty("background").objectReferenceValue = image;
            so.FindProperty("highlightOverlay").objectReferenceValue = highlight;
            so.FindProperty("indexLabel").objectReferenceValue = idx;
            so.FindProperty("contentLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            _ = group;
            return item;
        }

        static void CreatePositionButtons(Transform parent, UIDynamicOrderSystem system, Vector2 start)
        {
            string[] methods =
            {
                "OnPositionButton1", "OnPositionButton2", "OnPositionButton3",
                "OnPositionButton4", "OnPositionButton5"
            };

            for (int i = 0; i < methods.Length; i++)
            {
                var button = CreateCommandButton(parent, (i + 1).ToString(), start + new Vector2(i * 82f, 0f),
                    new Vector2(56f, 46f), new Color(0.16f, 0.20f, 0.27f), new Color(0.45f, 0.52f, 1f));
                button.raycastTarget = true;
                var btn = button.gameObject.AddComponent<Button>();
                btn.targetGraphic = button;

                var btnSo = new SerializedObject(btn);
                var calls = btnSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                calls.arraySize = 1;
                var call = calls.GetArrayElementAtIndex(0);
                call.FindPropertyRelative("m_Target").objectReferenceValue = system;
                call.FindPropertyRelative("m_MethodName").stringValue = methods[i];
                call.FindPropertyRelative("m_Mode").intValue = 1;
                call.FindPropertyRelative("m_CallState").intValue = 2;
                btnSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static Image CreateImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TextMeshProUGUI CreateLabel(
            Transform parent,
            string text,
            Vector2 pos,
            Vector2 size,
            float fontSize,
            Color color,
            FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(text.Length > 28 ? text.Substring(0, 28) : text, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void AddShadow(GameObject go, Color color, Vector2 distance)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
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

        static void SaveScene(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        }

        static void AssignArray<T>(SerializedProperty prop, T[] values) where T : Object
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static void AssignVectorArray(SerializedProperty prop, Vector2[] values)
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).vector2Value = values[i];
        }

        static void AssignFloatArray(SerializedProperty prop, float[] values)
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).floatValue = values[i];
        }

        readonly struct ShimmerCard
        {
            public readonly string label;
            public readonly string detail;
            public readonly Color background;
            public readonly Color shimmer;
            public readonly float angle;
            public readonly float width;
            public readonly float intensity;

            public ShimmerCard(string label, string detail, Color background, Color shimmer, float angle, float width, float intensity)
            {
                this.label = label;
                this.detail = detail;
                this.background = background;
                this.shimmer = shimmer;
                this.angle = angle;
                this.width = width;
                this.intensity = intensity;
            }
        }

        readonly struct DynamicItem
        {
            public readonly string label;
            public readonly Color color;

            public DynamicItem(string label, Color color)
            {
                this.label = label;
                this.color = color;
            }
        }
    }
}
