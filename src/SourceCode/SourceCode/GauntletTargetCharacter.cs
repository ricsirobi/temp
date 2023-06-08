using UnityEngine;

public class GauntletTargetCharacter : GauntletTarget
{
	public int _WrongHitScore;

	public string _WrongHitAnim;

	public Texture[] _EnemyTexture;

	public Texture[] _FriendTexture;

	public float _RotateSpeed;

	public float _ResetTime;

	public int _ResetCount;

	public AudioClip _ShowSFX;

	public AudioClip _HideSFX;

	public AudioClip _NegativeSFX;

	public int _CorrectMathAnswerScore = 500;

	public int _InCorrectMathAnswerScore = -100;

	private Quaternion mInitialRotation = Quaternion.identity;

	private Quaternion mDesiredRotation = Quaternion.identity;

	private bool mIsFriend;

	private bool mIsRotatingBackward;

	private bool mIsRotatingForward;

	private float mResetTimer;

	private bool mPlayShowSFX;

	private CmAnswerItem mAnswer;

	protected override void Start()
	{
		base.Start();
		if (string.IsNullOrEmpty(_SoundPool))
		{
			_SoundPool = "GSTargetChar_Pool";
		}
		mInitialRotation = base.transform.rotation;
		Vector3 eulerAngles = mInitialRotation.eulerAngles;
		eulerAngles.y = (eulerAngles.y + 180f) % 360f;
		mDesiredRotation = Quaternion.Euler(eulerAngles);
		ResetTarget();
	}

	protected override void Update()
	{
		if (!mIsActive)
		{
			return;
		}
		if (mIsRotatingBackward)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, mInitialRotation, _RotateSpeed * Time.deltaTime);
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			Vector3 eulerAngles2 = mInitialRotation.eulerAngles;
			if ((eulerAngles - eulerAngles2).magnitude <= 0.1f)
			{
				mIsRotatingBackward = false;
				base.transform.rotation = mInitialRotation;
				if (_ResetCount > 1)
				{
					_ResetCount--;
					ResetTarget();
				}
				else
				{
					mIsActive = false;
				}
			}
		}
		else if (mIsRotatingForward)
		{
			if (mPlayShowSFX)
			{
				mPlayShowSFX = false;
				PlayAudio(_ShowSFX);
			}
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, mDesiredRotation, _RotateSpeed * Time.deltaTime);
			if (base.transform.rotation == mDesiredRotation)
			{
				mIsRotatingForward = false;
				mResetTimer = _ResetTime;
			}
		}
		else if (mResetTimer > 0f)
		{
			mResetTimer -= Time.deltaTime;
			if (mResetTimer <= 0f)
			{
				mIsRotatingBackward = true;
				PlayAudio(_HideSFX);
			}
		}
	}

	private void ResetTarget()
	{
		base.transform.rotation = mInitialRotation;
		mIsRotatingForward = true;
		mPlayShowSFX = true;
		if ((bool)GauntletRailShootManager.pInstance && !GauntletRailShootManager.pInstance.pIsMathContent)
		{
			Texture tex = null;
			mIsFriend = ((Random.Range(0, 10) % 5 == 0) ? true : false);
			if (mIsFriend && _FriendTexture != null)
			{
				tex = _FriendTexture[Random.Range(0, _FriendTexture.Length)];
			}
			else if (_EnemyTexture != null)
			{
				tex = _EnemyTexture[Random.Range(0, _EnemyTexture.Length)];
			}
			UpdateTexture(tex);
		}
	}

	protected override void OnTriggerEnter(Collider collider)
	{
		RailGunProjectile component = collider.GetComponent<RailGunProjectile>();
		if (!(component != null))
		{
			return;
		}
		if (!mIsRotatingBackward && mIsActive)
		{
			bool flag = (bool)GauntletRailShootManager.pInstance && GauntletRailShootManager.pInstance.pIsMathContent;
			if ((!flag) ? (!mIsFriend) : mAnswer.pbCorrect)
			{
				if (!flag)
				{
					component.HandleTargetHit(base.gameObject, _HitScore, _HitAnim);
				}
				else
				{
					component.HandleTargetHit(base.gameObject, _CorrectMathAnswerScore, _HitAnim);
					GauntletRailShootManager.pInstance.UpdateMathScore(mAnswer, _CorrectMathAnswerScore);
				}
				PlayAudio(_HitSFX);
			}
			else
			{
				if (!flag)
				{
					component.HandleTargetHit(base.gameObject, _WrongHitScore, _WrongHitAnim);
				}
				else
				{
					component.HandleTargetHit(base.gameObject, _InCorrectMathAnswerScore, _WrongHitAnim);
					GauntletRailShootManager.pInstance.UpdateMathScore(mAnswer, _InCorrectMathAnswerScore);
				}
				PlayAudio(_NegativeSFX);
			}
			mIsRotatingBackward = true;
		}
		else
		{
			component.PlayNegativeSFX();
		}
		component.DestroyMe();
	}

	public void UpdateTexture(Texture tex)
	{
		if (mRenderer != null && tex != null)
		{
			mRenderer.materials[1].mainTexture = tex;
		}
	}

	public void SetMathTargetByText(CmAnswerItem answer, GameObject prefab)
	{
		mAnswer = answer;
		BoxCollider component = GetComponent<BoxCollider>();
		if (!component)
		{
			return;
		}
		Vector3 position = base.transform.TransformPoint(component.center) + base.transform.TransformDirection(Vector3.down * 0.2f + Vector3.back * 0.2f);
		GameObject gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
		if (!gameObject)
		{
			return;
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.localRotation = prefab.transform.rotation;
		string text = "";
		if (answer == null)
		{
			return;
		}
		foreach (string item in answer.pText)
		{
			text += item;
		}
		TextMesh component2 = gameObject.GetComponent<TextMesh>();
		if ((bool)component2)
		{
			component2.text = text;
		}
	}

	public void SetMathTargetByImage(CmAnswerItem answer, Texture image)
	{
		mAnswer = answer;
		UpdateTexture(image);
	}
}
