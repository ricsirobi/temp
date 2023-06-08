using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Gear Factory/Gear")]
public class GFGear : MonoBehaviour
{
	public List<GFGear> children = new List<GFGear>();

	private GFGear[] _childrenAsArray;

	public GFGear DrivenBy;

	public bool AutoSetDrivenBy = true;

	public bool ReverseRotation;

	public bool ReverseRotationPlusSubtree;

	public bool SyncSpeed;

	[HideInInspector]
	public int numberOfTeeth = 8;

	[HideInInspector]
	public bool rotateX;

	[HideInInspector]
	public bool rotateY;

	[HideInInspector]
	public bool rotateZ = true;

	private Quaternion rotationQuaternion;

	private Vector3 rotationAngles;

	private Vector3 rotationVector;

	private Quaternion tempRotation;

	private float totalAngle;

	private float finalAngle;

	public GFMachine machine
	{
		get
		{
			if (base.gameObject.transform.parent != null)
			{
				GFMachine component = base.gameObject.transform.parent.gameObject.GetComponent<GFMachine>();
				if (component != null)
				{
					return component;
				}
			}
			return null;
		}
	}

	private GFGear[] rootGearObjects
	{
		get
		{
			List<GFGear> list = new List<GFGear>();
			Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
			foreach (Transform transform in array)
			{
				if (transform.parent == null)
				{
					GFGear component = transform.gameObject.GetComponent<GFGear>();
					if (component != null && !component.gameObject.Equals(base.gameObject))
					{
						list.Add(component);
					}
				}
			}
			return list.ToArray();
		}
	}

	public GFGear[] otherGears
	{
		get
		{
			if (machine != null)
			{
				return machine.GetGears(this);
			}
			return rootGearObjects;
		}
	}

	public Mesh mesh => base.gameObject.GetSharedMesh();

	public float radius
	{
		get
		{
			if (mesh != null)
			{
				Bounds bounds = mesh.bounds;
				return Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)) / 2f;
			}
			return -1f;
		}
	}

	public bool CCW { get; set; }

	public float speedMultiplier { get; set; }

	public GFGear[] childrenAsArray
	{
		get
		{
			if (_childrenAsArray == null)
			{
				_childrenAsArray = children.ToArray();
			}
			return _childrenAsArray;
		}
	}

	public float toothSize => 0.25f;

	public GFGearGen gearGen => base.gameObject.GetComponent<GFGearGen>();

	public float angle { get; private set; }

	private void Start()
	{
		rotationQuaternion = Quaternion.identity;
		rotationAngles = Vector3.zero;
	}

	public void Reset()
	{
		children.Clear();
		_childrenAsArray = null;
	}

	public void SetRotation(float angle)
	{
		this.angle = angle;
		finalAngle = (CCW ? (-1f) : 1f) * angle;
		if (rotateX)
		{
			rotationAngles.x = finalAngle;
		}
		if (rotateY)
		{
			rotationAngles.y = finalAngle;
		}
		if (rotateZ)
		{
			rotationAngles.z = finalAngle;
		}
		rotationQuaternion.eulerAngles = rotationAngles;
		base.transform.localRotation = base.transform.localRotation * rotationQuaternion;
	}

	public bool Intersects(GFGear otherGear)
	{
		MeshUtils.IntersectType intersectType = MeshUtils.Intersect(base.gameObject.CalcOrientedBoundingBox(), otherGear.gameObject.CalcOrientedBoundingBox());
		if (intersectType != MeshUtils.IntersectType.Intersect)
		{
			return intersectType == MeshUtils.IntersectType.Inside;
		}
		return true;
	}

	public GameObject Clone(Vector3 direction)
	{
		GFGearGen gFGearGen = (GFGearGen)base.gameObject.GetComponent(typeof(GFGearGen));
		float num = ((!(gFGearGen != null)) ? (radius * 2.1f - toothSize) : (gFGearGen.radius + (gFGearGen.radius - gFGearGen.tipLength)));
		GameObject obj = UnityEngine.Object.Instantiate(gameObject);
		obj.transform.parent = gameObject.transform.parent;
		GFGear obj2 = obj.GetComponent(typeof(GFGear)) as GFGear;
		obj2.DrivenBy = this;
		obj2.AutoSetDrivenBy = false;
		GFGearGen component = obj2.GetComponent<GFGearGen>();
		if (component != null)
		{
			component.alignTeethWithParent = true;
			component.alignRadiusWithParent = true;
		}
		obj.transform.position = transform.position + direction.normalized * num;
		string pattern = "(\\d+)(?!.*\\d)";
		Match match = Regex.Match(gameObject.name, pattern);
		int num2 = 1;
		if (match != null && match.Value != null && DrivenBy != null)
		{
			num2 = int.Parse(match.Value) + 1;
		}
		obj.name = Regex.Replace(gameObject.name, pattern, "").Trim() + " " + num2;
		component.Align(gFGearGen);
		return obj;
	}
}
