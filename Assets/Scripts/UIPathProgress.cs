using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathProgress : MonoBehaviour
{
    [Header("Nodes (in path order)")]
    public List<RectTransform> nodes = new List<RectTransform>();

    [Header("Prefabs / Parents")]
    public Image linePrefab;
    public RectTransform linesParent;

    [Header("Rendering")]
    [Range(0f, 1f)] public float startProgress = 0f;
    [Range(0f, 1f)] public float endProgress = 1f;
    public bool loopPath = false;

    private readonly List<Image> _lines = new List<Image>();
    private readonly List<Image> _nodeFillImages = new List<Image>();
    private float[] _segmentNormalizedLengths;

    private bool _builtAtRuntime = false;

    void Awake()
    {
        if (Application.isPlaying)
        {
            Rebuild();
            UpdateRender();
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying && _builtAtRuntime)
        {
            ClearLines();
        }
    }

    void OnDestroy()
    {
        if (_builtAtRuntime)
        {
            ClearLines();
        }
    }

    // =========================
    // Public API
    // =========================

    public void SetRange(float start, float end)
    {
        startProgress = Mathf.Clamp01(start);
        endProgress = Mathf.Clamp01(end);
        if (startProgress > endProgress)
            startProgress = endProgress;

        UpdateRender();
    }

    public void SetProgress(float progress)
    {
        endProgress = Mathf.Clamp01(progress);
        if (endProgress < startProgress)
            startProgress = endProgress;

        UpdateRender();
    }

    public void AnimateRangeTo(float newStart, float newEnd, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateRangeRoutine(newStart, newEnd, duration));
    }

    // =========================
    // Build + Cleanup
    // =========================

    public void Rebuild()
    {
        if (!Application.isPlaying)
            return;

        _builtAtRuntime = true;

        ClearLines();
        _nodeFillImages.Clear();

        if (linePrefab == null || nodes == null || nodes.Count < 2)
            return;

        if (linesParent == null)
            linesParent = (RectTransform)transform;

        int segments = loopPath ? nodes.Count : nodes.Count - 1;

        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = (i + 1) % nodes.Count;

            Image instance = Instantiate(linePrefab, linesParent);
            instance.name = $"Line_{a}_{b}";
            _lines.Add(instance);

            Image nodeFill = null;
            Transform fillTransform = nodes[b].Find("Fill");
            if (fillTransform != null)
                nodeFill = fillTransform.GetComponent<Image>();

            _nodeFillImages.Add(nodeFill);
        }

        ComputeSegmentLengths();
    }

    private void ClearLines()
    {
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            if (_lines[i] != null)
                Destroy(_lines[i].gameObject);
        }

        _lines.Clear();
    }

    // =========================
    // Rendering Logic
    // =========================

    private void ComputeSegmentLengths()
    {
        int segCount = loopPath ? nodes.Count : nodes.Count - 1;
        _segmentNormalizedLengths = new float[segCount];

        float total = 0f;

        for (int i = 0; i < segCount; i++)
        {
            RectTransform a = nodes[i];
            RectTransform b = nodes[(i + 1) % nodes.Count];

            float dist = Vector2.Distance(a.anchoredPosition, b.anchoredPosition);
            _segmentNormalizedLengths[i] = dist;
            total += dist;
        }

        if (total <= 0f)
            total = 1f;

        for (int i = 0; i < segCount; i++)
            _segmentNormalizedLengths[i] /= total;
    }

    private void UpdateLineTransforms()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            RectTransform a = nodes[i];
            RectTransform b = nodes[(i + 1) % nodes.Count];

            Vector2 dir = b.anchoredPosition - a.anchoredPosition;
            float dist = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            RectTransform rt = _lines[i].rectTransform;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(dist, rt.sizeDelta.y);
            rt.anchoredPosition = a.anchoredPosition;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void UpdateRender()
    {
        if (!Application.isPlaying)
            return;

        if (_lines.Count == 0)
            return;

        ComputeSegmentLengths();
        UpdateLineTransforms();

        float start = Mathf.Clamp01(startProgress);
        float end = Mathf.Clamp01(endProgress);

        if (end < start)
            end = start;

        float accumulated = 0f;

        for (int i = 0; i < _lines.Count; i++)
        {
            float segLen = _segmentNormalizedLengths[i];
            float segStart = accumulated;
            float segEnd = accumulated + segLen;

            float overlapStart = Mathf.Max(start, segStart);
            float overlapEnd = Mathf.Min(end, segEnd);

            float fillPercent = 0f;

            if (overlapEnd > overlapStart && segLen > 0f)
                fillPercent = (overlapEnd - overlapStart) / segLen;

            fillPercent = Mathf.Clamp01(fillPercent);

            _lines[i].fillAmount = fillPercent;

            if (_nodeFillImages[i] != null)
                _nodeFillImages[i].fillAmount = fillPercent;

            accumulated += segLen;
        }

        if (!loopPath && end >= 1f)
        {
            int lastIndex = nodes.Count - 1;
            Transform fillTransform = nodes[lastIndex].Find("Fill");

            if (fillTransform != null)
            {
                Image img = fillTransform.GetComponent<Image>();
                if (img != null)
                    img.fillAmount = 1f;
            }
        }
    }

    // =========================
    // Animation Coroutine
    // =========================

    private System.Collections.IEnumerator AnimateRangeRoutine(
        float targetStart,
        float targetEnd,
        float duration)
    {
        float initialStart = startProgress;
        float initialEnd = endProgress;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            startProgress = Mathf.Lerp(initialStart, targetStart, p);
            endProgress = Mathf.Lerp(initialEnd, targetEnd, p);

            UpdateRender();
            yield return null;
        }

        startProgress = targetStart;
        endProgress = targetEnd;
        UpdateRender();
    }
}