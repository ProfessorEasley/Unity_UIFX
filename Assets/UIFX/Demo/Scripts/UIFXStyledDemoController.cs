using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIFX.FX;

namespace UIFX.Demo
{
    public class UIFXStyledDemoController : MonoBehaviour
    {
        public enum DemoKind
        {
            Shimmer,
            PathProgress,
            ShineShader,
            SparkleParticles,
            CoinCounter,
            SpriteGlow,
            DynamicOrder
        }

        [SerializeField] DemoKind demoKind;

        [Header("Shimmer")]
        [SerializeField] UIShimmerEffect[] shimmerEffects;

        [Header("Path Progress")]
        [SerializeField] RectTransform[] pathFillRects;
        [SerializeField] Image[] pathNodeFills;
        [SerializeField] RectTransform pathHead;
        [SerializeField] Vector2[] pathPoints;
        [SerializeField] float[] pathLengths;

        [Header("Shine Shader")]
        [SerializeField] Image shineButton;
        [SerializeField] Image shineRipple;
        [SerializeField] RectTransform shineCursor;

        [Header("Sparkles")]
        [SerializeField] UISparkleLoop sparkleLoop;
        [SerializeField] Image sparkleBarFill;
        [SerializeField] TextMeshProUGUI sparklePercentLabel;

        [Header("Coin Counter")]
        [SerializeField] UIFX_CoinCounter coinCounter;
        [SerializeField] RectTransform coinTarget;
        [SerializeField] RectTransform[] coinBursts;

        [Header("Sprite Glow")]
        [SerializeField] RectTransform[] glowTargets;
        [SerializeField] Graphic[] glowGraphics;

        [Header("Dynamic Order")]
        [SerializeField] UIDynamicOrderSystem dynamicOrderSystem;
        [SerializeField] UIDynamicOrderItem[] dynamicItems;

        [Header("Edit Mode Preview")]
        [SerializeField] GameObject[] playModeHiddenObjects;

        Material _shineMaterial;
        float _timer;
        float _nextAction;

        void Start()
        {
            HidePlayModeHiddenObjects();
            SeedTeaserTiming();

            if (shineButton != null && shineButton.material != null)
            {
                _shineMaterial = Instantiate(shineButton.material);
                shineButton.material = _shineMaterial;
            }

            if (demoKind == DemoKind.DynamicOrder)
                StartCoroutine(RunDynamicOrderLoop());
        }

        void Update()
        {
            _timer += Time.deltaTime;

            switch (demoKind)
            {
                case DemoKind.Shimmer:
                    UpdateShimmer();
                    break;
                case DemoKind.PathProgress:
                    UpdatePathProgress();
                    break;
                case DemoKind.ShineShader:
                    UpdateShineShader();
                    break;
                case DemoKind.SparkleParticles:
                    UpdateSparkles();
                    break;
                case DemoKind.CoinCounter:
                    UpdateCoinCounter();
                    break;
                case DemoKind.SpriteGlow:
                    UpdateSpriteGlow();
                    break;
            }
        }

        void OnDestroy()
        {
            if (_shineMaterial != null)
                Destroy(_shineMaterial);
        }

        void HidePlayModeHiddenObjects()
        {
            if (playModeHiddenObjects == null) return;

            foreach (var obj in playModeHiddenObjects)
                if (obj != null)
                    obj.SetActive(false);
        }

        void SeedTeaserTiming()
        {
            switch (demoKind)
            {
                case DemoKind.Shimmer:
                    _nextAction = 0.18f;
                    break;
                case DemoKind.PathProgress:
                    _timer = 1.18f;
                    break;
                case DemoKind.ShineShader:
                    _timer = 0.35f;
                    break;
                case DemoKind.SparkleParticles:
                    _nextAction = 0.65f;
                    break;
                case DemoKind.CoinCounter:
                    _nextAction = 0.9f;
                    break;
            }
        }

        void UpdateShimmer()
        {
            if (_timer < _nextAction) return;
            _nextAction = _timer + 1.25f;

            foreach (var shimmer in shimmerEffects)
                if (shimmer != null)
                    shimmer.TriggerEffect();
        }

        void UpdatePathProgress()
        {
            if (pathFillRects == null || pathLengths == null || pathPoints == null) return;
            if (pathFillRects.Length == 0 || pathLengths.Length == 0 || pathPoints.Length < 2) return;

            float progress = 0.08f + 0.86f * EaseInOut((Mathf.Sin(_timer * 0.85f - Mathf.PI * 0.5f) + 1f) * 0.5f);
            float totalLength = 0f;
            foreach (float length in pathLengths)
                totalLength += Mathf.Max(0.001f, length);

            float filledLength = totalLength * progress;
            float accumulated = 0f;

            for (int i = 0; i < pathFillRects.Length; i++)
            {
                float segmentLength = Mathf.Max(0.001f, pathLengths[i]);
                float fill = Mathf.Clamp01((filledLength - accumulated) / segmentLength);
                var rect = pathFillRects[i];
                if (rect != null)
                    rect.sizeDelta = new Vector2(segmentLength * fill, rect.sizeDelta.y);
                accumulated += segmentLength;
            }

            if (pathNodeFills != null)
            {
                float nodeStep = 1f / Mathf.Max(1, pathNodeFills.Length - 1);
                for (int i = 0; i < pathNodeFills.Length; i++)
                {
                    var fill = pathNodeFills[i];
                    if (fill == null) continue;

                    bool active = progress >= i * nodeStep - 0.01f;
                    fill.color = active
                        ? new Color(0.22f, 0.92f, 0.78f, 1f)
                        : new Color(0.18f, 0.24f, 0.34f, 1f);
                    float pulse = active ? 1f + Mathf.Max(0f, Mathf.Sin((_timer * 3.8f) - i)) * 0.08f : 1f;
                    fill.rectTransform.localScale = Vector3.one * pulse;
                }
            }

            if (pathHead == null) return;

            filledLength = totalLength * progress;
            accumulated = 0f;
            for (int i = 0; i < pathLengths.Length; i++)
            {
                float segmentLength = Mathf.Max(0.001f, pathLengths[i]);
                if (filledLength <= accumulated + segmentLength || i == pathLengths.Length - 1)
                {
                    float local = Mathf.Clamp01((filledLength - accumulated) / segmentLength);
                    pathHead.anchoredPosition = Vector2.Lerp(pathPoints[i], pathPoints[i + 1], local);
                    Vector2 direction = pathPoints[i + 1] - pathPoints[i];
                    pathHead.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    break;
                }
                accumulated += segmentLength;
            }
        }

