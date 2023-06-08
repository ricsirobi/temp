using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class UILayout
{
	public string LayoutName;

	public List<UIGameObject> GameObjects = new List<UIGameObject>();

	public UILayout DefaultLayoutReference;

	public UILayout(string layoutName, Transform uiTransform, UILayout defaultLayout)
	{
		LayoutName = layoutName;
		DefaultLayoutReference = defaultLayout;
		FindGameObjects(uiTransform, "");
	}

	private void FindGameObjects(Transform trans, string hierarchy)
	{
		UIGameObject defaultLayoutObj = GetDefaultLayoutObj(hierarchy);
		UIGameObject uIGameObject = new UIGameObject(trans, hierarchy, defaultLayoutObj);
		if (uIGameObject != null && uIGameObject.ComponentsJson.Count > 0)
		{
			GameObjects.Add(uIGameObject);
		}
		for (int i = 0; i < trans.childCount; i++)
		{
			Transform child = trans.GetChild(i);
			string hierarchy2 = (string.IsNullOrEmpty(hierarchy) ? child.name : (hierarchy + "/" + child.name));
			FindGameObjects(child, hierarchy2);
		}
	}

	private UIGameObject GetDefaultLayoutObj(string hierarchy)
	{
		if (DefaultLayoutReference == null)
		{
			return null;
		}
		foreach (UIGameObject gameObject in DefaultLayoutReference.GameObjects)
		{
			if (gameObject.Hierarchy == hierarchy)
			{
				return gameObject;
			}
		}
		return null;
	}
}
