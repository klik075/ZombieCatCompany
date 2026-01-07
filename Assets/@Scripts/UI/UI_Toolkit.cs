using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class UI_Toolkit : UI_Base
{
	protected Dictionary<System.Type, VisualElement[]> _uiObjects = new Dictionary<System.Type, VisualElement[]>();

	// UI Toolkit version of Bind methods
	protected void BindButtons(System.Type type) { Bind<Button>(type); }
	protected void BindTexts(System.Type type) { Bind<Label>(type); }
	protected void BindImages(System.Type type) { Bind<Image>(type); }

	protected void Bind<T>(System.Type type) where T : VisualElement
	{
		string[] names = System.Enum.GetNames(type);
		VisualElement[] elements = new VisualElement[names.Length];

		var root = GetComponent<UIDocument>().rootVisualElement;
		var panel = root.Q<VisualElement>();

		for (int i = 0; i < names.Length; i++)
		{
			elements[i] = panel.Q<T>(names[i]);

			if (elements[i] == null)
				Debug.Log($"Failed to bind({names[i]})");
		}

		_uiObjects.Add(typeof(T), elements);
	}

	protected T Get<T>(int idx) where T : VisualElement
	{
		if (_uiObjects.TryGetValue(typeof(T), out VisualElement[] elements) == false)
			return null;

		return elements[idx] as T;
	}
}
