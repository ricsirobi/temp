using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GFGearGen : MonoBehaviour
{
	public readonly GFGearGenProc ggp = new GFGearGenProc();

	public GFGearGenTextureCoordinates generateTextureCoordinates = GFGearGenTextureCoordinates.Box;

	public Vector3 uvTiling = Vector3.one;

	public Vector3 uvOffset = Vector3.zero;

	public int numberOfTeeth = 9;

	public float radius = 1f;

	public float tipLength = 0.3f;

	public float tipSize;

	public float tipAngleOffset;

	public float valleySize;

	public float valleyAngleOffset;

	public float skew;

	public float twistAngle;

	public bool twistOutside;

	public float thickness = 0.1f;

	public bool showNormals;

	public float splitVerticesAngle = 30f;

	public bool is3d = true;

	public bool fillCenter = true;

	public bool fillOutside = true;

	public float innerRadius;

	public bool alignTeethWithParent = true;

	public bool alignRadiusWithParent = true;

	public bool innerTeeth;

	public int numberOfVertices
	{
		get
		{
			if (base.gameObject.GetComponent<MeshFilter>().sharedMesh != null)
			{
				return base.gameObject.GetComponent<MeshFilter>().sharedMesh.vertexCount;
			}
			return 0;
		}
	}

	public int numberOfFaces
	{
		get
		{
			if (base.gameObject.GetComponent<MeshFilter>().sharedMesh != null)
			{
				return base.gameObject.GetComponent<MeshFilter>().sharedMesh.triangles.Length;
			}
			return 0;
		}
	}

	public bool isShifted { get; set; }

	public GFGear gear => base.gameObject.GetComponent<GFGear>();

	public void Rebuild()
	{
		ggp.GenerateGear(this);
	}

	private void Start()
	{
		isShifted = false;
	}

	private void Awake()
	{
		MeshRenderer obj = base.gameObject.GetComponent("MeshRenderer") as MeshRenderer;
		MeshFilter meshFilter = base.gameObject.GetComponent("MeshFilter") as MeshFilter;
		if (!(obj != null) || !(meshFilter != null))
		{
			return;
		}
		if (meshFilter.sharedMesh != null)
		{
			GFGearGen[] array = Object.FindObjectsOfType(typeof(GFGearGen)) as GFGearGen[];
			for (int i = 0; i < array.Length; i++)
			{
				MeshFilter meshFilter2 = array[i].GetComponent(typeof(MeshFilter)) as MeshFilter;
				if (meshFilter2 != null && meshFilter2.sharedMesh == meshFilter.sharedMesh && meshFilter2 != meshFilter)
				{
					meshFilter.mesh = new Mesh();
					Rebuild();
				}
			}
		}
		if (meshFilter.sharedMesh == null)
		{
			meshFilter.sharedMesh = new Mesh();
			Rebuild();
		}
	}

	public void Align(GFGearGen alignTo, bool snapPositions = false)
	{
		int num = (int)Mathf.Round((float)alignTo.numberOfTeeth / alignTo.radius * radius);
		if (numberOfTeeth != num)
		{
			numberOfTeeth = num;
			Rebuild();
		}
		AlignPositions(alignTo, snapPositions);
	}

	public void AlignRadius(GFGearGen alignTo, bool snapPositions = false)
	{
		float num = alignTo.radius / (float)alignTo.numberOfTeeth * (float)numberOfTeeth;
		if (radius != num)
		{
			radius = num;
			Rebuild();
		}
		AlignPositions(alignTo, snapPositions);
	}

	public void AlignPositions(GFGearGen alignTo, bool snapPositions = false)
	{
		bool flag = (alignTo.innerTeeth ? (!alignTo.isShifted) : alignTo.isShifted);
		float num = 180f / (float)numberOfTeeth;
		float num2 = 1f + (float)alignTo.numberOfTeeth / (float)numberOfTeeth;
		if ((float)numberOfTeeth % 2f == 0f)
		{
			isShifted = !flag;
		}
		else if ((float)alignTo.numberOfTeeth % 2f == 0f && flag)
		{
			isShifted = !flag;
			num = 0f;
		}
		else
		{
			isShifted = flag;
		}
		Vector3 normalized = (base.gameObject.transform.position - alignTo.gameObject.transform.position).normalized;
		Vector3 vector = alignTo.transform.worldToLocalMatrix * normalized;
		float num3 = 57.29578f * Mathf.Atan2(vector.y, vector.x);
		Vector3 eulerAngles = base.gameObject.transform.localRotation.eulerAngles;
		try
		{
			if (alignTo.innerTeeth)
			{
				num2 = 1f - (float)alignTo.numberOfTeeth / (float)numberOfTeeth;
				base.gameObject.transform.localRotation = alignTo.gameObject.transform.localRotation * Quaternion.Euler(eulerAngles.x, eulerAngles.y, num3 * num2 - (isShifted ? num : (flag ? 0f : (0f - num))));
			}
			else
			{
				base.gameObject.transform.localRotation = alignTo.gameObject.transform.localRotation * Quaternion.Euler(eulerAngles.x, eulerAngles.y, num3 * num2 - (isShifted ? num : (flag ? (0f - num) : 0f)));
			}
		}
		catch
		{
		}
		if (snapPositions || alignTo.innerTeeth || innerTeeth)
		{
			if (alignTo.innerTeeth)
			{
				base.gameObject.transform.position = alignTo.transform.position + normalized * (alignTo.radius - (isShifted ? alignTo.tipLength : 0f) - (radius + (isShifted ? tipLength : 0f)));
			}
			else
			{
				base.gameObject.transform.position = alignTo.transform.position + normalized * (alignTo.radius - (isShifted ? 0f : alignTo.tipLength) + (radius - (isShifted ? tipLength : 0f)));
			}
		}
	}

	public void Randomize()
	{
		if (!alignTeethWithParent)
		{
			numberOfTeeth = Random.Range(2, 20);
		}
		radius = Random.Range(0.1f, 5f);
		tipLength = Random.Range(0.1f, 1f);
		tipSize = Random.Range(0f, 1f);
		valleySize = Random.Range(0f, 1f);
		skew = Random.Range(0f, 1f);
		if (is3d)
		{
			thickness = Random.Range(0.1f, 2f);
		}
		if (Random.Range(0f, 1f) > 0.5f)
		{
			innerRadius = Random.Range(0f, radius - tipLength);
		}
		else
		{
			innerRadius = 0f;
		}
		Rebuild();
	}
}
