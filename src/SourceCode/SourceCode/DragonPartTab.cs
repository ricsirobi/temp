using System;
using UnityEngine;

[Serializable]
public class DragonPartTab
{
	public string _PrtTypeName = "";

	public int[] _DefaultStoreIDs;

	public string _BtnName = "";

	public GameObject _SelectionPanel;

	public int _NumCol = 2;

	public int _NumRow = 4;

	public int _WHSize = 120;

	public string _IntroTxtName = "";

	public string _NoItemTxtName = "";

	public int _NoItemNum = 1;

	public AudioClip _NoItemVO;

	public AudioClip _DisclaimVO;

	public bool _ShowForAllDragons;

	public void OnSelected(bool t, KAUI ui)
	{
		KAWidget kAWidget = ui.FindItem(_IntroTxtName);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(t);
		}
		if (_SelectionPanel != null)
		{
			_SelectionPanel.SetActive(t);
		}
		if (!t)
		{
			kAWidget = ui.FindItem(_NoItemTxtName);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(t);
			}
		}
	}

	public void OnItemReady(int numItems, KAUI ui)
	{
		if (numItems <= _NoItemNum)
		{
			if (ui != null)
			{
				KAWidget kAWidget = ui.FindItem(_NoItemTxtName);
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
			if (_NoItemVO != null)
			{
				SnChannel.Play(_NoItemVO, "VO_Pool", inForce: true, null);
			}
		}
		else if (_DisclaimVO != null)
		{
			SnChannel.Play(_DisclaimVO, "VO_Pool", inForce: true, null);
		}
	}
}
