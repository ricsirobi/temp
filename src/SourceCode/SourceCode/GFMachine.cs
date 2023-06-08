using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GFMachine : MonoBehaviour
{
	public bool ShowPoweredByIndicator = true;

	public GFGear PoweredGear;

	[HideInInspector]
	public float maxSpeed = 1000f;

	[HideInInspector]
	public float speed = 20f;

	[HideInInspector]
	public float step;

	public bool Reverse;

	public bool AutoReverseOnReversedOrientation = true;

	private Queue<GFGear> gearUpdateQueue = new Queue<GFGear>();

	private List<GFGear> history = new List<GFGear>();

	private GFGear[] gears;

	public bool isStarted { get; private set; }

	public GFGear[] GetGears(GFGear currentGear)
	{
		List<GFGear> list = new List<GFGear>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GFGear component = base.transform.GetChild(i).gameObject.GetComponent<GFGear>();
			if (component != null && (currentGear == null || !component.gameObject.Equals(currentGear.gameObject)))
			{
				list.Add(component);
			}
		}
		return list.ToArray();
	}

	public GFGear[] GetGears()
	{
		return GetGears(null);
	}

	private void Start()
	{
		CalculateGears();
	}

	private void Update()
	{
		if (PoweredGear != null && isStarted && speed > 0f)
		{
			Step(speed);
		}
	}

	public void StartMachine()
	{
		isStarted = true;
	}

	public void StopMachine()
	{
		isStarted = false;
	}

	public void ResetMachine()
	{
		StopMachine();
		step = 0f;
		Step(0f);
	}

	public void Step(float speed)
	{
		step = Time.deltaTime * speed * (Reverse ? (-1f) : 1f);
		PoweredGear.SetRotation(step);
		gearUpdateQueue.Enqueue(PoweredGear);
		while (gearUpdateQueue.Count > 0)
		{
			GFGear gFGear = gearUpdateQueue.Dequeue();
			if (gFGear != null)
			{
				for (int i = 0; i < gFGear.childrenAsArray.Length; i++)
				{
					GFGear gFGear2 = gFGear.childrenAsArray[i];
					gFGear2.SetRotation(gFGear.angle * gFGear2.speedMultiplier);
					gearUpdateQueue.Enqueue(gFGear2);
				}
			}
		}
	}

	public void ReAlignCogs()
	{
		Queue<GFGear> queue = new Queue<GFGear>();
		queue.Enqueue(PoweredGear);
		while (queue.Count > 0)
		{
			GFGear gFGear = queue.Dequeue();
			if (gFGear != null)
			{
				for (int i = 0; i < gFGear.childrenAsArray.Length; i++)
				{
					GFGear gFGear2 = gFGear.childrenAsArray[i];
					gFGear2.gearGen.AlignPositions(gFGear.gearGen, snapPositions: true);
					queue.Enqueue(gFGear2);
				}
			}
		}
	}

	private void CalculateGears()
	{
		gears = GetGears();
		history.Clear();
		ResetHierarchy();
		if (PoweredGear != null)
		{
			RebuildTurnDirections(PoweredGear, PoweredGear.ReverseRotation, 1f);
		}
	}

	public void RecalculateGears()
	{
		CalculateGears();
	}

	private bool Opposing(GFGear g1, GFGear g2)
	{
		if (AutoReverseOnReversedOrientation)
		{
			Vector3 position = g1.transform.position;
			Vector3 position2 = g2.transform.position;
			Plane plane = g1.gameObject.CalcYAlignedCenterPlane(Space.World);
			Plane plane2 = g2.gameObject.CalcYAlignedCenterPlane(Space.World);
			Plane plane3 = new Plane(position, position + plane.normal, position2);
			Plane plane4 = new Plane(position2, position2 + plane2.normal, position);
			return plane3.normal == plane4.normal;
		}
		return false;
	}

	private float GetGearRatio(GFGear gear1, GFGear gear2)
	{
		return (float)gear1.numberOfTeeth / (float)gear2.numberOfTeeth;
	}

	private void ResetHierarchy()
	{
		GFGear[] array = gears;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
	}

	private void RebuildTurnDirections(GFGear poweredBy, bool ccw, float speedMultiplier)
	{
		if (history.Contains(poweredBy))
		{
			return;
		}
		history.Add(poweredBy);
		poweredBy.CCW = ccw;
		poweredBy.speedMultiplier = speedMultiplier;
		GFGear[] array = gears;
		foreach (GFGear gFGear in array)
		{
			int num = 0;
			while (num < gFGear.children.Count)
			{
				if (gFGear.children[num] == null)
				{
					gFGear.children.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (gFGear.DrivenBy == poweredBy)
			{
				bool flag = ccw;
				if (!poweredBy.children.Contains(gFGear))
				{
					poweredBy.children.Add(gFGear);
				}
				if (Opposing(gFGear, poweredBy) || (poweredBy.gearGen != null && poweredBy.gearGen.innerTeeth) || (gFGear.gearGen != null && gFGear.gearGen.innerTeeth))
				{
					flag = !ccw;
				}
				speedMultiplier = (gFGear.SyncSpeed ? 1f : GetGearRatio(poweredBy, gFGear));
				if (poweredBy.ReverseRotationPlusSubtree)
				{
					gFGear.ReverseRotation = true;
					flag = !flag;
				}
				RebuildTurnDirections(gFGear, !flag, speedMultiplier);
				if (gFGear.ReverseRotation && !gFGear.ReverseRotationPlusSubtree)
				{
					gFGear.CCW = !gFGear.CCW;
				}
			}
		}
	}
}
