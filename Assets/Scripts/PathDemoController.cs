using System.Collections;
using UnityEngine;

public class PathDemoController : MonoBehaviour
{
    [Header("References")]
    public UIPathProgress pathSystem;
    
    [Header("Settings")]
    public float animationDuration = 2.0f;
    public float pauseAtEnd = 0.5f;

    private bool _isAnimating = false;

    // This is the function we will link to the Button
    public void PlayDemo()
    {
        if (_isAnimating) return;
        StartCoroutine(DemoSequence());
    }

    private IEnumerator DemoSequence()
    {
        _isAnimating = true;

        // 1. Reset to zero
        pathSystem.SetRange(0, 0);
        yield return new WaitForSeconds(0.2f);

        // 2. Animate Fill UP (0 to 1)
        float elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0, 1, elapsed / animationDuration);
            pathSystem.SetRange(0, percent);
            yield return null;
        }

        pathSystem.SetRange(0, 1);
        yield return new WaitForSeconds(pauseAtEnd);

        // 3. Animate Clear DOWN (1 to 0)
        elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(1, 0, elapsed / animationDuration);
            pathSystem.SetRange(0, percent);
            yield return null;
        }

        pathSystem.SetRange(0, 0);
        _isAnimating = false;
    }
}