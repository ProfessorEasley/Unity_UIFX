using UnityEngine;
using TMPro;

[AddComponentMenu("Scripts/Health Bar Button")]
public class UIHealthBarButton : MonoBehaviour
{
    public UISparkleLoop sparkleLoop;
    public TMP_Text buttonLabel;

    private bool _full = false;

    public void OnButtonClicked()
    {
        if (_full)
        {
            sparkleLoop.ResetBar();
            _full = false;
            if (buttonLabel) buttonLabel.text = "Fill +20%";
        }
        else
        {
            sparkleLoop.AddFillChunk();
        }
    }

    private void Start()
    {
        sparkleLoop.OnFillComplete += () =>
        {
            _full = true;
            if (buttonLabel) buttonLabel.text = "Reset";
        };

        if (buttonLabel) buttonLabel.text = "Fill +20%";
    }
}