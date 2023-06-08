using System.Collections.Generic;
using UnityEngine;

public class ObTriggerMaterial : ObTrigger
{
	public MeshRenderer[] _MeshRenderers;

	public string _MaterialPath = "JS Games/Transparent/Unlit Color";

	public Color _MaterialColor = new Color(1f, 1f, 1f, 0.1f);

	private Material mMaterialOverride;

	private Dictionary<MeshRenderer, List<Material>> mCachedMaterials;

	private void Start()
	{
		mMaterialOverride = new Material(Shader.Find(_MaterialPath));
		mMaterialOverride.color = _MaterialColor;
	}

	public override void DoTriggerAction(GameObject other)
	{
		SetMaterials(overrideMaterial: true);
		base.DoTriggerAction(other);
	}

	public override void DoTriggerExit()
	{
		SetMaterials(overrideMaterial: false);
		base.DoTriggerExit();
	}

	public void SetMaterials(bool overrideMaterial)
	{
		if (_MeshRenderers.Length == 0)
		{
			_MeshRenderers = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		}
		if (overrideMaterial && mCachedMaterials == null)
		{
			mCachedMaterials = new Dictionary<MeshRenderer, List<Material>>();
			for (int i = 0; i <= _MeshRenderers.Length - 1; i++)
			{
				List<Material> list = new List<Material>();
				Material[] materials = _MeshRenderers[i].materials;
				foreach (Material item in materials)
				{
					list.Add(item);
				}
				mCachedMaterials.Add(_MeshRenderers[i], list);
			}
		}
		MeshRenderer[] meshRenderers = _MeshRenderers;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			if (overrideMaterial)
			{
				List<Material> list2 = new List<Material>();
				for (int k = 0; k <= meshRenderer.materials.Length - 1; k++)
				{
					list2.Add(mMaterialOverride);
				}
				meshRenderer.materials = list2.ToArray();
			}
			else if (mCachedMaterials.ContainsKey(meshRenderer))
			{
				meshRenderer.materials = mCachedMaterials[meshRenderer].ToArray();
			}
		}
	}
}
