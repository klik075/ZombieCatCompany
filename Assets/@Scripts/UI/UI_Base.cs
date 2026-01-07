using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public interface IUI_Popup
{

}

public interface IUI_Scene
{

}

public class UI_Base : MonoBehaviour
{
    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        RefreshUI();
    }

    protected virtual void OnEnable()
    {
        EventManager.Instance.AddEvent(Define.EEventType.LanguageChanged, RefreshUI);
    }

    protected virtual void OnDisable()
    {
        EventManager.Instance.RemoveEvent(Define.EEventType.LanguageChanged, RefreshUI);
    }

    public virtual void RefreshUI()
    {

    }
}
