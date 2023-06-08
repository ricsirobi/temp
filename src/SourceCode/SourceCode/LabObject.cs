using System.Collections.Generic;
using UnityEngine;

public class LabObject : KAMonoBase
{
	private static List<GameObject> mList = new List<GameObject>();

	protected ScientificExperiment mManager;

	public static List<GameObject> pList => mList;

	public ScientificExperiment pManager => mManager;

	public static float GetWeight(bool inCurrentWeight = true)
	{
		if (mList == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (GameObject m in mList)
		{
			if (m == null)
			{
				continue;
			}
			LabTestObject component = m.GetComponent<LabTestObject>();
			if (component != null && component.pCanWeight)
			{
				if (inCurrentWeight)
				{
					num += component.GetCurrentWeight();
				}
				else if (component.pTestItem != null)
				{
					num += component.pTestItem.Weight;
				}
			}
		}
		return num;
	}

	public static float GetResistance()
	{
		if (mList == null)
		{
			return float.NegativeInfinity;
		}
		float num = 0f;
		int num2 = 0;
		foreach (GameObject m in mList)
		{
			if (!(m == null))
			{
				LabTestObject component = m.GetComponent<LabTestObject>();
				if (component != null && component.pCanTestResistance)
				{
					num += component.pTestItem.Resistance;
					num2++;
				}
			}
		}
		if (num2 == 0)
		{
			return float.NegativeInfinity;
		}
		return num / (float)num2;
	}

	public virtual void Update()
	{
		CheckKillMarker();
	}

	public void Initialize(ScientificExperiment inManager)
	{
		mManager = inManager;
	}

	public virtual void OnEnable()
	{
		if (!mList.Contains(base.gameObject))
		{
			mList.Add(base.gameObject);
		}
	}

	public virtual void OnDisable()
	{
		if (mList.Contains(base.gameObject))
		{
			mList.Remove(base.gameObject);
		}
	}

	protected virtual bool CheckKillMarker()
	{
		if (mManager == null || mManager._KillMarker == null)
		{
			return false;
		}
		Vector3 normalized = (mManager._KillMarker.position - base.transform.position).normalized;
		Vector3 rhs = mManager._KillMarker.up * -1f;
		if (Vector3.Dot(normalized, rhs) >= 0f)
		{
			return false;
		}
		Object.Destroy(base.gameObject);
		return true;
	}
}
