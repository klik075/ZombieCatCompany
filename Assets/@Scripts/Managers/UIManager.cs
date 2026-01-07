using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : Singleton<UIManager>
{
    Transform _root;
    Transform Root
    {
        get
        {
            return Utils.GetRootTransform(ref _root, "@UI_Root");
        }
    }

    #region Scene UI
    private UI_Base _sceneUI;
    public UI_Base SceneUI
    {
        get
        {
			foreach (UI_Base ui in FindObjectsByType<UI_Base>(FindObjectsSortMode.None))
			{
				if (ui is IUI_Scene)
				{
					_sceneUI = ui;
					break;
				}
			}
			
			return _sceneUI;
        }
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Base, IUI_Scene
	{
        if (_sceneUI != null)
            return _sceneUI as T;

        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        T sceneUI = FindFirstObjectByType<T>();
        if (sceneUI == null)
        {
            GameObject go = ResourceManager.Instance.Instantiate(name);
            sceneUI = Utils.GetOrAddComponent<T>(go);
        }

        sceneUI.transform.SetParent(Root);
        _sceneUI = sceneUI;

        return sceneUI;
    }
    #endregion

    #region Popup UI
    Transform _popupRoot;
    Transform PopupRoot
    {
        get
        {
            return Utils.GetRootTransform(ref _popupRoot, "@PopupRoot", Root);
        }
    }

    private int _popupOrder = 100;
    private Stack<UI_Base> _popupStack = new Stack<UI_Base>();
    private Dictionary<string, UI_Base> _popups = new Dictionary<string, UI_Base>();

    public T ShowPopupUI<T>(string name = null) where T : UI_Base, IUI_Popup
	{
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        if (_popups.TryGetValue(name, out UI_Base popup) == false)
        {
            GameObject go = ResourceManager.Instance.Instantiate(name);
            popup = Utils.GetOrAddComponent<T>(go);
            _popups[name] = popup;
        }

        _popupStack.Push(popup);

        popup.transform.SetParent(PopupRoot);
        popup.gameObject.SetActive(true);
        _popupOrder++;

		if (popup is UI_Toolkit toolkitUI)
        {
            toolkitUI.GetComponent<UIDocument>().sortingOrder = _popupOrder;
			toolkitUI.GetComponent<UIDocument>().rootVisualElement.visible = true;
		}
        else
        {
            popup.GetComponent<Canvas>().sortingOrder = _popupOrder;
        }

        return popup as T;
    }

    public T GetLastPopupUI<T>() where T : UI_Base
	{
        if (_popupStack.Count == 0)
            return null;

        return _popupStack.Peek() as T;
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Base popup = _popupStack.Pop();
        if (popup is UI_Toolkit toolkitUI)
			toolkitUI.GetComponent<UIDocument>().rootVisualElement.visible = false;
        else
            popup.gameObject.SetActive(false);

        _popupOrder--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }
    #endregion

    public T ShowUI<T>(string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate(name);

        return go.GetOrAddComponent<T>();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _popups.Clear();

        Root.DestroyChildren();

        _sceneUI = null;
    }
}
