using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
	public GFMachine machine;

	public GFGear gearToTest;

	public int numberOfInstances = 1;

	private List<GFGear> instances = new List<GFGear>();

	private void Start()
	{
		AddGears();
	}

	private void AddGears()
	{
		if (numberOfInstances < 1)
		{
			numberOfInstances = 1;
		}
		if (numberOfInstances <= 1)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < numberOfInstances - 1; i += num)
		{
			if (num2 == 1)
			{
				num2 = 0;
				num++;
			}
			else
			{
				num2++;
			}
			for (int j = 0; j < num; j++)
			{
				if (instances.Count == numberOfInstances - 1)
				{
					i = 0;
					break;
				}
				GFGear obj = ((instances.Count > 0) ? instances[instances.Count - 1] : gearToTest);
				Vector3 direction = Quaternion.AngleAxis(90f * (float)num3, Vector3.back) * Vector3.right;
				GFGear component = obj.Clone(direction).GetComponent<GFGear>();
				instances.Add(component);
			}
			num3++;
			if (num3 >= 4)
			{
				num3 = 0;
			}
		}
		machine.RecalculateGears();
	}

	private void Update()
	{
	}
}
