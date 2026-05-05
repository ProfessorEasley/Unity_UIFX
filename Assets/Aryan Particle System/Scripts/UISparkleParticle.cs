using UnityEngine;

internal class UISparkleParticle
{
    public RectTransform rect;
    public CanvasGroup group;
    public float lifetime;
    public float maxLifetime;
    public float speed;
    public Vector2 targetPos;
    public float rotationSpeed;
    public float scaleStart;
    public float scaleEnd;
    public bool active;
    public bool arrived;

    public Vector2 startPos;
    public Vector2 controlPoint;
    public bool isGhost;
    public float trailTimer;
}