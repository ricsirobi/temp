using UnityEngine;

public class TargetHit3DScore : KAMonoBase
{
	public float _LifeTime;

	public GameObject _TextObject;

	public Color _NegativeColorText;

	public Color _PositiveColorText;

	public LocaleString _CriticalText = new LocaleString("CRITICAL HIT");

	public LocaleString _StunnedText = new LocaleString("STUNNED");

	public LocaleString _SlowedText = new LocaleString("SLOWED");

	[HideInInspector]
	public int mDisplayScore;

	[HideInInspector]
	public string mDisplayText = "";

	private float mTimer;

	private Vector3 mStartPos = Vector3.zero;

	private Vector3 mEndPos = Vector3.zero;

	private TextMesh mScoreMesh;

	private TextMesh mTextMesh;

	public void Start()
	{
		if (_LifeTime <= 0f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		mStartPos = base.transform.position;
		mEndPos = base.transform.position + Vector3.up;
		mScoreMesh = GetComponent<TextMesh>();
		if (_TextObject != null)
		{
			mTextMesh = _TextObject.GetComponent<TextMesh>();
			if (mTextMesh != null)
			{
				mTextMesh.text = mDisplayText;
			}
		}
		if (mScoreMesh != null)
		{
			if (mDisplayScore != 0)
			{
				mScoreMesh.text = ((mDisplayScore >= 0) ? "+" : "") + mDisplayScore;
			}
			else
			{
				mScoreMesh.text = "";
			}
			if (mDisplayScore < 0)
			{
				mTextMesh.color = _NegativeColorText;
				mScoreMesh.color = _NegativeColorText;
			}
			else
			{
				mTextMesh.color = _PositiveColorText;
				mScoreMesh.color = _PositiveColorText;
			}
		}
	}

	public void Update()
	{
		if (mScoreMesh != null && mTimer < _LifeTime)
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

	public static void Show3DHitScore(GameObject hitScorePrefab, Vector3 inPosition, int inScore, bool isCritical = false, EffectType effectType = EffectType.INVALID)
	{
		if (!(hitScorePrefab != null))
		{
			return;
		}
		TargetHit3DScore component = Object.Instantiate(hitScorePrefab).GetComponent<TargetHit3DScore>();
		if (!(component != null))
		{
			return;
		}
		component.transform.position = inPosition;
		component.mDisplayScore = inScore;
		if (isCritical)
		{
			component.mDisplayText = component._CriticalText.GetLocalizedString();
			return;
		}
		switch (effectType)
		{
		case EffectType.SLOW:
			component.mDisplayText = component._SlowedText.GetLocalizedString();
			break;
		case EffectType.STUN:
			component.mDisplayText = component._StunnedText.GetLocalizedString();
			break;
		}
	}
}
