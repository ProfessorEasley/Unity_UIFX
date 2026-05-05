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
    [Tooltip("Particles launch from this point — usually the Fill button.")]
    public RectTransform launchPoint;

    [Header("Spawn")]
    [Range(1, 120)] public int poolSize = 60;
    [Range(0.5f, 30f)] public float emissionRate = 12f;
    [Tooltip("Spread at launch point — 0 = perfect laser, higher = wider stream.")]
    [Range(0f, 60f)] public float launchSpread = 15f;

    [Header("Motion")]
    [Range(50f, 800f)] public float flySpeed = 220f;
    [Range(0f, 300f)] public float arcHeight = 120f;

    [Header("Trails")]
    public bool enableTrails = true;
    [Range(0.02f, 0.2f)] public float trailSpawnInterval = 0.04f;
    [Range(0.1f, 0.8f)] public float trailGhostLifetime = 0.3f;

    [Header("Scale")]
    [Range(4f, 60f)] public float minSize = 8f;
    [Range(4f, 60f)] public float maxSize = 20f;

    [Header("Fill Behavior")]
    [Range(0.05f, 1f)] public float fillPerClick = 0.2f;
    [Range(0.005f, 0.1f)] public float fillPerParticle = 0.02f;

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

    public void SetParticleColor(Color c) => sparkleColor = c;
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
            TrySpawnMain();
        }
    }

    private void TrySpawnMain()
    {
        foreach (var p in _pool)
        {
            if (!p.active) { ActivateMain(p); return; }
        }
    }

    private void ActivateMain(UISparkleParticle p)
    {
        // Launch from the button position (with small spread)
        Vector2 launchLocal = GetLaunchLocalPos();
        launchLocal += new Vector2(
            Random.Range(-launchSpread, launchSpread),
            Random.Range(-launchSpread, launchSpread));

        p.startPos = launchLocal;
        p.targetPos = GetRandomBarPoint();
        p.rect.anchoredPosition = launchLocal;

        // Arc control point — always curves UPWARD toward the bar
        Vector2 mid = (launchLocal + p.targetPos) * 0.5f;
        // push the midpoint higher (positive Y) for a nice upward arc
        p.controlPoint = mid + new Vector2(
            Random.Range(-40f, 40f),   // slight side variation
            arcHeight);                 // consistently upward

        float s = Random.Range(minSize, maxSize);
        p.rect.sizeDelta = Vector2.one * s;
        p.scaleStart = s;
        p.scaleEnd = s * 0.2f;

        float dist = Vector2.Distance(launchLocal, p.targetPos);
        p.maxLifetime = Mathf.Clamp(dist / flySpeed, 0.3f, 3f);
        p.lifetime = 0f;
        p.arrived = false;
        p.isGhost = false;
        p.trailTimer = 0f;
        p.speed = flySpeed;
        p.rotationSpeed = Random.Range(-180f, 180f);

        ApplyColor(p);

        p.group.alpha = 0f;
        p.rect.gameObject.SetActive(true);
        p.active = true;
    }

    private void TrySpawnGhost(Vector2 atPos, float parentSize, Color parentColor)
    {
        foreach (var p in _pool)
        {
            if (!p.active)
            {
                ActivateGhost(p, atPos, parentSize, parentColor);
                return;
            }
        }
    }

    private void ActivateGhost(UISparkleParticle p, Vector2 atPos, float parentSize, Color parentColor)
    {
        p.rect.anchoredPosition = atPos;
        p.rect.sizeDelta = Vector2.one * parentSize * 0.7f;
        p.scaleStart = parentSize * 0.7f;
        p.scaleEnd = 0f;
        p.lifetime = 0f;
        p.maxLifetime = trailGhostLifetime;
        p.isGhost = true;
        p.arrived = false;

        Color c = parentColor;
        c.a = 0.5f;
        p.rect.GetComponent<Image>().color = c;

        p.group.alpha = 0.5f;
        p.rect.gameObject.SetActive(true);
        p.active = true;
    }

    private void Deactivate(UISparkleParticle p)
    {
        p.group.alpha = 0f;
        p.rect.gameObject.SetActive(false);
        p.active = false;
    }

    private void ApplyColor(UISparkleParticle p)
    {
        Color c = sparkleColor;
        if (hueVariance > 0f)
        {
            Color.RGBToHSV(c, out float h, out float sv, out float v);
            h = Mathf.Repeat(h + Random.Range(-hueVariance, hueVariance), 1f);
            c = Color.HSVToRGB(h, sv, v);
            c.a = sparkleColor.a;
        }
        p.rect.GetComponent<Image>().color = c;
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

            if (p.isGhost)
            {
                if (t >= 1f) { Deactivate(p); continue; }
                p.group.alpha = (1f - t) * 0.5f;
                p.rect.sizeDelta = Vector2.one * Mathf.Lerp(p.scaleStart, p.scaleEnd, t);
                continue;
            }

            Vector2 pos = QuadraticBezier(p.startPos, p.controlPoint, p.targetPos, t);
            p.rect.anchoredPosition = pos;

            if (enableTrails)
            {
                p.trailTimer -= dt;
                if (p.trailTimer <= 0f)
                {
                    p.trailTimer = trailSpawnInterval;
                    TrySpawnGhost(pos, p.rect.sizeDelta.x,
                                  p.rect.GetComponent<Image>().color);
                }
            }

            if (t >= 1f && !p.arrived)
            {
                p.arrived = true;
                if (_currentFill < _targetFill)
                {
                    _currentFill = Mathf.Min(_targetFill, _currentFill + fillPerParticle);
                    if (healthBarFill != null)
                    {
                        healthBarFill.fillAmount = _currentFill;
                        healthBarFill.color = Color.Lerp(Color.red, sparkleColor, _currentFill);
                    }
                }
                Deactivate(p);
                continue;
            }

            p.group.alpha = t < 0.2f ? t / 0.2f : 1f;
            p.rect.sizeDelta = Vector2.one * Mathf.Lerp(p.scaleStart, p.scaleEnd, t);
            p.rect.Rotate(0f, 0f, p.rotationSpeed * dt);
        }
    }

    // ── Bezier ────────────────────────────────────────────────

    private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float u = 1f - t;
        return (u * u) * a + (2f * u * t) * b + (t * t) * c;
    }

    // ── Position helpers ──────────────────────────────────────

    private Vector2 GetLaunchLocalPos()
    {
        if (launchPoint == null) return Vector2.zero;

        // World center of the launch RectTransform
        Vector3[] corners = new Vector3[4];
        launchPoint.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selfRect, screenPoint, GetCanvasCamera(), out Vector2 local);
        return local;
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