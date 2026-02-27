using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIPathProgress))]
public class MinMaxSliderController : MonoBehaviour
{
    public Slider startSlider;
    public Slider endSlider;

    private UIPathProgress _path;

    void Awake()
    {
        _path = GetComponent<UIPathProgress>();
        if (startSlider != null) startSlider.onValueChanged.AddListener(OnStartSliderChanged);
        if (endSlider != null) endSlider.onValueChanged.AddListener(OnEndSliderChanged);
        // initialize UI from component
        if (startSlider != null) startSlider.value = _path.startProgress;
        if (endSlider != null) endSlider.value = _path.endProgress;
    }

    void OnDestroy()
    {
        if (startSlider != null) startSlider.onValueChanged.RemoveListener(OnStartSliderChanged);
        if (endSlider != null) endSlider.onValueChanged.RemoveListener(OnEndSliderChanged);
    }

    private void OnStartSliderChanged(float v)
    {
        // clamp so start <= end
        float newStart = Mathf.Min(v, endSlider.value);
        startSlider.value = newStart;
        _path.SetRange(newStart, endSlider.value);
    }

    private void OnEndSliderChanged(float v)
    {
        float newEnd = Mathf.Max(v, startSlider.value);
        endSlider.value = newEnd;
        _path.SetRange(startSlider.value, newEnd);
    }
}