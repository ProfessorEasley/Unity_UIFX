using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonShinyControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float duration = 0.6f;

    private Material mat;
    private Image img;

    private float timer = 0f;
    private bool playing = false;

    private void Awake()
    {
        img = GetComponent<Image>();

        mat = Instantiate(img.material);
        img.material = mat;

        mat.SetFloat("_HoverAmount", 0f);
    }

    private void Update()
    {
        if (!playing) return;

        timer += Time.deltaTime;

        float t = timer / duration;

        mat.SetFloat("_HoverAmount", 1f);
        mat.SetFloat("_ShineSpeed", 0f); // stop auto movement
        mat.SetFloat("_ManualProgress", t);

        if (t >= 1f)
        {
            playing = false;
            mat.SetFloat("_HoverAmount", 0f); // hide shine after pass
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        timer = 0f;
        playing = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playing = false;
        mat.SetFloat("_HoverAmount", 0f);
    }
}