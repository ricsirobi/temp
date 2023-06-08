using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Bar : KAMonoBase
{
	public bool _Vertical;

	public float _Width;

	public float _Heigth;

	public Material _BarBG;

	public Material _BarFill;

	public float _Z;

	public bool _isFill;

	public GameObject _Marker1;

	public GameObject _Marker2;

	private float mCurrentScale;

	private MeshFilter mFilter;

	public float _Min;

	public float _Max;

	public float pScale => mCurrentScale;

	public static Mesh MakeQuad(float originX, float originY, float depth, float sizeX, float sizeY)
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(originX - sizeX / 2f, originY - sizeY / 2f, depth),
			new Vector3(originX + sizeX / 2f, originY - sizeY / 2f, depth),
			new Vector3(originX + sizeX / 2f, originY + sizeY / 2f, depth),
			new Vector3(originX - sizeX / 2f, originY + sizeY / 2f, depth)
		};
		int[] triangles = new int[6] { 2, 1, 0, 0, 3, 2 };
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		return mesh;
	}

	private void Awake()
	{
		mFilter = GetComponent<MeshFilter>();
		Mesh mesh = MakeQuad(0f, 0f, _Z, _Width, _Heigth);
		mFilter.mesh = mesh;
		if (!_isFill)
		{
			base.renderer.material = _BarBG;
		}
		else
		{
			base.renderer.material = _BarFill;
		}
		if (_isFill)
		{
			SetScale(0f);
		}
	}

	public void SetScale(float scale)
	{
		mCurrentScale = scale;
		Vector3[] vertices = mFilter.mesh.vertices;
		if (_Vertical)
		{
			vertices[0].y = vertices[3].y + _Heigth * scale;
			vertices[1].y = vertices[2].y + _Heigth * scale;
		}
		else
		{
			vertices[1].x = vertices[0].x + _Width * scale;
			vertices[2].x = vertices[3].x + _Width * scale;
		}
		mFilter.mesh.vertices = vertices;
	}

	public void SetMarker1Pos(float _precentage)
	{
		if (null != _Marker1)
		{
			Vector3 vector = -(Vector3.right * _Width * 0.5f) + Vector3.right * _Width * _precentage;
			vector.y += _Heigth * 0.5f;
			GameObject obj = Object.Instantiate(_Marker1, vector, base.transform.rotation);
			obj.transform.parent = base.transform;
			obj.transform.localPosition = vector;
		}
	}

	public void SetMarker2Pos(float _precentage)
	{
		if (null != _Marker2)
		{
			Vector3 vector = -(Vector3.right * _Width * 0.5f) + Vector3.right * _Width * _precentage;
			vector.y -= _Heigth * 0.5f;
			GameObject obj = Object.Instantiate(_Marker2, vector, base.transform.rotation);
			obj.transform.parent = base.transform;
			obj.transform.localPosition = vector;
		}
	}
}