        void UpdateShineShader()
        {
            float progress = Mathf.Repeat(_timer * 0.42f, 1f);

            if (_shineMaterial != null)
            {
                _shineMaterial.SetFloat("_ManualProgress", progress);
                _shineMaterial.SetFloat("_HoverAmount", 1f);
                _shineMaterial.SetFloat("_ShineSpeed", 0f);
            }

            if (shineRipple != null)
            {
                float ripple = Mathf.Repeat(_timer * 0.85f, 1f);
                Color color = shineRipple.color;
                color.a = Mathf.Lerp(0.35f, 0f, ripple);
                shineRipple.color = color;
                shineRipple.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(70f, 430f, ripple);
            }

            if (shineCursor != null)
            {
                shineCursor.anchoredPosition = new Vector2(
                    186f + Mathf.Sin(_timer * 2.7f) * 18f,
                    -118f + Mathf.Cos(_timer * 2.2f) * 10f);
            }
        }

        void UpdateSparkles()
        {
            if (sparkleLoop != null && _timer >= _nextAction)
            {
                if (sparkleBarFill != null && sparkleBarFill.fillAmount > 0.92f)
                    sparkleLoop.ResetBar();

                sparkleLoop.AddFillChunk();
                _nextAction = _timer + 0.8f;
            }

            if (sparklePercentLabel != null && sparkleBarFill != null)
                sparklePercentLabel.text = Mathf.RoundToInt(sparkleBarFill.fillAmount * 100f) + "%";
        }

        void UpdateCoinCounter()
        {
            if (_timer >= _nextAction && coinCounter != null)
            {
                int target = 1250 + Mathf.RoundToInt(Mathf.Repeat(_timer * 0.19f, 1f) * 8750f);
                coinCounter.Play(target);
                _nextAction = _timer + 1.15f;
            }

            if (coinTarget != null)
            {
                float punch = 1f + Mathf.Max(0f, Mathf.Sin(Mathf.Repeat(_timer * 2.3f, 1f) * Mathf.PI)) * 0.08f;
                coinTarget.localScale = Vector3.one * punch;
            }

            if (coinBursts == null) return;

            for (int i = 0; i < coinBursts.Length; i++)
            {
                var coin = coinBursts[i];
                if (coin == null) continue;

                float p = Mathf.Repeat(_timer * 0.42f + i * 0.071f, 1f);
                float x = Mathf.Lerp(-420f + (i % 7) * 120f, -165f, EaseOutCubic(p));
                float y = Mathf.Lerp(-300f, 0f, EaseOutCubic(p)) - Mathf.Sin(p * Mathf.PI) * 120f;
                coin.anchoredPosition = new Vector2(x, y);
                coin.localRotation = Quaternion.Euler(0f, 0f, _timer * 180f + i * 31f);
            }
        }

        void UpdateSpriteGlow()
        {
            if (glowTargets == null) return;

            for (int i = 0; i < glowTargets.Length; i++)
            {
                var target = glowTargets[i];
                if (target == null) continue;

                float pulse = Mathf.Pow(Mathf.Sin(_timer * 1.8f + i * 0.8f), 2f);
                target.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.18f, pulse);

                if (glowGraphics != null && i < glowGraphics.Length && glowGraphics[i] != null)
                {
                    Color color = glowGraphics[i].color;
                    color.a = Mathf.Lerp(0.32f, 0.72f, pulse);
                    glowGraphics[i].color = color;
                }
            }
        }

        IEnumerator RunDynamicOrderLoop()
        {
            yield return null;

            while (true)
            {
                if (dynamicOrderSystem != null && dynamicItems != null && dynamicItems.Length >= 4)
                {
                    dynamicItems[3].OnPointerClick(null);
                    yield return new WaitForSeconds(0.28f);
                    dynamicOrderSystem.MoveSelectedToPosition(1);
                    yield return new WaitForSeconds(2.2f);
                    dynamicOrderSystem.ResetOrder();
                }

                yield return new WaitForSeconds(1.2f);
            }
        }

        static float EaseInOut(float t) =>
            t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

        static float EaseOutCubic(float t) =>
            1f - Mathf.Pow(1f - t, 3f);
    }
}
