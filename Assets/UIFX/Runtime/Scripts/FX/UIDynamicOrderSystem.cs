using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFX.FX
{
    public class UIDynamicOrderSystem : MonoBehaviour
    {
        [SerializeField] List<UIDynamicOrderItem> items = new List<UIDynamicOrderItem>();

        [Header("Layout")]
        [SerializeField] float itemHeight = 70f;
        [SerializeField] float itemSpacing = 14f;
        [SerializeField] float startY = 210f;

        [Header("Animation Durations")]
        [SerializeField] float exitDuration = 0.35f;
        [SerializeField] float translateDuration = 0.3f;
        [SerializeField] float enterDuration = 0.4f;

        UIDynamicOrderItem _selectedItem;
        bool _animating;

        void Start()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Clicked += OnItemClicked;
                items[i].OriginalIndex = i;
                items[i].SetIndex(i);
                items[i].SetPositionImmediate(GetSlotPosition(i));
            }
        }

        void OnDestroy()
        {
            foreach (var item in items)
                if (item != null)
                    item.Clicked -= OnItemClicked;
        }

        Vector2 GetSlotPosition(int index)
        {
            return new Vector2(0f, startY - index * (itemHeight + itemSpacing));
        }

        void OnItemClicked(UIDynamicOrderItem item)
        {
            if (_animating) return;

            if (_selectedItem == item)
            {
                _selectedItem.Deselect();
                _selectedItem = null;
                return;
            }

            if (_selectedItem != null)
                _selectedItem.Deselect();

            _selectedItem = item;
            _selectedItem.Select();
        }

        public void MoveSelectedToPosition(int targetIndex)
        {
            if (_selectedItem == null || _animating) return;
            if (targetIndex < 0 || targetIndex >= items.Count) return;
            StartCoroutine(ReorderSequence(_selectedItem, targetIndex));
        }

        public void OnPositionButton1() => MoveSelectedToPosition(0);
        public void OnPositionButton2() => MoveSelectedToPosition(1);
        public void OnPositionButton3() => MoveSelectedToPosition(2);
        public void OnPositionButton4() => MoveSelectedToPosition(3);
        public void OnPositionButton5() => MoveSelectedToPosition(4);

        IEnumerator ReorderSequence(UIDynamicOrderItem item, int targetIndex)
        {
            _animating = true;
            int currentIndex = items.IndexOf(item);

            if (currentIndex == targetIndex)
            {
                item.Deselect();
                _selectedItem = null;
                _animating = false;
                yield break;
            }

            item.Deselect();
            _selectedItem = null;

            yield return StartCoroutine(item.AnimateExit(exitDuration));

            items.RemoveAt(currentIndex);
            items.Insert(targetIndex, item);

            var translates = new List<Coroutine>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == item) continue;
                items[i].SetIndex(i);
                translates.Add(StartCoroutine(
                    items[i].AnimateTranslate(GetSlotPosition(i), translateDuration)));
            }
            foreach (var c in translates)
                yield return c;

            item.SetPositionImmediate(GetSlotPosition(targetIndex));
            item.SetIndex(targetIndex);
            yield return StartCoroutine(item.AnimateEnter(enterDuration));

            _animating = false;
        }

        public void ResetOrder()
        {
            if (_animating) return;
            StartCoroutine(ResetSequence());
        }

        IEnumerator ResetSequence()
        {
            _animating = true;

            if (_selectedItem != null)
            {
                _selectedItem.Deselect();
                _selectedItem = null;
            }

            items.Sort((a, b) => a.OriginalIndex.CompareTo(b.OriginalIndex));

            var moves = new List<Coroutine>();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ResetVisuals();
                items[i].SetIndex(i);
                moves.Add(StartCoroutine(
                    items[i].AnimateTranslate(GetSlotPosition(i), translateDuration)));
            }
            foreach (var c in moves)
                yield return c;

            _animating = false;
        }
    }
}
