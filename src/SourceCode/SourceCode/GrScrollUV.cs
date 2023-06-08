using UnityEngine;

public class GrScrollUV : KAMonoBase
{
	public Material _Material;

	public float _USpeed;

	public float _VSpeed;

	private Material mMaterial;

	public Material pMaterial => mMaterial;

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
	}

	private void Update()
	{
		if (mMaterial != null)
		{
			mMaterial.mainTextureOffset += new Vector2(Time.deltaTime * _USpeed, Time.deltaTime * _VSpeed);
			mMaterial.mainTextureOffset = new Vector2(Frac(mMaterial.mainTextureOffset.x), Frac(mMaterial.mainTextureOffset.y));
		}
	}

	private float Frac(float x)
	{
		if (x > 1f)
		{
			return x - (float)Mathf.FloorToInt(x);
		}
		if (x < -1f)
		{
			return x - (float)Mathf.CeilToInt(x);
		}
		return x;
	}
}
