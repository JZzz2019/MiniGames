using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnHover;
    public Action OnUnhover;
    public Action OnSelect;
    //private void Awake()
    //{
    //    outline = GetComponent<Outline>();
    //    isOutlineEnabled = outline.enabled;
    //}
    public void OnPointerClick(PointerEventData eventData)
    {
        //isOutlineEnabled = !isOutlineEnabled;
        //outline.enabled = isOutlineEnabled;
        OnSelect?.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUnhover?.Invoke();
    }
}
