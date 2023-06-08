using UnityEngine;

public class ObInfoCube : KAMonoBase
{
	public Transform _BoneUL;

	public Transform _BoneUR;

	public Transform _BoneBL;

	public Transform _BoneBR;

	public float _StretchTime = 1f;

	public float _StretchDistX = 3f;

	public float _StretchDistY = 3f;

	public float _IntroSpeed = 1f;

	public string _IntroAnimName = "Open";

	public float _SpinSpeed = 1f;

	public string _SpinAnimName = "Close";

	public AudioClip _IntroSFX;

	public AudioClip _SpinSFX;

	public AudioClip _OpenSFX;

	public AudioClip _CloseSFX;

	private float mStretchTimer;

	private GameObject mMsgObj;

	private string mCurAnim = "";

	private string mMessage = "";

	private bool mStretchOpen = true;

	private Vector3 mPosUL;

	private Vector3 mPosUR;

	private Vector3 mPosBL;

	private Vector3 mPosBR;

	private void Start()
	{
		mPosUL = _BoneUL.localPosition;
		mPosUR = _BoneUR.localPosition;
		mPosBL = _BoneBL.localPosition;
		mPosBR = _BoneBR.localPosition;
	}

	public void Reset()
	{
		_BoneUL.localPosition = mPosUL;
		_BoneUR.localPosition = mPosUR;
		_BoneBL.localPosition = mPosBL;
		_BoneBR.localPosition = mPosBR;
	}

	private void MoveBone(Transform bone, Vector3 op, float f, Vector3 md)
	{
		bone.localPosition = op + md * f;
	}

	private void Update()
	{
		if (mStretchTimer > 0f)
		{
			mStretchTimer -= Time.deltaTime;
			if (mStretchTimer <= 0f)
			{
				mStretchTimer = 0f;
				mMsgObj.SendMessage(mMessage);
			}
			float num = mStretchTimer / _StretchTime;
			if (mStretchOpen)
			{
				num = 1f - num;
			}
			MoveBone(_BoneUL, mPosUL, num, new Vector3(_StretchDistX, _StretchDistY, 0f));
			MoveBone(_BoneUR, mPosUR, num, new Vector3(0f - _StretchDistX, _StretchDistY, 0f));
			MoveBone(_BoneBL, mPosBL, num, new Vector3(_StretchDistX, 0f - _StretchDistY, 0f));
			MoveBone(_BoneBR, mPosBR, num, new Vector3(0f - _StretchDistX, 0f - _StretchDistY, 0f));
		}
		if (mCurAnim.Length > 0 && !base.animation.IsPlaying(mCurAnim))
		{
			mCurAnim = "";
			mMsgObj.SendMessage(mMessage);
		}
	}

	public void StretchOpen(GameObject msgobj, string msg, bool isOpen)
	{
		mMsgObj = msgobj;
		mMessage = msg;
		mStretchTimer = _StretchTime;
		mStretchOpen = isOpen;
		if (isOpen)
		{
			if (_OpenSFX != null)
			{
				SnChannel.Play(_OpenSFX, "SFX_Pool", inForce: true, null);
			}
		}
		else if (_CloseSFX != null)
		{
			SnChannel.Play(_CloseSFX, "SFX_Pool", inForce: true, null);
		}
	}

	public void PlayIntroAnim(GameObject msgobj, string msg)
	{
		PlayAnim(_IntroAnimName, msgobj, msg);
		base.animation[_IntroAnimName].speed = _IntroSpeed;
		if (_IntroSFX != null)
		{
			SnChannel.Play(_IntroSFX, "SFX_Pool", inForce: true, null);
		}
	}

	public void PlaySpinAnim(GameObject msgobj, string msg)
	{
		PlayAnim(_SpinAnimName, msgobj, msg);
		base.animation[_SpinAnimName].speed = _SpinSpeed;
		if (_SpinSFX != null)
		{
			SnChannel.Play(_SpinSFX, "SFX_Pool", inForce: true, null);
		}
	}

	public void PlayAnim(string aname, GameObject msgobj, string msg)
	{
		base.animation.Play(aname);
		mMsgObj = msgobj;
		mCurAnim = aname;
		mMessage = msg;
	}
}
