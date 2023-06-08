using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class LabItemCombination
{
	public delegate void CombinationEvent(LabItemCombination inCombination);

	[XmlElement(ElementName = "Action")]
	public string Action;

	[XmlElement(ElementName = "ItemName")]
	public string[] ItemNames;

	[XmlElement(ElementName = "ResultItemName")]
	public string ResultItemName;

	[XmlElement(ElementName = "ParticleData")]
	public LabAdditionObjectData ParticleData;

	private bool mInitialized;

	private GameObject mParticle;

	private CombinationEvent mEvent;

	private static int mLoadCount;

	[XmlIgnore]
	public GameObject pParticle
	{
		get
		{
			return mParticle;
		}
		set
		{
			mParticle = value;
		}
	}

	public static bool pIsLoading => mLoadCount > 0;

	public void Initialize(CombinationEvent inEvent)
	{
		mLoadCount++;
		mEvent = inEvent;
		if (mInitialized || ParticleData == null)
		{
			OnInitialized();
		}
		else
		{
			ParticleData.Initialize(OnParticleDownloaded);
		}
	}

	private void OnParticleDownloaded(LabAdditionObjectData inObjectData)
	{
		OnInitialized();
	}

	private void OnInitialized()
	{
		mInitialized = true;
		mLoadCount = Mathf.Max(0, mLoadCount - 1);
		if (mEvent != null)
		{
			mEvent(this);
		}
	}

	public bool Contains(string inItemName)
	{
		if (string.IsNullOrEmpty(inItemName) || ItemNames == null || ItemNames.Length == 0)
		{
			return false;
		}
		string[] itemNames = ItemNames;
		for (int i = 0; i < itemNames.Length; i++)
		{
			if (itemNames[i] == inItemName)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsExistInCombination(List<LabTestObject> inObjects)
	{
		if (inObjects == null || inObjects.Count == 0 || ItemNames == null || ItemNames.Length == 0)
		{
			return false;
		}
		string[] itemNames = ItemNames;
		foreach (string text in itemNames)
		{
			bool flag = false;
			foreach (LabTestObject inObject in inObjects)
			{
				if (inObject.pTestItem != null && inObject.pTestItem.Name == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsExistInCombination(string[] inItems)
	{
		if (inItems == null || inItems.Length == 0 || ItemNames == null || ItemNames.Length == 0 || inItems.Length != ItemNames.Length)
		{
			return false;
		}
		string[] itemNames = ItemNames;
		foreach (string text in itemNames)
		{
			bool flag = false;
			for (int j = 0; j < inItems.Length; j++)
			{
				if (inItems[j] == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
