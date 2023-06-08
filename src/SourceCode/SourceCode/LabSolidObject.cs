using System;
using System.Collections.Generic;
using UnityEngine;

public class LabSolidObject : LabTestObject
{
	[Serializable]
	public enum LabSubstanceType
	{
		FLOAT,
		COLOR
	}

	[Serializable]
	public class LabSubstanceValue
	{
		public float _ValueFloat;

		public Color _Color;
	}

	[Serializable]
	public class LabSubstanceData
	{
		public string _Property;

		public LabSubstanceType _Type;

		public LabSubstanceValue _DataMin;

		public LabSubstanceValue _DataMax;
	}

	[Serializable]
	public class LabSubstanceStateData
	{
		public string _StateName;

		public LabSubstanceData[] _SubstanceData;
	}

	private Dictionary<LabLiquidTestObject, float> mLiquidInteractionData;

	public override Vector3 pTopPosition
	{
		get
		{
			Vector3 result = Vector3.zero;
			if (_Renderer != null)
			{
				result = collider.bounds.center;
				result.y += collider.bounds.extents.y;
			}
			return result;
		}
	}

	public override Vector3 pBottomPosition
	{
		get
		{
			Vector3 result = Vector3.zero;
			if (collider != null)
			{
				result = collider.bounds.center;
				result.y -= collider.bounds.extents.y;
			}
			return result;
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		base.Update();
		HandleInteractionWithLiquid();
	}

	private void HandleInteractionWithLiquid()
	{
		if (!IsDippedInInteractingLiquids(out var inDippedLiqids))
		{
			return;
		}
		foreach (LabLiquidTestObject item in inDippedLiqids)
		{
			if (item == null)
			{
				continue;
			}
			if (mLiquidInteractionData == null)
			{
				mLiquidInteractionData = new Dictionary<LabLiquidTestObject, float>();
			}
			if (!mLiquidInteractionData.ContainsKey(item))
			{
				mLiquidInteractionData.Add(item, 0f);
			}
			float num = mLiquidInteractionData[item];
			mLiquidInteractionData[item] += Time.deltaTime;
			LabInteraction interaction = mTestItem.GetInteraction(item.pTestItem.Name);
			if (interaction != null)
			{
				float duration = interaction.Duration;
				UpdateShader(Mathf.Min(1f, Mathf.Max(0f, num / duration)), interaction.Interpolations);
				if (num < duration && mLiquidInteractionData[item] >= duration)
				{
					OnInteractionDone(item);
				}
			}
		}
	}

	public float GetDippedPosition(LabLiquidTestObject inLiquidObject)
	{
		if (inLiquidObject == null)
		{
			return 0f;
		}
		float num = pTopPosition.y - pBottomPosition.y;
		float num2 = inLiquidObject.pTopPosition.y - pBottomPosition.y;
		if (num2 < 0f)
		{
			return 0f;
		}
		if (num2 > num)
		{
			return 1f;
		}
		return num2 / num;
	}

	public bool IsDippedInInteractingLiquids(out List<LabLiquidTestObject> inDippedLiqids)
	{
		inDippedLiqids = null;
		List<LabTestObject> crucibleItems = base.pCrucible.GetCrucibleItems(LabItemCategory.LIQUID);
		if (crucibleItems == null || crucibleItems.Count == 0)
		{
			return false;
		}
		bool result = false;
		foreach (LabTestObject item in crucibleItems)
		{
			if (!(item == null) && CanInteractWith(item) && !(GetDippedPosition(item as LabLiquidTestObject) <= 0f) && base.pCrucible.HasItemInCrucible(this))
			{
				if (inDippedLiqids == null)
				{
					inDippedLiqids = new List<LabLiquidTestObject>();
				}
				inDippedLiqids.Add(item as LabLiquidTestObject);
				result = true;
			}
		}
		return result;
	}

	public bool CanInteractWith(LabTestObject inTestObject)
	{
		if (inTestObject != null && inTestObject.pTestItem != null)
		{
			return mTestItem.CanInteractWith(inTestObject.pTestItem.Name);
		}
		return false;
	}

	public void OnInteractionDone(LabLiquidTestObject inLiquidTestObjecct)
	{
	}

	public override bool IsInteractionDone(LabTestObject inTestObject)
	{
		if (mLiquidInteractionData != null && mLiquidInteractionData.Count > 0 && mLiquidInteractionData.ContainsKey(inTestObject as LabLiquidTestObject))
		{
			return mLiquidInteractionData[inTestObject as LabLiquidTestObject] >= mTestItem.GetInteraction(inTestObject.pTestItem.Name).Duration;
		}
		return false;
	}
}
