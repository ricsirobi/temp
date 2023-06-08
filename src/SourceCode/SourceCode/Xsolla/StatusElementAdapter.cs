using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class StatusElementAdapter : IBaseAdapter
{
	private GameObject statusPrefab;

	private List<XsollaStatusText.StatusTextElement> elements;

	public void Awake()
	{
		statusPrefab = Resources.Load("Prefabs/SimpleView/_StatusElements/XsollaStatusElement") as GameObject;
	}

	public override int GetElementType(int id)
	{
		return 0;
	}

	public override int GetCount()
	{
		return elements.Count;
	}

	public override GameObject GetView(int position)
	{
		XsollaStatusText.StatusTextElement statusTextElement = elements[position];
		GameObject obj = UnityEngine.Object.Instantiate(statusPrefab);
		Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
		componentsInChildren[0].text = statusTextElement.GetPref();
		componentsInChildren[1].text = statusTextElement.GetValue();
		return obj;
	}

	public override GameObject GetPrefab()
	{
		return statusPrefab;
	}

	public void SetElements(List<XsollaStatusText.StatusTextElement> elements)
	{
		this.elements = elements;
	}

	public override GameObject GetNext()
	{
		throw new NotImplementedException();
	}
}
