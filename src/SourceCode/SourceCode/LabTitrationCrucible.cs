using System;
using System.Collections.Generic;
using UnityEngine;

public class LabTitrationCrucible : LabCrucible
{
	private LabTitration mTitration;

	private List<LabItem> mAddedSolutionItems = new List<LabItem>();

	private LabLiquidTestObject mTitrationMixture;

	private LabColorAgentTestObject.PHColorBracket[] mPHColorBrackets;

	private bool mUpdateValues;

	private Color mNewColor = Color.white;

	private int mNewPHValue = 7;

	private int mTotalAcidityAdded;

	private int mAcidityBracket;

	private int mGoalAcidityBracket;

	private LabItem mAcid;

	private LabItem mBase;

	private string mTitrationAgentName = "";

	private string mTestItemName = "";

	private string mTestItemLocalisedName = "";

	private int mTestItemAcidity;

	private bool mResultShown;

	private bool mGoalColorShown;

	public bool pIsTitrationAgentPresent { get; set; }

	public LabTitrationCrucible(ScientificExperiment inManager, LabTitration titration)
		: base(inManager)
	{
		mTitration = titration;
		UtDebug.Log(mTitration.gameObject.name);
	}

	protected override void AddTestItemReal(LabTestObject inItemToAdd, ItemPositionOption inPositionOption, Vector3 inPosition, Quaternion inRotation, Vector3 inScale, LabItem inParentItem, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects, CrucibleItemAddedCallback inCallback)
	{
		base.AddTestItemReal(inItemToAdd, inPositionOption, inPosition, inRotation, inScale, inParentItem, inReplaceObjects, inCallback);
		bool flag = false;
		LabColorAgentTestObject labColorAgentTestObject = inItemToAdd as LabColorAgentTestObject;
		if (labColorAgentTestObject != null)
		{
			flag = true;
		}
		if (!pIsTitrationAgentPresent && flag)
		{
			pIsTitrationAgentPresent = true;
			mTitrationAgentName = inItemToAdd.pTestItem.Name;
			mPHColorBrackets = new LabColorAgentTestObject.PHColorBracket[labColorAgentTestObject._PHColorBracket.Length];
			Array.Copy(labColorAgentTestObject._PHColorBracket, mPHColorBrackets, labColorAgentTestObject._PHColorBracket.Length);
		}
		if (inItemToAdd != null && inItemToAdd.pTestItem.pCategory == LabItemCategory.SOLID)
		{
			if (!flag)
			{
				mAddedSolutionItems.Add(inItemToAdd.pTestItem);
				mTotalAcidityAdded += inItemToAdd.pTestItem.AcidityBracket;
				mAcidityBracket = mTotalAcidityAdded / mAddedSolutionItems.Count;
			}
			if (!mGoalColorShown && pIsTitrationAgentPresent && mAddedSolutionItems.Count > 0)
			{
				LabColorAgentTestObject.PHColorBracket pHColorBracket = Array.Find(mPHColorBrackets, (LabColorAgentTestObject.PHColorBracket x) => x._ColorBracket.IsInRange(mGoalAcidityBracket));
				mManager._MainUI.SetTitrationGoalColor(pHColorBracket._Color);
				mManager._MainUI.SetTitrationPHMenu(mPHColorBrackets);
				mGoalColorShown = true;
			}
			if (!mGoalColorShown)
			{
				if (!string.IsNullOrEmpty(inItemToAdd.pTestItem.ColorValue))
				{
					mNewColor = LabStringUtil.Parse(inItemToAdd.pTestItem.ColorValue, Color.white);
					mUpdateValues = true;
				}
			}
			else
			{
				UpdateColorAgentAcidity();
			}
		}
		if (mTitrationMixture == null)
		{
			mTitrationMixture = inItemToAdd as LabLiquidTestObject;
		}
	}

	public void AddAcidity(int inQuantity)
	{
		if (pIsTitrationAgentPresent && mAcid != null && mBase != null)
		{
			bool flag = false;
			if (inQuantity < 0 && mAcidityBracket < mAcid.AcidityBracket)
			{
				mAcidityBracket += inQuantity * -1;
				flag = true;
			}
			else if (inQuantity > 0 && mAcidityBracket > mBase.AcidityBracket)
			{
				mAcidityBracket += inQuantity * -1;
				flag = true;
			}
			if (!flag)
			{
				mAcidityBracket += inQuantity;
			}
			if (inQuantity < 0 && mAcidityBracket < mAcid.AcidityBracket)
			{
				mAcidityBracket = mAcid.AcidityBracket;
			}
			else if (inQuantity > 0 && mAcidityBracket > mBase.AcidityBracket)
			{
				mAcidityBracket = mBase.AcidityBracket;
			}
			UpdateColorAgentAcidity();
		}
	}

	private void UpdateColorAgentAcidity()
	{
		if (mPHColorBrackets != null || mPHColorBrackets.Length > 1)
		{
			LabColorAgentTestObject.PHColorBracket pHColorBracket = Array.Find(mPHColorBrackets, (LabColorAgentTestObject.PHColorBracket x) => x._ColorBracket.IsInRange(mAcidityBracket));
			if (pHColorBracket._ColorBracket.Min == float.NegativeInfinity)
			{
				mAcidityBracket = (int)pHColorBracket._ColorBracket.Max;
			}
			if (pHColorBracket._ColorBracket.Max == float.PositiveInfinity)
			{
				mAcidityBracket = (int)pHColorBracket._ColorBracket.Min;
			}
			mNewColor = pHColorBracket._Color;
			mNewPHValue = pHColorBracket._PHValue;
			mUpdateValues = true;
		}
	}

