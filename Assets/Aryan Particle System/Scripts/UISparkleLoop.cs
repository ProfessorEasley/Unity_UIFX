using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Scripts/Sparkle Loop")]
public class UISparkleLoop : MonoBehaviour
{
    [Header("Appearance")]
    public Sprite sparkleSprite;
    public Color sparkleColor = new Color(0.4f, 1f, 0.6f, 1f);
    [Range(0f, 0.15f)] public float hueVariance = 0.06f;

    [Header("References")]
    public Image healthBarFill;

    [Header("Spawn")]
    [Range(1, 60)] public int poolSize = 30;
    [Range(0.5f, 30f)] public float emissionRate = 12f;

    [Header("Motion")]
    [Range(50f, 800f)] public float flySpeed = 220f;
    [Range(0f, 80f)] public float wobble = 30f;

    [Header("Scale")]
    [Range(4f, 60f)] public float minSize = 8f;
    [Range(4f, 60f)] public float maxSize = 20f;

    [Header("Fill Behavior")]
    [Range(0.05f, 1f)] public float fillPerClick = 0.2f;          // how much each click adds
    [Range(0.005f, 0.1f)] public float fillPerParticle = 0.02f;   // how much each particle adds

    // ── Private ───────────────────────────────────────────────
    private readonly List<UISparkleParticle> _pool = new List<UISparkleParticle>();
    private RectTransform _selfRect;
    private Canvas _canvas;
    private bool _emitting = false;
    private float _emitTimer;
    private float _currentFill = 0f;
    private float _targetFill = 0f;

    public System.Action OnFillComplete;

    // ── Unity lifecycle ───────────────────────────────────────

