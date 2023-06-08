using System;
using UnityEngine;

public class GrScrollMultiUV : KAMonoBase
{
	[Serializable]
	public class TextureEntry
	{
		public string _Texture;

		public Vector2 _Speed;
	}

	public Material _Material;

	public TextureEntry[] _Settings;

	private Material mMaterial;

	private void Start()
	{
		if (_Material == null)
		{
			return;
		}
		string text = _Material.name + " (Instance)";
		for (int i = 0; i < base.renderer.materials.Length; i++)
		{
			if (base.renderer.materials[i].name == text)
			{
				mMaterial = base.renderer.materials[i];
				break;
			}
		}
		TextureEntry[] settings = _Settings;
		foreach (TextureEntry textureEntry in settings)
		{
			if ((bool)mMaterial && !mMaterial.HasProperty(textureEntry._Texture))
			{
				UtDebug.LogWarning("Property " + textureEntry._Texture + " not found on object");
			}
		}
	}

	private void Update()
	{
		if (mMaterial == null || _Settings == null || _Settings.Length == 0)
		{
			return;
		}
		TextureEntry[] settings = _Settings;
		foreach (TextureEntry textureEntry in settings)
		{
			if (mMaterial.HasProperty(textureEntry._Texture))
			{
				Vector2 textureOffset = mMaterial.GetTextureOffset(textureEntry._Texture);
				textureOffset.x += Time.deltaTime * textureEntry._Speed.x;
				textureOffset.y += Time.deltaTime * textureEntry._Speed.y;
				mMaterial.SetTextureOffset(textureEntry._Texture, textureOffset);
			}
		}
	}
}
