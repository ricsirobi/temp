using System.Collections.Generic;
using UnityEngine;

public class RuntimeRope : MonoBehaviour
{
	public GameObject chainObject;

	public Transform pointsHolder;

	public bool connectEndPoints = true;

	public Transform pointA;

	public Transform pointB;

	public bool lockFirstChain;

	public bool lockLastChain;

	public bool connectToA = true;

	public bool connectToB = true;

	public bool hideEndChains = true;

	public bool useLineRenderer;

	public Material ropeMat;

	public float delay = 0.01f;

	public float ropeWidth;

	private GameObject chainsHolder;

	private List<Transform> pointsHolderArray;

	private Rope2D rope = new Rope2D();

	private void Start()
	{
		if (!chainObject.GetComponent<Collider2D>())
		{
			Debug.LogWarning("Chain Object Doesn't Have Collider2D Attached");
		}
		if ((bool)chainObject)
		{
			HingeJoint2D component = chainObject.GetComponent<HingeJoint2D>();
			if (!component)
			{
				Debug.LogWarning("Chain Object Doesn't Have 'HingeJoint2D' Component Attached");
			}
			else
			{
				component.enabled = false;
			}
			rope.Initialize(chainObject, 50);
		}
		else
		{
			Debug.LogWarning("Chain Object Isn't Assigned");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (!pointA && !pointsHolder)
			{
				Debug.LogWarning("PointA Isn't Assigned");
				return;
			}
			if (!pointB && !pointsHolder)
			{
				Debug.LogWarning("PointB Isn't Assigned");
				return;
			}
			if (!chainObject)
			{
				Debug.LogWarning("Chain Object Isn't Assigned");
				return;
			}
			if ((bool)pointA && (bool)pointB && pointA.GetInstanceID() == pointB.GetInstanceID())
			{
				Debug.LogWarning("Same Object Is Assigned For Both PointA and PointB");
				return;
			}
			if ((bool)pointsHolder)
			{
				pointsHolderArray = new List<Transform>();
				foreach (Transform item in pointsHolder)
				{
					pointsHolderArray.Add(item);
				}
				for (int i = 0; i < pointsHolderArray.Count - 1; i++)
				{
					pointA = pointsHolderArray[i];
					pointB = pointsHolderArray[i + 1];
					Create();
				}
				if (connectEndPoints)
				{
					pointA = pointsHolderArray[pointsHolderArray.Count - 1];
					pointB = pointsHolderArray[0];
					Create();
				}
			}
			else
			{
				Create();
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			rope.Remove();
		}
	}

	private void Create()
	{
		if (!chainObject.GetComponent<Collider2D>())
		{
			Debug.LogWarning("Chain Object Doesn't Have Collider2D Attached");
			return;
		}
		if (!chainObject.GetComponent<HingeJoint2D>())
		{
			Debug.LogWarning("Chain Object Doesn't Have HingeJoint2D Attached");
			return;
		}
		if (ropeWidth <= 0f)
		{
			ropeWidth = chainObject.GetComponent<Renderer>().bounds.size.x;
		}
		Collider component = pointA.GetComponent<Collider>();
		if ((bool)component)
		{
			Object.DestroyImmediate(component);
		}
		if (connectToA)
		{
			DistanceJoint2D component2 = pointA.GetComponent<DistanceJoint2D>();
			if (!component2 || ((bool)component2 && (bool)component2.connectedBody))
			{
				pointA.gameObject.AddComponent<DistanceJoint2D>();
				pointA.GetComponent<Rigidbody2D>().isKinematic = true;
			}
		}
		Collider component3 = pointB.GetComponent<Collider>();
		if ((bool)component3)
		{
			Object.DestroyImmediate(component3);
		}
		if (connectToB)
		{
			DistanceJoint2D component4 = pointB.GetComponent<DistanceJoint2D>();
			if (!component4 || ((bool)component4 && (bool)component4.connectedBody))
			{
				pointB.gameObject.AddComponent<DistanceJoint2D>();
				pointB.GetComponent<Rigidbody2D>().isKinematic = true;
			}
		}
		if ((int)(Vector3.Distance(pointA.position, pointB.position) / (chainObject.GetComponent<Renderer>().bounds.extents.x * 1.9f)) < 2)
		{
			Debug.LogWarning("Distance from " + pointA.name + " (PointA) to " + pointB.name + " (PointB) is very small, increase distance");
		}
		else
		{
			chainsHolder = new GameObject("Chains Holder");
			StartCoroutine(rope.CreateRopeWithDelay(chainsHolder, chainObject, pointA, pointB, lockFirstChain, lockLastChain, connectToA, connectToB, hideEndChains, useLineRenderer, ropeMat, ropeWidth, delay));
		}
	}
}
