using TMPro;
using UnityEngine;
using System.Collections;

public class UIFX_CoinCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private RectTransform target;

    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float punchScale = 1.15f;

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

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float value = Mathf.Lerp(start, end, eased);
            valueText.text = Mathf.RoundToInt(value).ToString();

            float punch = Mathf.Sin(t * Mathf.PI);
            target.localScale = originalScale * Mathf.Lerp(1f, punchScale, punch);

            yield return null;
        }

        valueText.text = end.ToString();
        target.localScale = originalScale;

        currentValue = end;
    }
}