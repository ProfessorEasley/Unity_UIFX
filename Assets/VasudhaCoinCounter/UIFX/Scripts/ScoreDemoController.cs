using UnityEngine;

public class ScoreDemoController : MonoBehaviour
{
    public UIFX_CoinCounter counter;

    private int score = 0;

    public void AddRandomScore()
    {
        int add = Random.Range(50, 500);
        score += add;

        counter.Play(score);
    }

    public void ResetScore()
    {
        score = 0;
        counter.Play(score);
    }
}