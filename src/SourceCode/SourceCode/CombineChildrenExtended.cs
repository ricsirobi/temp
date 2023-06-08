using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildrenExtended : KAMonoBase
{
	public int frameToWait;

	public bool generateTriangleStrips = true;

	public bool combineOnStart = true;

	public bool destroyAfterOptimized;

	public bool castShadow = true;

	public bool receiveShadow = true;

	public bool keepLayer = true;

	public bool addMeshCollider;

	private void Start()
	{
		if (combineOnStart && frameToWait == 0)
		{
			Combine();
		}
		else
		{
			StartCoroutine(CombineLate());
		}
	}

	private IEnumerator CombineLate()
	{
		for (int i = 0; i < frameToWait; i++)
		{
			yield return 0;
		}
		Combine();
	}

	[ContextMenu("Combine Now on Childs")]
	public void CallCombineOnAllChilds()
	{
		CombineChildrenExtended[] componentsInChildren = base.gameObject.GetComponentsInChildren<CombineChildrenExtended>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			if (componentsInChildren[i] != this)
			{
				componentsInChildren[i].Combine();
			}
		}
		bool flag2 = (base.enabled = false);
		combineOnStart = flag2;
	}

	[ContextMenu("Combine Now")]
	public void Combine()
	{
		Component[] componentsInChildren = GetComponentsInChildren(typeof(MeshFilter));
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		Hashtable hashtable = new Hashtable();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshFilter meshFilter = (MeshFilter)componentsInChildren[i];
			Renderer component = componentsInChildren[i].GetComponent<Renderer>();
			MeshCombineUtility.MeshInstance meshInstance = default(MeshCombineUtility.MeshInstance);
			meshInstance.mesh = meshFilter.sharedMesh;
			if (!(component != null) || !component.enabled || !(meshInstance.mesh != null))
			{
				continue;
			}
			meshInstance.transform = worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
			Material[] sharedMaterials = component.sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				meshInstance.subMeshIndex = Math.Min(j, meshInstance.mesh.subMeshCount - 1);
				ArrayList arrayList = (ArrayList)hashtable[sharedMaterials[j]];
				if (arrayList != null)
				{
					arrayList.Add(meshInstance);
					continue;
				}
				arrayList = new ArrayList();
				arrayList.Add(meshInstance);
				hashtable.Add(sharedMaterials[j], arrayList);
			}
			if (Application.isPlaying && destroyAfterOptimized && combineOnStart)
			{
				UnityEngine.Object.Destroy(component.gameObject);
			}
			else if (destroyAfterOptimized)
			{
				UnityEngine.Object.DestroyImmediate(component.gameObject);
			}
			else
			{
				component.enabled = false;
			}
		}
		foreach (DictionaryEntry item in hashtable)
		{
			MeshCombineUtility.MeshInstance[] combines = (MeshCombineUtility.MeshInstance[])((ArrayList)item.Value).ToArray(typeof(MeshCombineUtility.MeshInstance));
			if (hashtable.Count == 1)
			{
				if (GetComponent(typeof(MeshFilter)) == null)
				{
					base.gameObject.AddComponent(typeof(MeshFilter));
				}
				if (!GetComponent("MeshRenderer"))
				{
					base.gameObject.AddComponent<MeshRenderer>();
				}
				MeshFilter meshFilter2 = (MeshFilter)GetComponent(typeof(MeshFilter));
				if (Application.isPlaying)
				{
					meshFilter2.mesh = MeshCombineUtility.Combine(combines, generateTriangleStrips);
				}
				else
				{
					meshFilter2.sharedMesh = MeshCombineUtility.Combine(combines, generateTriangleStrips);
				}
				base.renderer.material = (Material)item.Key;
				base.renderer.enabled = true;
				if (addMeshCollider)
				{
					base.gameObject.AddComponent<MeshCollider>();
				}
				base.renderer.shadowCastingMode = (castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off);
				base.renderer.receiveShadows = receiveShadow;
				continue;
			}
			GameObject gameObject = new GameObject("Combined mesh");
			if (keepLayer)
			{
				gameObject.layer = base.gameObject.layer;
			}
			gameObject.transform.parent = base.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.AddComponent(typeof(MeshFilter));
			gameObject.AddComponent<MeshRenderer>();
			Renderer component2 = gameObject.GetComponent<Renderer>();
			component2.material = (Material)item.Key;
			MeshFilter meshFilter3 = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
			if (Application.isPlaying)
			{
				meshFilter3.mesh = MeshCombineUtility.Combine(combines, generateTriangleStrips);
			}
			else
			{
				meshFilter3.sharedMesh = MeshCombineUtility.Combine(combines, generateTriangleStrips);
			}
			component2.shadowCastingMode = (castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off);
			component2.receiveShadows = receiveShadow;
			if (addMeshCollider)
			{
				gameObject.AddComponent<MeshCollider>();
			}
		}
	}

	[ContextMenu("Save mesh as asset")]
	private void SaveMeshAsAsset()
	{
	}
}
