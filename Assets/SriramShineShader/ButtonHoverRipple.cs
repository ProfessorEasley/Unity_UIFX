using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonHoverRipple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float rippleDuration = 0.55f;
    [SerializeField] private float hoverAmount = 1f;
    [SerializeField] private float idleAmount = 0f;

    private Material materialInstance;
    private float timer;
    private bool playing;

    private void Awake()
    {
        Image img = GetComponent<Image>();

        if (img.material != null)
        {
            materialInstance = Instantiate(img.material);
            img.material = materialInstance;
        }

        if (materialInstance != null)
        {
            materialInstance.SetFloat("_ManualProgress", 0f);
            materialInstance.SetFloat("_HoverAmount", 0f);
        }
    }

    private void Update()
    {
        if (!playing || materialInstance == null)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / rippleDuration);

        materialInstance.SetFloat("_ManualProgress", t);
        materialInstance.SetFloat("_HoverAmount", hoverAmount);

        if (t >= 1f)
        {
            playing = false;
            materialInstance.SetFloat("_ManualProgress", 0f);
            materialInstance.SetFloat("_HoverAmount", idleAmount);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (materialInstance == null)
            return;

        timer = 0f;
        playing = true;
        materialInstance.SetFloat("_ManualProgress", 0f);
        materialInstance.SetFloat("_HoverAmount", hoverAmount);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (materialInstance == null)
            return;

        playing = false;
        timer = 0f;
        materialInstance.SetFloat("_ManualProgress", 0f);
        materialInstance.SetFloat("_HoverAmount", idleAmount);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}