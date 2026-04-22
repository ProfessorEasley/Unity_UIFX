using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Scripts/Control Panel")]
public class UIControlPanel : MonoBehaviour
{
    public UISparkleLoop sparkleLoop;

    [Header("Color Buttons")]
    public Button greenButton;
    public Button blueButton;
    public Button goldButton;

    [Header("Sliders")]
    public Slider speedSlider;    // range 50-800
    public Slider sizeSlider;     // range 4-60

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
            speedSlider.onValueChanged.AddListener(sparkleLoop.SetSpeed);
        }

        if (sizeSlider)
        {
            sizeSlider.minValue = 4f;
            sizeSlider.maxValue = 60f;
            sizeSlider.value = 20f;
            sizeSlider.onValueChanged.AddListener(sparkleLoop.SetSize);
        }
    }
}