using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Xsolla;

public abstract class ScreenBaseConroller<T> : MonoBehaviour
{
	public delegate void RecieveError(XsollaError xsollaError);

	protected Dictionary<string, GameObject> screenObjects;

	private const string PrefabStatus = "Prefabs/SimpleView/Status";

	private const string PrefabStatusWaiting = "Prefabs/SimpleView/StatusWaiting";

	private const string PrefabTitle = "Prefabs/SimpleView/TitleNoImg";

	private const string PrefabtwoTextPlate = "Prefabs/SimpleView/_ScreenCheckout/TwoTextGrayPlate";

	private const string PrefabError = "Prefabs/SimpleView/Error";

	private const string PrefabListView = "Prefabs/SimpleView/ListView";

	private const string PrefabInstructions = "Prefabs/SimpleView/Instructions";

	private const string PrefabButton = "Prefabs/SimpleView/Button";

	private const string PrefabClose = "Prefabs/SimpleView/Close";

	private const string PrefabEmpty = "Prefabs/SimpleView/Empty";

	public event Action<XsollaError> ErrorHandler;

	public ScreenBaseConroller()
	{
		screenObjects = new Dictionary<string, GameObject>();
	}

	public abstract void InitScreen(XsollaTranslations translations, T model);

	private void DrawScreen(XsollaTranslations translations, T model)
	{
	}

	protected void InitView()
	{
		foreach (KeyValuePair<string, GameObject> screenObject in screenObjects)
		{
			screenObjects[screenObject.Key] = UnityEngine.Object.Instantiate(screenObject.Value);
		}
	}

	protected GameObject GetObjectByTag(string tag)
	{
		if (screenObjects.ContainsKey(tag))
		{
			return screenObjects[tag];
		}
		return null;
	}

	protected GameObject GetOkStatus(string titleText)
	{
		if (titleText != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Status");
			SetText(@object, titleText);
			return @object;
		}
		return null;
	}

	protected GameObject GetWaitingStatus(string titleText)
	{
		if (titleText != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/StatusWaiting");
			SetText(@object, titleText);
			return @object;
		}
		return null;
	}

	protected GameObject GetTitle(string titleText)
	{
		if (titleText != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/TitleNoImg");
			SetText(@object, titleText);
			return @object;
		}
		return null;
	}

	protected GameObject GetTwoTextPlate(string titleText, string valueText)
	{
		if (titleText != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/_ScreenCheckout/TwoTextGrayPlate");
			Text[] componentsInChildren = @object.GetComponentsInChildren<Text>();
			componentsInChildren[0].text = titleText;
			componentsInChildren[1].text = valueText;
			return @object;
		}
		return null;
	}

	protected GameObject GetErrorByString(string error)
	{
		if (error != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Error");
			SetText(@object, error);
			return @object;
		}
		return null;
	}

	protected GameObject GetError(XsollaError error)
	{
		if (error != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Error");
			SetText(@object, error.GetMessage());
			return @object;
		}
		return null;
	}

	protected GameObject GetList(IBaseAdapter adapter)
	{
		if (adapter != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/ListView");
			ListView component = @object.GetComponent<ListView>();
			component.SetAdapter(adapter);
			component.DrawList();
			return @object;
		}
		return null;
	}

	protected GameObject GetTextPlate(string s)
	{
		if (s != null)
		{
			int num = s.IndexOf("<a");
			int num2 = s.IndexOf("a>");
			string text = s.Substring(num, num2 - num + 2);
			string[] array = text.Split('<', '>');
			string newValue = "<color=#a38dd8>" + array[2] + "</color>";
			s = s.Replace(text, newValue);
			GameObject @object = GetObject("Prefabs/SimpleView/Instructions");
			SetText(@object, s);
			return @object;
		}
		return null;
	}

	protected GameObject GetButton(string text, UnityAction onClick)
	{
		if (text != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Button");
			SetText(@object, text);
			@object.GetComponentInChildren<Button>().onClick.AddListener(onClick);
			return @object;
		}
		return null;
	}

	protected GameObject GetHelp(XsollaTranslations translations)
	{
		if (translations != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Help");
			Text[] componentsInChildren = @object.GetComponentsInChildren<Text>();
			componentsInChildren[0].text = translations.Get(XsollaTranslations.SUPPORT_PHONE);
			componentsInChildren[1].text = translations.Get(XsollaTranslations.SUPPORT_NEED_HELP);
			componentsInChildren[2].text = "support@xsolla.com";
			componentsInChildren[3].text = translations.Get(XsollaTranslations.SUPPORT_CUSTOMER_SUPPORT);
			return @object;
		}
		return null;
	}

	protected GameObject GetClose(UnityAction onClick)
	{
		if (onClick != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/Close");
			@object.GetComponentInChildren<Button>().onClick.AddListener(onClick);
			return @object;
		}
		return null;
	}

	protected GameObject GetEmpty()
	{
		return GetObject("Prefabs/SimpleView/Empty");
	}

	protected void OnErrorRecived(XsollaError error)
	{
		if (this.ErrorHandler != null)
		{
			this.ErrorHandler(error);
		}
	}

	public GameObject GetObject(string pathToPrefab)
	{
		return UnityEngine.Object.Instantiate(Resources.Load(pathToPrefab)) as GameObject;
	}

	public string GetFirstAHrefText(string s)
	{
		int num = s.IndexOf("<a");
		int num2 = s.IndexOf("a>");
		return s.Substring(num, num2 - num + 2).Split('<', '>')[2];
	}

	protected void SetImage(GameObject go, string imgUrl)
	{
		Image[] componentsInChildren = go.GetComponentsInChildren<Image>();
		GetComponent<ImageLoader>().LoadImage(componentsInChildren[1], imgUrl);
	}

	protected void SetText(GameObject go, string s)
	{
		go.GetComponentInChildren<Text>().text = s;
	}

	protected void SetText(Text text, string s)
	{
		text.text = s;
	}

	protected void ResizeToParent()
	{
		RectTransform component = GetComponent<RectTransform>();
		RectTransform component2 = base.transform.parent.gameObject.GetComponent<RectTransform>();
		float height = component2.rect.height;
		float width = component2.rect.width;
		float num = width / height;
		float width2 = component.rect.width;
		if (num < 1f)
		{
			component.offsetMin = new Vector2((0f - width) / 2f, (0f - height) / 2f);
			component.offsetMax = new Vector2(width / 2f, height / 2f);
			return;
		}
		float num2 = width / 3f;
		if (width2 < num2)
		{
			component.offsetMin = new Vector2((0f - num2) / 2f, (0f - height) / 2f);
			component.offsetMax = new Vector2(num2 / 2f, height / 2f);
		}
	}
}
