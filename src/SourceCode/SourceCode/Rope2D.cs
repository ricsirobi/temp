using System.Collections;
using UnityEngine;

public class Rope2D
{
	private Transform objHolder;

	private GameObject availableChainsHolder;

	private PoolingSystem ropePool;

	public void Initialize(GameObject chainObject, int count)
	{
		if (!availableChainsHolder)
		{
			availableChainsHolder = GameObject.Find("Available Chains Holder");
			if (!availableChainsHolder)
			{
				availableChainsHolder = new GameObject("Available Chains Holder");
			}
		}
		ropePool = new PoolingSystem(chainObject, count, "availableChain", availableChainsHolder.transform);
	}

	public void CreateRope(GameObject objectsHolder, GameObject chainObject, Transform pointA, Transform pointB, bool lockFirstChain, bool lockLastChain, bool connectToA, bool connectToB, bool hideEndObjects, bool useLineRenderer, Material ropeMat, float ropeWidth)
	{
		objHolder = objectsHolder.transform;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		Transform transform = null;
		num = (int)(Vector3.Distance(pointA.position, pointB.position) / (chainObject.GetComponent<Renderer>().bounds.extents.x * 1.9f));
		while (!flag)
		{
			Transform transform2 = ((ropePool == null) ? Object.Instantiate(chainObject).transform : ropePool.Create().transform);
			if (useLineRenderer)
			{
				transform2.GetComponent<Renderer>().enabled = false;
			}
			else
			{
				transform2.GetComponent<Renderer>().enabled = true;
			}
			if (!transform)
			{
				transform2.position = pointA.position;
				transform2.name = "chain 1";
				transform2.GetComponent<Collider2D>().isTrigger = true;
				if (hideEndObjects)
				{
					transform2.GetComponent<Renderer>().enabled = false;
				}
				if (connectToA)
				{
					DistanceJoint2D[] componentsInChildren = pointA.GetComponentsInChildren<DistanceJoint2D>();
					DistanceJoint2D distanceJoint2D = new DistanceJoint2D();
					DistanceJoint2D[] array = componentsInChildren;
					foreach (DistanceJoint2D distanceJoint2D2 in array)
					{
						if (!distanceJoint2D2.connectedBody)
						{
							distanceJoint2D = distanceJoint2D2;
						}
					}
					distanceJoint2D.distance = 0.01f;
					distanceJoint2D.connectedBody = transform2.GetComponent<Rigidbody2D>();
					pointA.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				else
				{
					transform2.GetComponent<Rigidbody2D>().isKinematic = lockFirstChain;
				}
			}
			else
			{
				transform2.position = transform.position + (pointB.position - transform.position) / num;
				transform2.name = "chain " + (1 + num2);
			}
			Quaternion rotation = Quaternion.LookRotation(pointA.position - pointB.position, Vector3.up);
			rotation.x = 0f;
			rotation.y = 0f;
			transform2.rotation = rotation;
			if ((bool)transform)
			{
				HingeJoint2D component = transform.GetComponent<HingeJoint2D>();
				Vector3 position = transform.position + (transform2.position - transform.position).normalized * chainObject.GetComponent<Renderer>().bounds.extents.x;
				component.anchor = transform.transform.InverseTransformPoint(position);
				component.connectedAnchor = transform2.transform.InverseTransformPoint(position);
				component.connectedBody = transform2.GetComponent<Rigidbody2D>();
				component.enabled = true;
				if (num2 == 1)
				{
					HingeJoint2D hingeJoint2D = transform2.gameObject.AddComponent<HingeJoint2D>();
					hingeJoint2D.connectedBody = transform.GetComponent<Rigidbody2D>();
					if (!connectToA)
					{
						hingeJoint2D.anchor = new Vector2(0f - component.anchor.x, 0f - component.anchor.y);
						hingeJoint2D.connectedAnchor = new Vector2(0f - component.connectedAnchor.x, 0f - component.connectedAnchor.y);
					}
					else
					{
						hingeJoint2D.anchor = hingeJoint2D.transform.InverseTransformPoint(transform.position);
					}
					Object.DestroyImmediate(component);
				}
			}
			num = (int)(Vector3.Distance(transform2.position, pointB.position) / (chainObject.GetComponent<Renderer>().bounds.extents.x * 1.9f));
			if (num < 1)
			{
				HingeJoint2D component2 = transform.GetComponent<HingeJoint2D>();
				component2.useLimits = false;
				if (connectToB)
				{
					component2.connectedAnchor = Vector2.zero;
					component2.anchor = transform.transform.InverseTransformPoint(transform2.position);
				}
				transform2.GetComponent<Collider2D>().isTrigger = true;
				if (hideEndObjects)
				{
					transform2.GetComponent<Renderer>().enabled = false;
				}
				if (connectToB)
				{
					DistanceJoint2D[] componentsInChildren2 = pointB.GetComponentsInChildren<DistanceJoint2D>();
					DistanceJoint2D distanceJoint2D3 = new DistanceJoint2D();
					DistanceJoint2D[] array = componentsInChildren2;
					foreach (DistanceJoint2D distanceJoint2D4 in array)
					{
						if (!distanceJoint2D4.connectedBody)
						{
							distanceJoint2D3 = distanceJoint2D4;
						}
					}
					distanceJoint2D3.distance = 0.01f;
					distanceJoint2D3.connectedBody = transform2.GetComponent<Rigidbody2D>();
					pointB.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				else if (lockLastChain)
				{
					transform2.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				Object.DestroyImmediate(transform2.GetComponent<HingeJoint2D>());
				if (useLineRenderer)
				{
					UseLineRenderer useLineRenderer2 = objectsHolder.AddComponent<UseLineRenderer>();
					useLineRenderer2.ropeMaterial = ropeMat;
					useLineRenderer2.width = ropeWidth;
				}
				flag = true;
			}
			transform2.parent = objectsHolder.transform;
			transform = transform2;
			num2++;
		}
	}

	public IEnumerator CreateRopeWithDelay(GameObject objectsHolder, GameObject chainObject, Transform pointA, Transform pointB, bool lockFirstChain, bool lockLastChain, bool connectToA, bool connectToB, bool hideEndObjects, bool useLineRenderer, Material ropeMat, float ropeWidth, float delay)
	{
		objHolder = objectsHolder.transform;
		int chainIndex = 0;
		bool connected = false;
		Transform tempChain = null;
		int remainChainCount = (int)(Vector3.Distance(pointA.position, pointB.position) / (chainObject.GetComponent<Renderer>().bounds.extents.x * 1.9f));
		UseLineRenderer useLinerend = null;
		if (useLineRenderer)
		{
			useLinerend = objectsHolder.AddComponent<UseLineRenderer>();
			useLinerend.ropeMaterial = ropeMat;
			useLinerend.width = ropeWidth;
		}
		while (!connected)
		{
			Transform transform = ((ropePool == null) ? Object.Instantiate(chainObject).transform : ropePool.Create().transform);
			if (useLineRenderer)
			{
				transform.GetComponent<Renderer>().enabled = false;
			}
			else
			{
				transform.GetComponent<Renderer>().enabled = true;
			}
			if (!tempChain)
			{
				transform.position = pointA.position;
				transform.name = "chain 1";
				transform.GetComponent<Collider2D>().isTrigger = true;
				if (hideEndObjects)
				{
					transform.GetComponent<Renderer>().enabled = false;
				}
				if (connectToA)
				{
					DistanceJoint2D[] componentsInChildren = pointA.GetComponentsInChildren<DistanceJoint2D>();
					DistanceJoint2D distanceJoint2D = new DistanceJoint2D();
					DistanceJoint2D[] array = componentsInChildren;
					foreach (DistanceJoint2D distanceJoint2D2 in array)
					{
						if (!distanceJoint2D2.connectedBody)
						{
							distanceJoint2D = distanceJoint2D2;
						}
					}
					distanceJoint2D.distance = 0.01f;
					distanceJoint2D.connectedBody = transform.GetComponent<Rigidbody2D>();
					pointA.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				else
				{
					transform.GetComponent<Rigidbody2D>().isKinematic = lockFirstChain;
				}
			}
			else
			{
				transform.position = tempChain.position + (pointB.position - tempChain.position) / remainChainCount;
				transform.name = "chain " + (1 + chainIndex);
			}
			Quaternion rotation = Quaternion.LookRotation(pointA.position - pointB.position, Vector3.up);
			rotation.x = 0f;
			rotation.y = 0f;
			transform.rotation = rotation;
			if (useLineRenderer)
			{
				if (!useLinerend)
				{
					break;
				}
				useLinerend.AddChain(transform);
			}
			if ((bool)tempChain)
			{
				HingeJoint2D component = tempChain.GetComponent<HingeJoint2D>();
				Vector3 position = tempChain.position + (transform.position - tempChain.position).normalized * chainObject.GetComponent<Renderer>().bounds.extents.x;
				component.anchor = tempChain.transform.InverseTransformPoint(position);
				component.connectedAnchor = transform.transform.InverseTransformPoint(position);
				component.connectedBody = transform.GetComponent<Rigidbody2D>();
				component.enabled = true;
				if (chainIndex == 1)
				{
					HingeJoint2D hingeJoint2D = transform.gameObject.AddComponent<HingeJoint2D>();
					hingeJoint2D.connectedBody = tempChain.GetComponent<Rigidbody2D>();
					if (!connectToA)
					{
						hingeJoint2D.anchor = new Vector2(0f - component.anchor.x, 0f - component.anchor.y);
						hingeJoint2D.connectedAnchor = new Vector2(0f - component.connectedAnchor.x, 0f - component.connectedAnchor.y);
					}
					else
					{
						hingeJoint2D.anchor = hingeJoint2D.transform.InverseTransformPoint(tempChain.position);
					}
					Object.DestroyImmediate(component);
				}
			}
			remainChainCount = (int)(Vector3.Distance(transform.position, pointB.position) / (chainObject.GetComponent<Renderer>().bounds.extents.x * 1.9f));
			if (remainChainCount < 1)
			{
				HingeJoint2D component2 = tempChain.GetComponent<HingeJoint2D>();
				component2.useLimits = false;
				if (connectToB)
				{
					component2.connectedAnchor = Vector2.zero;
					component2.anchor = tempChain.transform.InverseTransformPoint(transform.position);
				}
				transform.GetComponent<Collider2D>().isTrigger = true;
				if (hideEndObjects)
				{
					transform.GetComponent<Renderer>().enabled = false;
				}
				if (connectToB)
				{
					DistanceJoint2D[] componentsInChildren2 = pointB.GetComponentsInChildren<DistanceJoint2D>();
					DistanceJoint2D distanceJoint2D3 = new DistanceJoint2D();
					DistanceJoint2D[] array = componentsInChildren2;
					foreach (DistanceJoint2D distanceJoint2D4 in array)
					{
						if (!distanceJoint2D4.connectedBody)
						{
							distanceJoint2D3 = distanceJoint2D4;
						}
					}
					if (!distanceJoint2D3)
					{
						distanceJoint2D3 = pointB.gameObject.AddComponent<DistanceJoint2D>();
					}
					distanceJoint2D3.distance = 0.01f;
					distanceJoint2D3.connectedBody = transform.GetComponent<Rigidbody2D>();
					pointB.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				else if (lockLastChain)
				{
					transform.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				Object.DestroyImmediate(transform.GetComponent<HingeJoint2D>());
				connected = true;
			}
			if ((bool)objectsHolder)
			{
				transform.parent = objectsHolder.transform;
				tempChain = transform;
				chainIndex++;
				yield return new WaitForSeconds(delay);
				continue;
			}
			break;
		}
	}

	public void Remove()
	{
		if ((bool)objHolder)
		{
			HingeJoint2D[] componentsInChildren = objHolder.GetComponentsInChildren<HingeJoint2D>();
			for (int i = 2; i < componentsInChildren.Length - 2; i++)
			{
				componentsInChildren[i].enabled = false;
				ropePool.Remove(componentsInChildren[i].gameObject);
			}
			Transform[] componentsInChildren2 = objHolder.GetComponentsInChildren<Transform>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				ropePool.RemoveFromList(componentsInChildren2[j].gameObject);
				Object.Destroy(componentsInChildren2[j].gameObject);
			}
		}
	}
}