    private void Awake()
    {
        _selfRect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        BuildPool();

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 0f;
            healthBarFill.color = Color.red;
        }
    }

    private void Update()
    {
        if (_emitting) TickEmitter();
        TickParticles();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>One click = add fillPerClick amount to the bar.</summary>
    public void AddFillChunk()
    {
        if (_currentFill >= 1f) return;
        _targetFill = Mathf.Clamp01(_targetFill + fillPerClick);
        _emitting = true;
    }

    public void ResetBar()
    {
        _emitting = false;
        _currentFill = 0f;
        _targetFill = 0f;
        foreach (var p in _pool) Deactivate(p);
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 0f;
            healthBarFill.color = Color.red;
        }
    }

    // ── Live setters for the control panel ──────────────────

    public void SetParticleColor(Color c)
    {
        sparkleColor = c;
    }

    public void SetSpeed(float s) => flySpeed = Mathf.Clamp(s, 50f, 800f);
    public void SetSize(float s)
    {
        minSize = Mathf.Clamp(s * 0.5f, 4f, 60f);
        maxSize = Mathf.Clamp(s, 4f, 60f);
    }

    // ── Pool ──────────────────────────────────────────────────

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"Sparkle_{i:00}");
            go.transform.SetParent(transform, false);

            var img = go.AddComponent<Image>();
            img.sprite = sparkleSprite;
            img.raycastTarget = false;
            img.color = sparkleColor;

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * minSize;
            rt.anchoredPosition = Vector2.zero;
            go.SetActive(false);

            _pool.Add(new UISparkleParticle { rect = rt, group = cg, active = false });
        }
    }

    // ── Emission ──────────────────────────────────────────────

    private void TickEmitter()
    {
        // Stop emitting when we've hit the target chunk
        if (_currentFill >= _targetFill - 0.001f)
        {
            _emitting = false;
            if (_currentFill >= 1f) OnFillComplete?.Invoke();
            return;
        }

        _emitTimer += Time.deltaTime;
        float interval = 1f / emissionRate;
        while (_emitTimer >= interval)
        {
            _emitTimer -= interval;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        foreach (var p in _pool)
        {
            if (!p.active) { Activate(p); return; }
        }
    }

    private void Activate(UISparkleParticle p)
    {
        Vector2 screenSpawn = GetRandomScreenEdgePoint();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selfRect, screenSpawn, GetCanvasCamera(), out Vector2 localSpawn);

        p.rect.anchoredPosition = localSpawn;
        p.targetPos = GetRandomBarPoint();

        float s = Random.Range(minSize, maxSize);
        p.rect.sizeDelta = Vector2.one * s;
        p.scaleStart = s;
        p.scaleEnd = s * 0.2f;

        float dist = Vector2.Distance(localSpawn, p.targetPos);
        p.maxLifetime = Mathf.Clamp(dist / flySpeed, 0.3f, 3f);
        p.lifetime = 0f;
        p.arrived = false;
        p.speed = flySpeed;
        p.rotationSpeed = Random.Range(-180f, 180f);

        Color c = sparkleColor;
        if (hueVariance > 0f)
        {
            Color.RGBToHSV(c, out float h, out float sv, out float v);
            h = Mathf.Repeat(h + Random.Range(-hueVariance, hueVariance), 1f);
            c = Color.HSVToRGB(h, sv, v);
            c.a = sparkleColor.a;
        }
        p.rect.GetComponent<Image>().color = c;

        p.group.alpha = 0f;
        p.rect.gameObject.SetActive(true);
        p.active = true;
    }

    private void Deactivate(UISparkleParticle p)
    {
        p.group.alpha = 0f;
        p.rect.gameObject.SetActive(false);
        p.active = false;
    }

    // ── Per-frame tick ────────────────────────────────────────

    private void TickParticles()
    {
        float dt = Time.deltaTime;

        foreach (var p in _pool)
        {
            if (!p.active) continue;

            p.lifetime += dt;
            float t = Mathf.Clamp01(p.lifetime / p.maxLifetime);

            Vector2 current = p.rect.anchoredPosition;
            float dist = Vector2.Distance(current, p.targetPos);

            if (dist < 8f && !p.arrived)
            {
                p.arrived = true;
                // Only fill up to the current target chunk
                if (_currentFill < _targetFill)
                {
                    _currentFill = Mathf.Min(_targetFill, _currentFill + fillPerParticle);
                    if (healthBarFill != null)
                    {
                        healthBarFill.fillAmount = _currentFill;
                        healthBarFill.color = Color.Lerp(Color.red, Color.green, _currentFill);
                    }
                }
                Deactivate(p);
                continue;
            }

            Vector2 dir = (p.targetPos - current).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float wobbleOffset = Mathf.Sin(p.lifetime * 8f) * wobble;
            p.rect.anchoredPosition += (dir + perp * wobbleOffset * dt) * p.speed * dt;

            p.group.alpha = t < 0.2f ? t / 0.2f : 1f;
            p.rect.sizeDelta = Vector2.one * Mathf.Lerp(p.scaleStart, p.scaleEnd, t);
            p.rect.Rotate(0f, 0f, p.rotationSpeed * dt);
        }
    }

    // ── Helpers ───────────────────────────────────────────────

    private Vector2 GetRandomScreenEdgePoint()
    {
        int edge = Random.Range(0, 4);
        float sw = Screen.width;
        float sh = Screen.height;
        return edge switch
        {
            0 => new Vector2(Random.Range(0f, sw), sh),
            1 => new Vector2(Random.Range(0f, sw), 0f),
            2 => new Vector2(0f, Random.Range(0f, sh)),
            _ => new Vector2(sw, Random.Range(0f, sh)),
        };
    }

    private Vector2 GetRandomBarPoint()
    {
        if (healthBarFill == null) return Vector2.zero;
        Vector3[] corners = new Vector3[4];
        healthBarFill.rectTransform.GetWorldCorners(corners);
        Vector3 worldPoint = Vector3.Lerp(
            Vector3.Lerp(corners[0], corners[1], Random.value),
            Vector3.Lerp(corners[3], corners[2], Random.value),
            Random.value);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selfRect,
            RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), worldPoint),
            GetCanvasCamera(), out Vector2 local);
        return local;
    }

    private Camera GetCanvasCamera() =>
        _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _canvas.worldCamera : null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxSize = Mathf.Max(maxSize, minSize);
    }
#endif
}