	public Color GetCombinedColor(Color inColor)
	{
		return Color.Lerp(mTitrationMixture._Renderer.material.color, inColor, 1f / (float)mAddedSolutionItems.Count);
	}

	public override void DoUpdate()
	{
		base.DoUpdate();
		if (mTitrationMixture != null && mTitrationMixture.pInitializedMaterial && mUpdateValues && !mManager.pTitrationActive)
		{
			mUpdateValues = false;
			if (!pIsTitrationAgentPresent)
			{
				mNewColor = GetCombinedColor(mNewColor);
			}
			if (mGoalColorShown)
			{
				mManager._MainUI.SetTitrationPHValue(mNewPHValue.ToString());
			}
			mTitrationMixture._Renderer.material.color = mNewColor;
		}
	}

	protected override bool CanDragonGetExcited(LabTask task)
	{
		return task.PlayExciteAlways;
	}

	protected override void ProcessRule(LabTaskRule rule, bool inFuelTimeout, bool inItemAdded, ref bool success)
	{
		if (!pIsTitrationAgentPresent || (mManager._AcidStream != null && mManager._AcidStream.emission.enabled) || (mManager._BaseStream != null && mManager._BaseStream.emission.enabled))
		{
			success = false;
			return;
		}
		string name = rule.Name;
		if (!(name == "TestItem"))
		{
			if (!(name == "AcidityBracket"))
			{
				return;
			}
			if (mGoalAcidityBracket != mAcidityBracket || mAddedSolutionItems.Count == 0)
			{
				success = false;
			}
			if (success && !mResultShown)
			{
				mResultShown = true;
				LabItem item = LabData.pInstance.GetItem(mTestItemName);
				if (item != null)
				{
					mTestItemName = rule.Value;
					mTestItemAcidity = item.AcidityBracket;
					mTestItemLocalisedName = item.DisplayNameText.GetLocalizedString();
				}
				else
				{
					mTestItemLocalisedName = "";
				}
				if (!string.IsNullOrEmpty(mTestItemLocalisedName))
				{
					string text = "";
					text = ((mTestItemAcidity >= mGoalAcidityBracket) ? mManager._TitrationAcidNeutralText.GetLocalizedString() : mManager._TitrationBaseNeutralText.GetLocalizedString());
					text = text.Replace("{{ITEM}}", mTestItemLocalisedName);
					text = text.Replace("{{ACIDITY}}", Mathf.Abs(mGoalAcidityBracket - mTestItemAcidity).ToString());
					mManager._MainUI.UpdateExperimentResultText(text);
				}
			}
		}
		else
		{
			if (mAddedSolutionItems.Exists((LabItem x) => x.Name != rule.Value) || !mAddedSolutionItems.Exists((LabItem x) => x.Name == rule.Value))
			{
				success = false;
			}
			if (success && mTestItemName != rule.Value)
			{
				mTestItemName = rule.Value;
			}
		}
	}

	public override void OnLabTaskUpdated(LabTask inTask)
	{
		mManager._MainUI.UpdateExperimentResultText("");
		mResultShown = false;
		for (int i = 0; i < inTask.Rules.Length; i++)
		{
			LabTaskRule labTaskRule = inTask.Rules[i];
			switch (labTaskRule.Name)
			{
			case "Acid":
				mAcid = LabData.pInstance.GetItem(labTaskRule.Value);
				if (mAcid == null)
				{
					UtDebug.Log("Acid rule is not defined!!!");
				}
				break;
			case "Base":
				mBase = LabData.pInstance.GetItem(labTaskRule.Value);
				if (mBase == null)
				{
					UtDebug.Log("Base rule is not defined!!!");
				}
				break;
			case "TestItem":
			{
				LabItem item = LabData.pInstance.GetItem(labTaskRule.Value);
				mTestItemAcidity = item.AcidityBracket;
				if (item != null)
				{
					mTestItemName = item.Name;
					mTestItemLocalisedName = item.DisplayNameText.GetLocalizedString();
				}
				else
				{
					mTestItemLocalisedName = "";
				}
				break;
			}
			case "AcidityBracket":
			{
				int result = 0;
				if (int.TryParse(labTaskRule.Value, out result))
				{
					mGoalAcidityBracket = result;
				}
				break;
			}
			}
		}
	}

	public override bool HasItemInCrucible(string inItemName)
	{
		if (mAddedSolutionItems == null || mAddedSolutionItems.Count == 0)
		{
			return false;
		}
		if (pIsTitrationAgentPresent && inItemName == mTitrationAgentName)
		{
			return true;
		}
		foreach (LabItem mAddedSolutionItem in mAddedSolutionItems)
		{
			if (mAddedSolutionItem != null && mAddedSolutionItem.Name == inItemName)
			{
				return true;
			}
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		mAddedSolutionItems.Clear();
		pIsTitrationAgentPresent = false;
		mTitrationMixture = null;
		mAcidityBracket = 0;
		mTotalAcidityAdded = 0;
		mTitrationAgentName = "";
		mUpdateValues = false;
		mGoalColorShown = false;
		mPHColorBrackets = new LabColorAgentTestObject.PHColorBracket[0];
	}
}
