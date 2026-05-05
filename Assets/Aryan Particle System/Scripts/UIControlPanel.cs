using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu("Scripts/Control Panel")]
public class UIControlPanel : MonoBehaviour
{
    public UISparkleLoop sparkleLoop;

    [Header("Color Buttons")]
    public Button greenButton;
    public Button blueButton;
    public Button goldButton;

    [Header("Sliders")]
    public Slider speedSlider;
    public Slider sizeSlider;

    [Header("Slider Value Labels (optional)")]
    public TMP_Text speedValueLabel;
    public TMP_Text sizeValueLabel;

    private void Start()
    {
        if (greenButton) greenButton.onClick.AddListener(() =>
            sparkleLoop.SetParticleColor(new Color(0.4f, 1f, 0.6f, 1f)));

        if (blueButton) blueButton.onClick.AddListener(() =>
            sparkleLoop.SetParticleColor(new Color(0.4f, 0.7f, 1f, 1f)));

        if (goldButton) goldButton.onClick.AddListener(() =>
            sparkleLoop.SetParticleColor(new Color(1f, 0.85f, 0.3f, 1f)));

        if (speedSlider)
        {
            speedSlider.minValue = 50f;
            speedSlider.maxValue = 800f;
            speedSlider.value = 220f;
            speedSlider.onValueChanged.AddListener(v =>
            {
                sparkleLoop.SetSpeed(v);
                if (speedValueLabel) speedValueLabel.text = $"Speed: {Mathf.RoundToInt(v)}";
            });
            if (speedValueLabel) speedValueLabel.text = $"Speed: {Mathf.RoundToInt(speedSlider.value)}";
        }

        if (sizeSlider)
        {
            sizeSlider.minValue = 4f;
            sizeSlider.maxValue = 60f;
            sizeSlider.value = 20f;
            sizeSlider.onValueChanged.AddListener(v =>
            {
                sparkleLoop.SetSize(v);
                if (sizeValueLabel) sizeValueLabel.text = $"Size: {Mathf.RoundToInt(v)}";
            });
            if (sizeValueLabel) sizeValueLabel.text = $"Size: {Mathf.RoundToInt(sizeSlider.value)}";
        }
    }
}