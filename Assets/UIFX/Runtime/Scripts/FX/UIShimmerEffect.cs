using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UIFX.Core;

namespace UIFX.FX
{
    /// <summary>
    /// Click-triggered shimmer sweep effect. Creates a transparent additive overlay Image
    /// as a child and animates a diagonal highlight band across it.
    ///
    /// Setup:
    ///   1. Assign a material using the UIFX/UIShimmer shader to shimmerMaterialSource.
    ///   2. Drop this component onto any UI Image or Button.
    ///
    /// The overlay is sized to cover this RectTransform exactly and has raycastTarget=false
    /// so it does not interfere with the underlying component's click events.
    /// </summary>
    public class UIShimmerEffect : UIEffectBase
    {
        [SerializeField] Material shimmerMaterialSource;

        [Header("Shimmer Settings")]
        [SerializeField] Color shimmerColor = Color.white;
        [SerializeField, Range(0.01f, 0.5f)] float shimmerWidth = 0.15f;
        [SerializeField, Range(-90f, 90f)] float shimmerAngle = 30f;
        [SerializeField, Range(0f, 3f)] float intensity = 1.5f;

        // Cached property IDs — avoid string lookup per frame
        static readonly int PropProgress  = Shader.PropertyToID("_ShimmerProgress");
        static readonly int PropColor     = Shader.PropertyToID("_ShimmerColor");
        static readonly int PropWidth     = Shader.PropertyToID("_ShimmerWidth");
        static readonly int PropAngle     = Shader.PropertyToID("_ShimmerAngle");
        static readonly int PropIntensity = Shader.PropertyToID("_Intensity");

        Image    _overlay;
        Material _mat;      // instanced — safe to mutate per-component

        void Awake()
        {
            if (shimmerMaterialSource == null)
            {
                Debug.LogWarning("[UIFX] UIShimmerEffect: no shimmerMaterialSource assigned.", this);
                return;
            }

            // Own instance so multiple components don't share animation state
            _mat = Instantiate(shimmerMaterialSource);
            _mat.SetColor(PropColor,    shimmerColor);
            _mat.SetFloat(PropWidth,    shimmerWidth);
            _mat.SetFloat(PropAngle,    shimmerAngle);
            _mat.SetFloat(PropIntensity, intensity);
            _mat.SetFloat(PropProgress, -shimmerWidth - 0.1f);  // parked off left edge

            // Build overlay Image child
            var go = new GameObject("ShimmerOverlay", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            go.transform.SetAsLastSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _overlay              = go.GetComponent<Image>();
            _overlay.material     = _mat;
            _overlay.raycastTarget = false;
            _overlay.color        = Color.white;

            // Mirror parent sprite so _MainTex alpha-masks the shimmer to the sprite shape
            var parentImage = GetComponent<Image>();
            if (parentImage != null)
            {
                _overlay.sprite = parentImage.sprite;
                _overlay.type   = parentImage.type;
            }
        }

        void OnDestroy()
        {
            if (_mat != null)
                Destroy(_mat);
        }

        protected override System.Collections.IEnumerator EffectRoutine()
        {
            if (_mat == null) yield break;

            float start = -shimmerWidth - 0.1f;
            float end   = 1f + shimmerWidth + 0.1f;
            float t     = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                _mat.SetFloat(PropProgress, Mathf.Lerp(start, end, p));
                yield return null;
            }

            _mat.SetFloat(PropProgress, end);   // park off right edge when done
        }
    }
}
