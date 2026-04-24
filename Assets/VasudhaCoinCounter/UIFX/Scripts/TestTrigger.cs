using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    public UIFX_CoinCounter counter;

    void Start()
    {
        Debug.Log("Trigger running");
        counter.Play(500);
    }
}