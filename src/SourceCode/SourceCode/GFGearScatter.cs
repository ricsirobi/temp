using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Gear Factory/Gear Scatter")]
public class GFGearScatter : MonoBehaviour
{
	public GFGearGen gearEntity;

	public bool followNormals = true;

	public bool hideEmitter = true;

	public float scale = 1f;

	private List<Vector3> vectors;

	private List<Vector3> normals;

	private List<GFGearGen> gears;

	public bool combineMeshes;

	public GameObject distributionObject { get; private set; }

	private void Start()
	{
		distributionObject = base.gameObject;
		vectors = new List<Vector3>();
		normals = new List<Vector3>();
		gears = new List<GFGearGen>();
		if (!(distributionObject != null) || !(gearEntity != null))
		{
			return;
		}
		MeshFilter meshFilter = (MeshFilter)distributionObject.GetComponent(typeof(MeshFilter));
		if (!(meshFilter != null))
		{
			return;
		}
		followNormals = followNormals && meshFilter.mesh.normals.Length == meshFilter.mesh.vertices.Length && meshFilter.mesh.normals.Length != 0;
		for (int i = 0; i < meshFilter.mesh.triangles.Length; i += 3)
		{
			int[] array = new int[3]
			{
				meshFilter.mesh.triangles[i],
				meshFilter.mesh.triangles[i + 1],
				meshFilter.mesh.triangles[i + 2]
			};
			float num = Vector3.Distance(meshFilter.mesh.vertices[array[0]], meshFilter.mesh.vertices[array[1]]);
			float num2 = Vector3.Distance(meshFilter.mesh.vertices[array[1]], meshFilter.mesh.vertices[array[2]]);
			float num3 = Vector3.Distance(meshFilter.mesh.vertices[array[2]], meshFilter.mesh.vertices[array[0]]);
			int num4 = 2;
			int num5 = 0;
			if (num > num2 && num > num3)
			{
				num4 = 0;
				num5 = 1;
			}
			else if (num2 > num && num2 > num3)
			{
				num4 = 1;
				num5 = 2;
			}
			Vector3 item = Vector3.zero;
			if (followNormals)
			{
				Vector3 vector = meshFilter.mesh.normals[array[1]];
				Vector3 vector2 = meshFilter.mesh.normals[array[2]];
				Vector3 vector3 = meshFilter.mesh.normals[array[0]];
				item = (vector + vector2 + vector3) / 3f;
				item.Normalize();
			}
			Vector3 item2 = Vector3.Lerp(meshFilter.mesh.vertices[array[num4]], meshFilter.mesh.vertices[array[num5]], 0.5f);
			if (!vectors.Contains(item2))
			{
				vectors.Add(item2);
				if (followNormals)
				{
					normals.Add(item);
				}
			}
		}
		for (int j = 0; j < vectors.Count; j++)
		{
			Vector3 position = vectors[j];
			Quaternion rotation = Quaternion.LookRotation(followNormals ? normals[j] : position.normalized);
			GFGearGen gFGearGen;
			if (j == 0)
			{
				gearEntity.transform.position = position;
				gearEntity.transform.rotation = rotation;
				gearEntity.transform.localRotation.eulerAngles.Set(0f, 0f, 0f);
				gFGearGen = gearEntity;
			}
			else
			{
				gFGearGen = Object.Instantiate(gearEntity);
				gFGearGen.transform.parent = gearEntity.transform.parent;
				gFGearGen.transform.position = position;
				gFGearGen.transform.rotation = rotation;
				if (gFGearGen.gear != null)
				{
					GFGear gear = gears[j - 1].gear;
					if (gear != null)
					{
						gFGearGen.gear.DrivenBy = gear;
					}
				}
			}
			gears.Add(gFGearGen);
		}
		GFGear gear2 = gearEntity.gear;
		if (gear2 != null && gear2.machine != null)
		{
			gear2.machine.RecalculateGears();
		}
		if (combineMeshes)
		{
			List<CombineInstance> list = new List<CombineInstance>();
			for (int k = 0; k < gears.Count; k++)
			{
				MeshFilter component = gears[k].gameObject.GetComponent<MeshFilter>();
				if (component != null)
				{
					CombineInstance item3 = default(CombineInstance);
					item3.mesh = component.sharedMesh;
					item3.transform = component.transform.localToWorldMatrix;
					component.gameObject.SetActive(value: false);
					list.Add(item3);
				}
			}
			MeshFilter component2 = base.transform.GetComponent<MeshFilter>();
			component2.SetMesh(new Mesh());
			component2.sharedMesh.CombineMeshes(list.ToArray());
			if (base.transform.GetComponent<MeshRenderer>() == null)
			{
				_ = (MeshRenderer)base.transform.gameObject.AddComponent(typeof(MeshRenderer));
			}
			base.transform.gameObject.SetActive(value: true);
		}
		meshFilter.GetComponent<Renderer>().enabled = !hideEmitter;
	}

	private void Update()
	{
	}
}
