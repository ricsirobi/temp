using UnityEngine;

public class GauntletHit3DScore : KAMonoBase
{
	public Material _NegativeScoreMaterial;

	public float _LifeTime;

	public int _DisplayScore;

	private float mTimer;

	private Vector3 mStartPos = Vector3.zero;

	private Vector3 mEndPos = Vector3.zero;

	private TextMesh mtextMesh;

	public void Start()
	{
		if (_LifeTime <= 0f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		mStartPos = base.transform.position;
		mEndPos = base.transform.position + Vector3.up;
		mtextMesh = GetComponent<TextMesh>();
		if (mtextMesh != null)
		{
			mtextMesh.text = ((_DisplayScore > 0) ? "+" : "") + _DisplayScore;
			if (_DisplayScore < 0 && base.renderer != null)
			{
				base.renderer.material = _NegativeScoreMaterial;
			}
		}
	}

	public void Update()
	{
		if (mtextMesh != null && mTimer < _LifeTime)
		{
			mTimer += Time.deltaTime;
			if (mTimer >= _LifeTime)
			{
				Object.Destroy(base.gameObject);
			}
			base.transform.position = Vector3.Lerp(mStartPos, mEndPos, mTimer / _LifeTime);
			if (Camera.main != null)
			{
				base.transform.forward = Camera.main.transform.forward;
			}
		}
	}
}
