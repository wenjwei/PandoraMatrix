using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

namespace com.tencent.pandora
{
    [CustomLuaClass]
    public class DraggableButton : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public class DragEvent : UnityEvent<Vector2>
        {
        }
        public DragEvent onDrag = new DragEvent();
        public DragEvent onBeginDrag = new DragEvent();
        public DragEvent onEndDrag = new DragEvent();

        private bool _isDragging = false;
        private RectTransform _rect;
        private Vector2 _lastMousePosition = new Vector2();
        private Vector2 _delta = new Vector2();

        protected override void Awake()
        {
            if (transform.parent == null)
            {
                return;
            }
            _rect = transform.parent.GetComponent<RectTransform>();
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 mousePosition = new Vector2();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out mousePosition))
            {
                _lastMousePosition = mousePosition;
                onBeginDrag.Invoke(mousePosition);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 mousePosition = new Vector2();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out mousePosition))
            {
                _delta = mousePosition - _lastMousePosition;
                onDrag.Invoke(_delta);
                _lastMousePosition = mousePosition;
            }
            //标记不再触发点击事件
            _isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            Vector2 mousePosition = new Vector2();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out mousePosition))
            {
                onEndDrag.Invoke(mousePosition);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging == true)
            {
                return;
            }
            base.OnPointerClick(eventData);
        }
    }
}

