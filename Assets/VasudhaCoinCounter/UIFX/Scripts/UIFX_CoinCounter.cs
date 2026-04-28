using TMPro;
using UnityEngine;
using System.Collections;
using System.Globalization;

public class UIFX_CoinCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private RectTransform target;

    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float punchScale = 1.15f;

    private int currentValue = 0;
    private Coroutine running;

    private void Awake()
    {
        if (valueText == null) return;

        string normalized = valueText.text.Replace(",", string.Empty);
        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            currentValue = parsed;
    }

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

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float value = Mathf.Lerp(start, end, eased);
            valueText.text = FormatValue(Mathf.RoundToInt(value));

            float punch = Mathf.Sin(t * Mathf.PI);
            target.localScale = originalScale * Mathf.Lerp(1f, punchScale, punch);

            yield return null;
        }

        valueText.text = FormatValue(end);
        target.localScale = originalScale;

        currentValue = end;
    }

    private static string FormatValue(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);
}
