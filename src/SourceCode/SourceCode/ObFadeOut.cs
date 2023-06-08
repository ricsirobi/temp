using System.Collections.Generic;
using UnityEngine;

public class ObFadeOut : MonoBehaviour
{
	private struct MatEntry
	{
		public Material mat;

		public Shader origShader;
	}

	private float mAlpha = 1f;

	private bool mComplete;

	private List<MatEntry> mMaterials;

	public float _FadeTime = 1f;

	private void Start()
	{
		mMaterials = new List<MatEntry>();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		MatEntry item = default(MatEntry);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshRenderer component = componentsInChildren[i].GetComponent<MeshRenderer>();
			if (!(component != null) || component.materials == null)
			{
				continue;
			}
			Material[] materials = component.materials;
			foreach (Material material in materials)
			{
				if (!(material.shader != null))
				{
					continue;
				}
				item.mat = material;
				item.origShader = material.shader;
				mMaterials.Add(item);
				if (!item.mat.shader.name.Contains("Transparent"))
				{
					if (item.mat.shader.name.Contains("Vertex"))
					{
						item.mat.shader = Shader.Find("Transparent/VertexLit");
					}
					else if (item.mat.shader.name.Contains("Bumped"))
					{
						item.mat.shader = Shader.Find("Transparent/Bumped Specular");
					}
					else
					{
						item.mat.shader = Shader.Find("Transparent/Diffuse");
					}
				}
				if (item.mat.HasProperty("_Color"))
				{
					Color color = item.mat.color;
					color.a = mAlpha;
					item.mat.color = color;
				}
			}
		}
	}

	private void Update()
	{
		if (mComplete)
		{
			return;
		}
		if (_FadeTime > Mathf.Epsilon)
		{
			mAlpha -= Time.deltaTime / _FadeTime;
		}
		else
		{
			mAlpha = 0f;
		}
		if (mAlpha <= 0f)
		{
			mComplete = true;
			mAlpha = 0f;
		}
		foreach (MatEntry mMaterial in mMaterials)
		{
			if (mMaterial.mat.HasProperty("_Color"))
			{
				Color color = mMaterial.mat.color;
				color.a = mAlpha;
				mMaterial.mat.color = color;
			}
			if (mComplete)
			{
				mMaterial.mat.shader = mMaterial.origShader;
			}
		}
		if (mComplete)
		{
			mMaterials.Clear();
			base.gameObject.SetActive(value: false);
		}
	}
}
