using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace UIFX.FX
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIDynamicOrderItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image background;
        [SerializeField] Image highlightOverlay;
        [SerializeField] TextMeshProUGUI indexLabel;
        [SerializeField] TextMeshProUGUI contentLabel;

        [Header("Colors")]
        [SerializeField] Color highlightColor = new Color(1f, 0.75f, 0.2f, 0.4f);
        [SerializeField] Color flashColor = new Color(1f, 0.4f, 0.1f, 0.6f);

        public event Action<UIDynamicOrderItem> Clicked;
        public int OriginalIndex { get; set; }

        RectTransform _rect;
        CanvasGroup _group;
        Coroutine _selectRoutine;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _group = GetComponent<CanvasGroup>();
        }

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(this);

        public void Select()
        {
            if (_selectRoutine != null)
                StopCoroutine(_selectRoutine);
            _selectRoutine = StartCoroutine(SelectPulse());
        }

        public void Deselect()
        {
            if (_selectRoutine != null)
            {
                StopCoroutine(_selectRoutine);
                _selectRoutine = null;
            }
            if (highlightOverlay != null)
                highlightOverlay.color = Color.clear;
            transform.localScale = Vector3.one;
        }

        IEnumerator SelectPulse()
        {
            if (highlightOverlay == null) yield break;

            float t = 0f;
            const float fadeDur = 0.12f;
            while (t < fadeDur)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / fadeDur) * highlightColor.a;
                highlightOverlay.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, a);
                yield return null;
            }
            highlightOverlay.color = highlightColor;

            t = 0f;
            const float punchDur = 0.15f;
            while (t < punchDur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / punchDur);
                float s = 1f + 0.04f * Mathf.Sin(p * Mathf.PI);
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;
            _selectRoutine = null;
        }

        public void SetIndex(int index)
        {
            if (indexLabel != null)
                indexLabel.text = (index + 1).ToString();
        }

        public void SetLabel(string text)
        {
            if (contentLabel != null)
                contentLabel.text = text;
        }

        public IEnumerator AnimateExit(float duration)
        {
            float flashDur = duration * 0.5f;
            float singleFlash = flashDur / 3f;
            float t = 0f;

            while (t < flashDur)
            {
                t += Time.deltaTime;
                if (highlightOverlay != null)
                {
                    float within = t % singleFlash;
                    highlightOverlay.color = within < singleFlash * 0.5f ? flashColor : Color.clear;
                }
                yield return null;
            }
            if (highlightOverlay != null)
                highlightOverlay.color = Color.clear;

            float fadeDur = duration * 0.5f;
            t = 0f;
            while (t < fadeDur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDur);
                _group.alpha = 1f - EaseInCubic(p);
                float s = Mathf.Lerp(1f, 0.85f, EaseInCubic(p));
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            _group.alpha = 0f;
            transform.localScale = new Vector3(0.85f, 0.85f, 1f);
        }

        public IEnumerator AnimateEnter(float duration)
        {
            _group.alpha = 0f;
            transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            float mainDur = duration * 0.7f;
            float t = 0f;
            while (t < mainDur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / mainDur);
                _group.alpha = Mathf.Clamp01(p * 1.5f);
                float s = LerpUnclamped(0.85f, 1f, EaseOutBack(p));
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            _group.alpha = 1f;
            transform.localScale = Vector3.one;

            if (highlightOverlay != null)
            {
                float pulseDur = duration * 0.3f;
                t = 0f;
                while (t < pulseDur)
                {
                    t += Time.deltaTime;
                    float p = Mathf.Clamp01(t / pulseDur);
                    float a = (1f - p) * highlightColor.a;
                    highlightOverlay.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, a);
                    yield return null;
                }
                highlightOverlay.color = Color.clear;
            }
        }

        public IEnumerator AnimateTranslate(Vector2 targetPos, float duration)
        {
            Vector2 startPos = _rect.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = EaseOutCubic(Mathf.Clamp01(t / duration));
                _rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, p);
                yield return null;
            }
            _rect.anchoredPosition = targetPos;
        }

        public void SetPositionImmediate(Vector2 pos)
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            _rect.anchoredPosition = pos;
        }

        public void ResetVisuals()
        {
            if (_group == null) _group = GetComponent<CanvasGroup>();
            _group.alpha = 1f;
            transform.localScale = Vector3.one;
            if (highlightOverlay != null)
                highlightOverlay.color = Color.clear;
        }

        static float EaseOutCubic(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        static float EaseInCubic(float t) => t * t * t;
        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;
    }
}
