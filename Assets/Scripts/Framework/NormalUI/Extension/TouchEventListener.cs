using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

// UI鼠标管理;

public class TouchEventListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate(GameObject go);

    public delegate void VoidDragDelegate(GameObject go, PointerEventData eventData);

    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDragDelegate onDragBengin;
    public VoidDragDelegate onDrag;
    public VoidDragDelegate onDragEnd;
    public VoidDelegate onMove;

    public static TouchEventListener Get(GameObject go)
    {
        TouchEventListener listener = go.GetComponent<TouchEventListener>();
        if (listener == null)
        {
            listener = go.AddComponent<TouchEventListener>();
        }

        return listener;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (onDragBengin != null)
        {
            onDragBengin(gameObject, eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null) onDrag(gameObject, eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (onDragEnd != null) onDragEnd(gameObject, eventData);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (onMove != null) onMove(gameObject);
    }
}
