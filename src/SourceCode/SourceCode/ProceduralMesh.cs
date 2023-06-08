using UnityEngine;

public class ProceduralMesh
{
	private Mesh mMesh;

	public void Initialize(MeshFilter meshFilter)
	{
		mMesh = meshFilter.mesh;
		if (mMesh == null)
		{
			meshFilter.mesh = (mMesh = new Mesh());
			mMesh.name = "Procedural Mesh";
		}
	}

	public void Reset()
	{
		mMesh.Clear();
		mMesh = null;
	}

	protected void Clear()
	{
		mMesh.Clear();
	}

	protected void GenerateMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs = null, Vector4[] tangents = null)
	{
		mMesh.Clear();
		mMesh.vertices = vertices;
		mMesh.triangles = triangles;
		if (uvs != null)
		{
			mMesh.uv = uvs;
		}
		if (tangents != null)
		{
			mMesh.tangents = tangents;
		}
		mMesh.RecalculateNormals();
	}

	protected int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
	{
		triangles[i] = v00;
		triangles[i + 1] = (triangles[i + 4] = v01);
		triangles[i + 2] = (triangles[i + 3] = v10);
		triangles[i + 5] = v11;
		return i + 6;
	}
}
