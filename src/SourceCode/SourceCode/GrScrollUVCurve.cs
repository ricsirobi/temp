using UnityEngine;

[RequireComponent(typeof(Animation))]
public class GrScrollUVCurve : KAMonoBase
{
	public Material _Material;

	public WrapMode _WrapMode;

	public bool _UseU;

	public bool _UseV;

	public AnimationCurve _CurveU = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve _CurveV = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private int mMaterialIndex;

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
				mMaterialIndex = i;
				break;
			}
		}
		AnimationClip animationClip = new AnimationClip();
		string text2 = "[" + mMaterialIndex + "].";
		if (_UseU)
		{
			animationClip.SetCurve("", typeof(Material), text2 + "_MainTex.offset.x", _CurveU);
		}
		if (_UseV)
		{
			animationClip.SetCurve("", typeof(Material), text2 + "_MainTex.offset.y", _CurveV);
		}
		animationClip.name = "UVScroll" + mMaterialIndex;
		base.animation.AddClip(animationClip, animationClip.name);
		base.animation[animationClip.name].layer = mMaterialIndex;
		base.animation[animationClip.name].wrapMode = _WrapMode;
		base.animation.Play(animationClip.name);
	}
}
