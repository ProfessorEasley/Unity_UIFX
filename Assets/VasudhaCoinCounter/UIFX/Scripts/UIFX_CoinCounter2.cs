using TMPro;
using UnityEngine;
using System.Collections;

public class UIFX_CoinCounter2 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private RectTransform target;

    [Header("Timing")]
    [SerializeField] private float duration = 0.8f;

    [Header("Punch Effects")]
    [SerializeField] private float punchScale = 1.15f;
    [SerializeField] private float tiltAmount = 8f;

    private int currentValue = 0;
    private Coroutine running;

    public void Play(int newValue)
    {
        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(Animate(currentValue, newValue));
    }

    private IEnumerator Animate(int start, int end)
    {
        float time = 0f;

        Vector3 originalScale = target.localScale;
        Quaternion originalRotation = target.localRotation;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

            // smoother UI easing
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            // number animation
            float value = Mathf.Lerp(start, end, eased);
            valueText.text = Mathf.RoundToInt(value).ToString();

            // scale punch
            float punch = Mathf.Sin(t * Mathf.PI);
            float scale = Mathf.Lerp(1f, punchScale, punch);
            target.localScale = originalScale * scale;

            // 3D tilt
            float tilt = Mathf.Sin(t * Mathf.PI) * tiltAmount;
            target.localRotation = Quaternion.Euler(0f, tilt, 0f);

            yield return null;
        }

        valueText.text = end.ToString();
        target.localScale = originalScale;
        target.localRotation = originalRotation;

        currentValue = end;
    }
}