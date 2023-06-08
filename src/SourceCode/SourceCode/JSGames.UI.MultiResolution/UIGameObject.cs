using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class UIGameObject
{
	public string Name;

	public string Hierarchy;

	public List<string> ComponentsJson = new List<string>();

	private UIGameObject DefaultLayoutObjReference;

	public UIGameObject(Transform trans, string hierarchy, UIGameObject defaultLayoutObj)
	{
		Name = trans.name;
		Hierarchy = hierarchy;
		DefaultLayoutObjReference = defaultLayoutObj;
		Component[] components = trans.GetComponents(typeof(Component));
		foreach (Component comp in components)
		{
			string componentJson = GetComponentJson(comp);
			if (!string.IsNullOrEmpty(componentJson))
			{
				ComponentsJson.Add(componentJson);
			}
		}
	}

	private string GetComponentJson(Component comp)
	{
		string name = comp.GetType().Name;
		string result = "";
		switch (name)
		{
		case "Canvas":
			result = GetCanvasComp(comp);
			break;
		case "CanvasScaler":
			result = GetCanvasScalerComp(comp);
			break;
		case "RectTransform":
			result = GetRectTransformComp(comp);
			break;
		case "Text":
			result = GetTextComp(comp);
			break;
		case "GridLayoutGroup":
			result = GetGridLayoutGroupComp(comp);
			break;
		}
		return result;
	}

	private string GetCanvasComp(Component comp)
	{
		CanvasComp canvasComp = new CanvasComp(comp);
		string defaultLayoutComp = GetDefaultLayoutComp("Canvas");
		if (!string.IsNullOrEmpty(defaultLayoutComp))
		{
			CanvasComp canvasComp2 = JsonUtility.FromJson<CanvasComp>(defaultLayoutComp);
			if (canvasComp.Equals(canvasComp2))
			{
				return "";
			}
		}
		return JsonUtility.ToJson(canvasComp);
	}

	private string GetCanvasScalerComp(Component comp)
	{
		CanvasScalerComp canvasScalerComp = new CanvasScalerComp(comp);
		string defaultLayoutComp = GetDefaultLayoutComp("CanvasScaler");
		if (!string.IsNullOrEmpty(defaultLayoutComp))
		{
			CanvasScalerComp canvasScalerComp2 = JsonUtility.FromJson<CanvasScalerComp>(defaultLayoutComp);
			if (canvasScalerComp.Equals(canvasScalerComp2))
			{
				return "";
			}
		}
		return JsonUtility.ToJson(canvasScalerComp);
	}

	private string GetRectTransformComp(Component comp)
	{
		RectTransformComp rectTransformComp = new RectTransformComp(comp);
		string defaultLayoutComp = GetDefaultLayoutComp("RectTransform");
		if (!string.IsNullOrEmpty(defaultLayoutComp))
		{
			RectTransformComp rectTransformComp2 = JsonUtility.FromJson<RectTransformComp>(defaultLayoutComp);
			if (rectTransformComp.Equals(rectTransformComp2))
			{
				return "";
			}
		}
		return JsonUtility.ToJson(rectTransformComp);
	}

	private string GetTextComp(Component comp)
	{
		TextComp textComp = new TextComp(comp);
		string defaultLayoutComp = GetDefaultLayoutComp("Text");
		if (!string.IsNullOrEmpty(defaultLayoutComp))
		{
			TextComp textComp2 = JsonUtility.FromJson<TextComp>(defaultLayoutComp);
			if (textComp.Equals(textComp2))
			{
				return "";
			}
		}
		return JsonUtility.ToJson(textComp);
	}

	private string GetGridLayoutGroupComp(Component comp)
	{
		GridLayoutGroupComp gridLayoutGroupComp = new GridLayoutGroupComp(comp);
		string defaultLayoutComp = GetDefaultLayoutComp("GridLayoutGroup");
		if (!string.IsNullOrEmpty(defaultLayoutComp))
		{
			GridLayoutGroupComp gridLayoutGroupComp2 = JsonUtility.FromJson<GridLayoutGroupComp>(defaultLayoutComp);
			if (gridLayoutGroupComp.Equals(gridLayoutGroupComp2))
			{
				return "";
			}
		}
		return JsonUtility.ToJson(gridLayoutGroupComp);
	}

	private string GetDefaultLayoutComp(string compType)
	{
		if (DefaultLayoutObjReference == null)
		{
			return "";
		}
		foreach (string item in DefaultLayoutObjReference.ComponentsJson)
		{
			if (string.IsNullOrEmpty(item))
			{
				return "";
			}
			if (JsonUtility.FromJson<UIComponent>(item).Type == compType)
			{
				return item;
			}
		}
		return "";
	}
}
