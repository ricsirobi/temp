using UnityEngine;

public class ObFading : KAMonoBase
{
	public float _TimetoChange = 5f;

	public float _AlphaStart;

	public float _AlphaEnd;

	public int _MaterialIndex;

	public string _ColorPropertyName = "_Color";

	private Color mObjectColor = Color.clear;

	private float mElapsedTime;

	private float mTimeScale;

	private void Start()
	{
		if (base.renderer != null)
		{
			mObjectColor = base.renderer.materials[_MaterialIndex].GetColor(_ColorPropertyName);
		}
	}

	public void Update()
	{
		if (!(base.renderer == null) && !(mTimeScale >= 1f) && _AlphaStart != _AlphaEnd && !(_TimetoChange <= 0f))
		{
			mElapsedTime += Time.deltaTime;
			mTimeScale = mElapsedTime / _TimetoChange;
			mObjectColor.a = _AlphaStart + mTimeScale * (_AlphaEnd - _AlphaStart);
			base.renderer.materials[_MaterialIndex].SetColor(_ColorPropertyName, mObjectColor);
		}
	}
}
