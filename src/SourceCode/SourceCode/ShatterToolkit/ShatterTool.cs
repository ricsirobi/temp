using System.Collections.Generic;
using UnityEngine;

namespace ShatterToolkit;

[RequireComponent(typeof(MeshFilter))]
public class ShatterTool : MonoBehaviour
{
	[SerializeField]
	protected int generation = 1;

	[SerializeField]
	protected int generationLimit = 3;

	[SerializeField]
	protected int cuts = 2;

	[SerializeField]
	protected bool fillCut = true;

	[SerializeField]
	protected bool sendPreSplitMessage;

	[SerializeField]
	protected bool sendPostSplitMessage;

	[SerializeField]
	protected HullType internalHullType;

	protected bool isIntact = true;

	protected IHull hull;

	protected Vector3 center;

	public int Generation
	{
		get
		{
			return generation;
		}
		set
		{
			generation = Mathf.Max(value, 1);
		}
	}

	public int GenerationLimit
	{
		get
		{
			return generationLimit;
		}
		set
		{
			generationLimit = Mathf.Max(value, 1);
		}
	}

	public int Cuts
	{
		get
		{
			return cuts;
		}
		set
		{
			cuts = Mathf.Max(value, 1);
		}
	}

	public bool FillCut
	{
		get
		{
			return fillCut;
		}
		set
		{
			fillCut = value;
		}
	}

	public bool SendPreSplitMessage
	{
		get
		{
			return sendPreSplitMessage;
		}
		set
		{
			sendPreSplitMessage = value;
		}
	}

	public bool SendPostSplitMessage
	{
		get
		{
			return sendPostSplitMessage;
		}
		set
		{
			sendPostSplitMessage = value;
		}
	}

	public HullType InternalHullType
	{
		get
		{
			return internalHullType;
		}
		set
		{
			internalHullType = value;
		}
	}

	public bool IsFirstGeneration => generation == 1;

	public bool IsLastGeneration => generation >= generationLimit;

	public Vector3 Center => base.transform.TransformPoint(center);

	protected void CalculateCenter()
	{
		center = GetComponent<MeshFilter>().sharedMesh.bounds.center;
	}

	public void Start()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		if (hull == null)
		{
			if (internalHullType == HullType.FastHull)
			{
				hull = new FastHull(sharedMesh);
			}
			else if (internalHullType == HullType.LegacyHull)
			{
				hull = new LegacyHull(sharedMesh);
			}
		}
		CalculateCenter();
	}

	public void Shatter(Vector3 point)
	{
		if (!IsLastGeneration)
		{
			generation++;
			Plane[] array = new Plane[cuts];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Plane(Random.onUnitSphere, point);
			}
			Split(array);
		}
	}

	public void Split(Plane[] planes)
	{
		if (planes != null && planes.Length != 0 && isIntact && hull != null && !hull.IsEmpty)
		{
			UvMapper component = GetComponent<UvMapper>();
			ColorMapper component2 = GetComponent<ColorMapper>();
			if (sendPreSplitMessage)
			{
				SendMessage("PreSplit", planes, SendMessageOptions.DontRequireReceiver);
			}
			ConvertPlanesToLocalspace(planes, out var points, out var normals);
			CreateNewHulls(component, component2, points, normals, out var newHulls);
			CreateNewGameObjects(newHulls, out var newGameObjects);
			if (sendPostSplitMessage)
			{
				SendMessage("PostSplit", newGameObjects, SendMessageOptions.DontRequireReceiver);
			}
			Object.Destroy(base.gameObject);
			isIntact = false;
		}
	}

	protected void ConvertPlanesToLocalspace(Plane[] planes, out Vector3[] points, out Vector3[] normals)
	{
		points = new Vector3[planes.Length];
		normals = new Vector3[planes.Length];
		for (int i = 0; i < planes.Length; i++)
		{
			Plane plane = planes[i];
			Vector3 vector = base.transform.InverseTransformPoint(plane.normal * (0f - plane.distance));
			Vector3 vector2 = base.transform.InverseTransformDirection(plane.normal);
			vector2.Scale(base.transform.localScale);
			vector2.Normalize();
			points[i] = vector;
			normals[i] = vector2;
		}
	}

	protected void CreateNewHulls(UvMapper uvMapper, ColorMapper colorMapper, Vector3[] points, Vector3[] normals, out IList<IHull> newHulls)
	{
		newHulls = new List<IHull>();
		newHulls.Add(this.hull);
		for (int i = 0; i < points.Length; i++)
		{
			int count = newHulls.Count;
			for (int j = 0; j < count; j++)
			{
				IHull hull = newHulls[0];
				hull.Split(points[i], normals[i], fillCut, uvMapper, colorMapper, out var resultA, out var resultB);
				newHulls.Remove(hull);
				if (!resultA.IsEmpty)
				{
					newHulls.Add(resultA);
				}
				if (!resultB.IsEmpty)
				{
					newHulls.Add(resultB);
				}
			}
		}
	}

	protected void CreateNewGameObjects(IList<IHull> newHulls, out GameObject[] newGameObjects)
	{
		Mesh[] array = new Mesh[newHulls.Count];
		float[] array2 = new float[newHulls.Count];
		float num = 0f;
		for (int i = 0; i < newHulls.Count; i++)
		{
			Mesh mesh = newHulls[i].GetMesh();
			Vector3 size = mesh.bounds.size;
			float num2 = size.x * size.y * size.z;
			array[i] = mesh;
			array2[i] = num2;
			num += num2;
		}
		MeshFilter component = GetComponent<MeshFilter>();
		MeshCollider component2 = GetComponent<MeshCollider>();
		Rigidbody component3 = GetComponent<Rigidbody>();
		component.sharedMesh = null;
		if (component2 != null)
		{
			component2.sharedMesh = null;
		}
		newGameObjects = new GameObject[newHulls.Count];
		for (int j = 0; j < newHulls.Count; j++)
		{
			IHull hull = newHulls[j];
			Mesh sharedMesh = array[j];
			float num3 = array2[j];
			GameObject gameObject = Object.Instantiate(base.gameObject);
			ShatterTool component4 = gameObject.GetComponent<ShatterTool>();
			if (component4 != null)
			{
				component4.hull = hull;
			}
			MeshFilter component5 = gameObject.GetComponent<MeshFilter>();
			if (component5 != null)
			{
				component5.sharedMesh = sharedMesh;
			}
			MeshCollider component6 = gameObject.GetComponent<MeshCollider>();
			if (component6 != null)
			{
				component6.sharedMesh = sharedMesh;
			}
			Rigidbody component7 = gameObject.GetComponent<Rigidbody>();
			if (component3 != null && component7 != null)
			{
				component7.mass = component3.mass * (num3 / num);
				if (!component7.isKinematic)
				{
					component7.velocity = component3.GetPointVelocity(component7.worldCenterOfMass);
					component7.angularVelocity = component3.angularVelocity;
				}
			}
			component4.CalculateCenter();
			newGameObjects[j] = gameObject;
		}
	}
}
