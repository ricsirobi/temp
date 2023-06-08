using System;
using System.Collections.Generic;
using JSGames.UI.MultiResolution;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIMultiResolution : MonoBehaviour
{
	[Serializable]
	public class DeviceConfiguration
	{
		public string _DeviceName;

		public TextAsset _Config;
	}

	[Serializable]
	public class AspectRatioConfiguration
	{
		public float _MinAspectRatio;

		public float _MaxAspectRatio;

		public TextAsset _Config;
	}

	public List<DeviceConfiguration> _DeviceConfigs = new List<DeviceConfiguration>();

	public List<AspectRatioConfiguration> _AspectRatioConfig = new List<AspectRatioConfiguration>();

	protected void Awake()
	{
		UpdateUi();
	}

	private void UpdateUi()
	{
		string deviceName = SystemInfo.deviceName;
		if (!UpdateUiForDevice(deviceName))
		{
			UpdateUiForResolution(Screen.width, Screen.height);
		}
	}

	private bool UpdateUiForDevice(string deviceName)
	{
		foreach (DeviceConfiguration deviceConfig in _DeviceConfigs)
		{
			if (deviceConfig._DeviceName == deviceName)
			{
				UpdateUiForConfig(deviceConfig._Config);
				return true;
			}
		}
		return false;
	}

	private void UpdateUiForResolution(float screenWidth, float screenHeight)
	{
		float aspectRatio = screenWidth / screenHeight;
		UpdateUiForAspectRatio(aspectRatio);
	}

	private void UpdateUiForAspectRatio(float aspectRatio)
	{
		foreach (AspectRatioConfiguration item in _AspectRatioConfig)
		{
			if (aspectRatio >= item._MinAspectRatio && aspectRatio <= item._MaxAspectRatio)
			{
				UpdateUiForConfig(item._Config);
				break;
			}
		}
	}

	private void UpdateUiForConfig(TextAsset configuration)
	{
		foreach (UIGameObject gameObject in JsonUtility.FromJson<UILayout>(configuration.text).GameObjects)
		{
			Transform transform = base.transform;
			if (!string.IsNullOrEmpty(gameObject.Hierarchy))
			{
				transform = base.transform.Find(gameObject.Hierarchy);
			}
			if (transform == null)
			{
				Debug.LogWarning("Couldn't find the game object " + gameObject.Hierarchy + "  in the gameobject " + base.transform.name);
				continue;
			}
			foreach (string item in gameObject.ComponentsJson)
			{
				UpdateComponent(transform, item);
			}
		}
	}

	private void UpdateComponent(Transform trans, string compJson)
	{
		switch (JsonUtility.FromJson<UIComponent>(compJson).Type)
		{
		case "Canvas":
			UpdateCanvasCompConfig(trans, compJson);
			break;
		case "CanvasScaler":
			UpdateCanvasScalerConfig(trans, compJson);
			break;
		case "RectTransform":
			UpdateRectTransformConfig(trans, compJson);
			break;
		case "Text":
			UpdateText(trans, compJson);
			break;
		case "GridLayoutGroup":
			UpdateGridLayoutGroup(trans, compJson);
			break;
		}
	}

	private void UpdateCanvasCompConfig(Transform trans, string canvasJson)
	{
		CanvasComp canvasComp = JsonUtility.FromJson<CanvasComp>(canvasJson);
		Canvas component = trans.GetComponent<Canvas>();
		component.pixelPerfect = canvasComp.PixelPerfect;
		component.overridePixelPerfect = canvasComp.OverridePixelPerfect;
		if (component.overridePixelPerfect)
		{
			component.renderMode = canvasComp.RenderMode;
		}
		component.sortingOrder = canvasComp.SortOrder;
	}

	private void UpdateCanvasScalerConfig(Transform trans, string scalerJson)
	{
		CanvasScalerComp canvasScalerComp = JsonUtility.FromJson<CanvasScalerComp>(scalerJson);
		CanvasScaler component = trans.GetComponent<CanvasScaler>();
		component.referenceResolution = canvasScalerComp.ScalerResolution;
		component.uiScaleMode = canvasScalerComp.UIScaleMode;
		component.screenMatchMode = canvasScalerComp.ScreenMatchMode;
	}

	private void UpdateRectTransformConfig(Transform trans, string rectJson)
	{
		RectTransformComp rectTransformComp = JsonUtility.FromJson<RectTransformComp>(rectJson);
		RectTransform component = trans.GetComponent<RectTransform>();
		component.anchoredPosition = rectTransformComp.AnchoredPosition;
		component.anchorMax = rectTransformComp.AnchorMax;
		component.anchorMin = rectTransformComp.AnchorMin;
		component.sizeDelta = rectTransformComp.SizeDelta;
		component.pivot = rectTransformComp.Pivot;
		component.rotation = rectTransformComp.Rotation;
		component.localScale = rectTransformComp.Scale;
	}

	private void UpdateText(Transform trans, string textJson)
	{
		TextComp textComp = JsonUtility.FromJson<TextComp>(textJson);
		Text component = trans.GetComponent<Text>();
		component.fontSize = textComp.FontSize;
		component.lineSpacing = textComp.LineSpacing;
		component.supportRichText = textComp.RichText;
		component.alignment = textComp.Alignment;
		component.resizeTextForBestFit = textComp.BestFit;
		component.resizeTextMaxSize = textComp.BestFitMax;
		component.resizeTextMinSize = textComp.BestFitMin;
		component.alignByGeometry = textComp.AlignByGeometry;
		component.horizontalOverflow = textComp.HorizontalWrapMode;
		component.verticalOverflow = textComp.VerticalWrapMode;
	}

	private void UpdateGridLayoutGroup(Transform trans, string gridLayoutJson)
	{
		GridLayoutGroupComp gridLayoutGroupComp = JsonUtility.FromJson<GridLayoutGroupComp>(gridLayoutJson);
		GridLayoutGroup component = trans.GetComponent<GridLayoutGroup>();
		component.padding = gridLayoutGroupComp.Padding;
		component.cellSize = gridLayoutGroupComp.CellSize;
		component.spacing = gridLayoutGroupComp.Spacing;
	}
}
