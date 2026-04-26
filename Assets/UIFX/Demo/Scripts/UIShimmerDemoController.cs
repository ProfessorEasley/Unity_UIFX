using System.Collections;
using UnityEngine;
using UIFX.FX;

namespace UIFX.Demo
{
    /// <summary>
    /// Attached to the root of the shimmer demo scene.
    /// Lets the "Trigger All" button fire every card's shimmer with a short stagger.
    /// </summary>
    public class UIShimmerDemoController : MonoBehaviour
    {
        [SerializeField] UIShimmerEffect[] cards;
        [SerializeField] float staggerDelay = 0.08f;

        public void TriggerAll()
        {
            StartCoroutine(TriggerAllStaggered());
        }

        private IEnumerator TriggerAllStaggered()
        {
            var wait = new WaitForSeconds(staggerDelay);
            foreach (var card in cards)
            {
                if (card != null)
                    card.TriggerEffect();
                yield return wait;
            }
        }
    }
}
