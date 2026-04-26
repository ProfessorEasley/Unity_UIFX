using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathProgress : MonoBehaviour
{
    [Header("Nodes (in path order)")]
    public List<RectTransform> nodes = new List<RectTransform>();

    [Header("Prefabs / Parents")]
    public GameObject linePrefab; // Changed to GameObject to support Background/Fill hierarchy
    public RectTransform linesParent;
    
    [Header("VFX Support")]
    public RectTransform leadHead; // The "Firework" emitter object
    public bool rotateHeadToPath = true;
    public float nodePulseScale = 1.3f;

    [Header("Rendering")]
    [Range(0f, 1f)] public float startProgress = 0f;
    [Range(0f, 1f)] public float endProgress = 1f;
    public bool loopPath = false;

    private readonly List<Image> _lines = new List<Image>();
    private readonly List<Image> _nodeFillImages = new List<Image>();
    private readonly HashSet<int> _pulsedNodes = new HashSet<int>(); 
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

    // =========================
    // Public API
    // =========================

    public void SetRange(float start, float end)
    {
        startProgress = Mathf.Clamp01(start);
        endProgress = Mathf.Clamp01(end);
        if (startProgress > endProgress) startProgress = endProgress;
        UpdateRender();
    }

    public void SetProgress(float progress)
    {
        endProgress = Mathf.Clamp01(progress);
        if (endProgress < startProgress) startProgress = endProgress;
        UpdateRender();
    }

    // =========================
    // Build + Cleanup
    // =========================

    public void Rebuild()
    {
        if (!Application.isPlaying) return;

        _builtAtRuntime = true;
        ClearLines();
        _nodeFillImages.Clear();
        _pulsedNodes.Clear();

        if (linePrefab == null || nodes == null || nodes.Count < 2) return;
        if (linesParent == null) linesParent = (RectTransform)transform;

        int segments = loopPath ? nodes.Count : nodes.Count - 1;

        for (int i = 0; i < segments; i++)
        {
            // 1. Instantiate the whole prefab root
            GameObject instance = Instantiate(linePrefab, linesParent);
            instance.name = $"Line_{i}";

            // 2. Extract the Fill Image (searches for child named "Fill", defaults to root)
            Image fillImage = null;
            Transform fillChild = instance.transform.Find("Fill");
            fillImage = fillChild != null ? fillChild.GetComponent<Image>() : instance.GetComponent<Image>();
            _lines.Add(fillImage);

            // 3. Cache Node Fills
            Image nodeFill = null;
            Transform nodeFillTransform = nodes[(i + 1) % nodes.Count].Find("Fill");
            if (nodeFillTransform != null) nodeFill = nodeFillTransform.GetComponent<Image>();
            _nodeFillImages.Add(nodeFill);
        }

        ComputeSegmentLengths();
    }

    private void ClearLines()
    {
        foreach (var img in _lines) if (img != null) Destroy(img.transform.parent.gameObject);
        _lines.Clear();
    }

    // =========================
    // Core Rendering Logic
    // =========================

    public void UpdateRender()
    {
        if (!Application.isPlaying || _lines.Count == 0) return;

        ComputeSegmentLengths();
        UpdateLineTransforms();

        float start = startProgress;
        float end = endProgress;
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
            {
                _nodeFillImages[i].fillAmount = fillPercent;
                
                // Trigger Node Pop when fully filled
                if (fillPercent >= 1.0f && !_pulsedNodes.Contains(i))
                {
                    _pulsedNodes.Add(i);
                    StartCoroutine(PulseNode(nodes[(i + 1) % nodes.Count]));
                }
            }
            accumulated += segLen;
        }

        UpdateLeadHead(end);
    }

    private void UpdateLeadHead(float end)
    {
        if (leadHead == null) return;

        float accumulated = 0f;
        for (int i = 0; i < _lines.Count; i++)
        {
            float segLen = _segmentNormalizedLengths[i];
            float segEnd = accumulated + segLen;

            // Find segment containing the end progress tip
            if (end <= segEnd + 0.001f || i == _lines.Count - 1)
            {
                float localT = (segLen > 0) ? (end - accumulated) / segLen : 0;
                localT = Mathf.Clamp01(localT);

                RectTransform a = nodes[i];
                RectTransform b = nodes[(i + 1) % nodes.Count];

                leadHead.anchoredPosition = Vector2.Lerp(a.anchoredPosition, b.anchoredPosition, localT);

                if (rotateHeadToPath)
                {
                    Vector2 dir = b.anchoredPosition - a.anchoredPosition;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    leadHead.localRotation = Quaternion.Euler(0, 0, angle);
                }
                break;
            }
            accumulated += segLen;
        }
    }

    private void ComputeSegmentLengths()
    {
        int segCount = loopPath ? nodes.Count : nodes.Count - 1;
        _segmentNormalizedLengths = new float[segCount];
        float total = 0f;

        for (int i = 0; i < segCount; i++)
        {
            float dist = Vector2.Distance(nodes[i].anchoredPosition, nodes[(i + 1) % nodes.Count].anchoredPosition);
            _segmentNormalizedLengths[i] = dist;
            total += dist;
        }

        if (total <= 0f) total = 1f;
        for (int i = 0; i < segCount; i++) _segmentNormalizedLengths[i] /= total;
    }

    private void UpdateLineTransforms()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            RectTransform a = nodes[i];
            RectTransform b = nodes[(i + 1) % nodes.Count];
            Vector2 dir = b.anchoredPosition - a.anchoredPosition;
            
            // Note: We rotate the Parent of the fill image to keep Background & Fill aligned
            RectTransform lineRoot = (RectTransform)_lines[i].transform.parent;
            lineRoot.pivot = new Vector2(0f, 0.5f);
            lineRoot.sizeDelta = new Vector2(dir.magnitude, lineRoot.sizeDelta.y);
            lineRoot.anchoredPosition = a.anchoredPosition;
            lineRoot.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
    }

    private System.Collections.IEnumerator PulseNode(RectTransform node)
    {
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * 5f;
            float s = Mathf.Lerp(1f, nodePulseScale, Mathf.Sin(t * Mathf.PI));
            node.localScale = Vector3.one * s;
            yield return null;
        }
        node.localScale = Vector3.one;
    }
}