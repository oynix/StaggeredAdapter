using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gaia
{
    public class EventListener : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private bool _dragging;

        private Action<PointerEventData> _onClick;
        private Action<PointerEventData> _onBeginDrag;
        private Action<PointerEventData> _onDragging;
        private Action<PointerEventData> _onEndDrag;

        #region export api

        public static void RegisterClick(Component t, Action<PointerEventData> onClick)
        {
            RegisterClick(t.gameObject, onClick);
        }

        public static void RegisterClick(GameObject g, Action<PointerEventData> onClick)
        {
            var listener = Get(g);
            listener._onClick = onClick;
        }

        public static void RegisterBeginDrag(Component t, Action<PointerEventData> onBeginDrag)
        {
            RegisterBeginDrag(t.gameObject, onBeginDrag);
        }

        public static void RegisterBeginDrag(GameObject g, Action<PointerEventData> onBeginDrag)
        {
            var listener = Get(g);
            listener._onBeginDrag = onBeginDrag;
        }

        public static void RegisterDragging(Component t, Action<PointerEventData> onDragging)
        {
            RegisterDragging(t.gameObject, onDragging);
        }

        public static void RegisterDragging(GameObject g, Action<PointerEventData> onDragging)
        {
            var listener = Get(g);
            listener._onDragging = onDragging;
        }

        public static void RegisterEndDrag(Component t, Action<PointerEventData> onEndDrag)
        {
            RegisterEndDrag(t.gameObject, onEndDrag);
        }

        public static void RegisterEndDrag(GameObject g, Action<PointerEventData> onEndDrag)
        {
            var listener = Get(g);
            listener._onEndDrag = onEndDrag;
        }

        #endregion

        private static EventListener Get(GameObject g)
        {
            var listener = g.GetComponent<EventListener>();
            if (listener == null) listener = g.AddComponent<EventListener>();
            return listener;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            _onBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDragging?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
            _onEndDrag?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(eventData);
        }
    }
}