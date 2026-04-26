using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFX.Core
{
    /// <summary>
    /// Abstract base for click-triggered UI shader effects.
    /// Attach a concrete subclass (e.g. UIShimmerEffect) to any UI GameObject.
    /// Handles click detection, coroutine lifecycle, and re-trigger interruption.
    /// </summary>
    public abstract class UIEffectBase : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] protected float duration = 0.6f;

        private Coroutine _activeRoutine;

        public void OnPointerClick(PointerEventData eventData) => TriggerEffect();

        /// <summary>Call this directly to trigger the effect without a click (e.g. from animation events).</summary>
        public void TriggerEffect()
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);

            _activeRoutine = StartCoroutine(EffectRoutine());
        }

        protected abstract IEnumerator EffectRoutine();
    }
}
