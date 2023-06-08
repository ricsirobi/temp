using System;
using System.Collections.Generic;
using UnityEngine;

public class LabGronckleCrucible : LabCrucible
{
	private LabGronckle mGronckle;

	protected int mVomittingItemCount;

	protected float mAverageScaleTime;

	private OnStateChange mOnStateChange;

	protected List<LabItem> mCombinationSuccessList = new List<LabItem>();

	public LabGronckleCrucible(ScientificExperiment inManager, LabGronckle gronckle, float heatMultiplier, float freezeMultiplier)
		: base(inManager)
	{
		mGronckle = gronckle;
		mCurrentHeatMultiplier = heatMultiplier;
		mCurrentFreezeMultiplier = freezeMultiplier;
	}

	protected override void RecalculateTemperatureMultipliers()
	{
		base.RecalculateTemperatureMultipliers();
		mCurrentFreezeMultiplier = mGronckle._FreezeMultiplier;
	}

	protected override void AddTestItemReal(LabTestObject inItemToAdd, ItemPositionOption inPositionOption, Vector3 inPosition, Quaternion inRotation, Vector3 inScale, LabItem inParentItem, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects, CrucibleItemAddedCallback inCallback)
	{
		base.AddTestItemReal(inItemToAdd, inPositionOption, inPosition, inRotation, inScale, inParentItem, inReplaceObjects, inCallback);
		if (inItemToAdd != null && mGronckle.IsItemInBelly(inItemToAdd.pTestItem))
		{
			inItemToAdd.transform.position = Vector3.zero;
		}
	}

	protected override bool CanDragonGetExcited(LabTask task)
	{
		if (task.pDone)
		{
			return task.PlayExciteAlways;
		}
		return false;
	}

	public void DoStateChange()
	{
		if (AddCombinationResult(base.pTemperature, "FEATHER", noScaling: false))
		{
			mAverageScaleTime = 0f;
			mCombinationSuccessList.Clear();
			foreach (LabTestObject pTestItem in base.pTestItems)
			{
				if (pTestItem != null)
				{
					if (pTestItem.pState != null)
					{
						mAverageScaleTime += pTestItem.pState.ScaleOnTransitionTime;
					}
					mCombinationSuccessList.Add(pTestItem.pTestItem);
				}
			}
			mAverageScaleTime /= base.pTestItems.Count;
		}
		else if (mOnStateChange != null)
		{
			mOnStateChange();
		}
		mOnStateChange = null;
	}

	protected override void OnMixItemAdded(LabTestObject inObject)
	{
		if (!(inObject == null) && inObject.pTestItem != null && inObject.pTestItem.pCategory == LabItemCategory.LIQUID)
		{
			((LabLiquidTestObject)inObject).pCurrentScaleTime = mAverageScaleTime;
			inObject.StartScaling(inScaleDown: false, mAverageScaleTime);
		}
	}

	public bool IsItemInCombinationList(string itemName)
	{
		return mCombinationSuccessList.Find((LabItem data) => data.Name == itemName) != null;
	}

	public override void Reset()
	{
		mVomittingItemCount = 0;
		base.Reset();
		mGronckle.ClearBelly();
	}

	public override void InitStateChange(LabTestObject inItem, OnStateChange onStateChange)
	{
		if (inItem != null && mGronckle.IsItemInBelly(inItem.pTestItem))
		{
			mVomittingItemCount++;
			mOnStateChange = (OnStateChange)Delegate.Combine(mOnStateChange, onStateChange);
			if (mVomittingItemCount == mGronckle.pBellyItemCount)
			{
				mGronckle.TriggerVomit();
			}
		}
		else
		{
			base.InitStateChange(inItem, onStateChange);
		}
	}

	protected override void ProcessRule(LabTaskRule rule, bool inFuelTimeout, bool inItemAdded, ref bool success)
	{
		if (rule.Name == "Combine")
		{
			if (!IsItemInCombinationList(rule.Value))
			{
				success = false;
			}
		}
		else
		{
			base.ProcessRule(rule, inFuelTimeout, inItemAdded, ref success);
		}
	}
}
