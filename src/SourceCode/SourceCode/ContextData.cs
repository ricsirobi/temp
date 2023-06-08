using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContextData : ICloneable
{
	public string _Name;

	public string _ButtonName;

	public LocaleString _DisplayName;

	public LocaleString _ToolTipText;

	public string[] _ChildrenNames;

	public bool _DeactivateOnClick = true;

	public string _ItemTemplateName;

	public string _IconSpriteName;

	public LocaleString _LabelText;

	public Color _BackgroundColor = new Color(0.1f, 1f, 0.1f, 0.4f);

	public Vector2 _2DScaleInPixels = new Vector2(120f, 56f);

	public Vector2 _WidgetSize = new Vector2(128f, 128f);

	[NonSerialized]
	private List<ContextData> mChildrenDataList = new List<ContextData>();

	public GameObject pTarget { get; set; }

	public ContextData pParent { get; set; }

	public UserItemData pUserItemData { get; set; }

	public List<ContextData> pChildrenDataList
	{
		get
		{
			return mChildrenDataList;
		}
		set
		{
			mChildrenDataList = value;
		}
	}

	public bool pIsChildOpened { get; set; }

	public Vector3 pPosition { get; set; }

	public bool pEnabled { get; set; }

	public object Clone()
	{
		ContextData obj = (ContextData)MemberwiseClone();
		obj._DisplayName = new LocaleString(_DisplayName._Text);
		obj.mChildrenDataList = new List<ContextData>();
		return obj;
	}

	public void AddChildName(string inName)
	{
		List<string> list = new List<string>(_ChildrenNames);
		list.Add(inName);
		_ChildrenNames = list.ToArray();
	}

	public void RemoveChildName(string inName)
	{
		List<string> list = new List<string>(_ChildrenNames);
		list.Remove(inName);
		_ChildrenNames = list.ToArray();
	}

	public void RemoveAllChildren()
	{
		List<string> list = new List<string>();
		_ChildrenNames = list.ToArray();
	}
}
