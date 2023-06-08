using UnityEngine;

public class ObRankRoom : MonoBehaviour
{
	public GameObject _AvatarMarker;

	public GameObject _Particle;

	public float _LookTime = 5f;

	private GameObject mEndMessageObj;

	private string mEndMessage = "";

	private Vector3 mOldAVPos;

	private Quaternion mOldAVRot;

	private GameObject mOldCam;

	private bool mCameraSwitched;

	private bool mUpdatingRank;

	private bool mParticleUp;

	private float mLookTimer;

	private float mWaitTimer;

	public AudioClip pNewRankMessageClip;

	public void OnParticleUp()
	{
		mParticleUp = true;
	}

	public void SetEndMessage(GameObject obj, string msg)
	{
		mEndMessageObj = obj;
		mEndMessage = msg;
	}

	private void OnSnEvent(SnEvent sndEvent)
	{
		if ((sndEvent.mType == SnEventType.STOP || sndEvent.mType == SnEventType.END) && SnUtility.SafeClipCompare(sndEvent.mClip, pNewRankMessageClip))
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void UpdateAvatarRank()
	{
		mUpdatingRank = true;
	}

	public void PlayRankVO()
	{
		SnChannel.Play(pNewRankMessageClip, "VO_Pool", 0, inForce: true, base.gameObject);
		UpdateAvatarRank();
	}

	private void Update()
	{
		if (!mCameraSwitched)
		{
			UICursorManager.pCursorManager.HideCursor(t: true);
			AvAvatar.pState = AvAvatarState.NONE;
			AvAvatar.SetDisplayNameVisible(inVisible: false);
			AvAvatar.PlayAnim("CadetIdle", WrapMode.Loop);
			mCameraSwitched = true;
			mOldCam = GameObject.Find("PfAvatarCamera");
			if (mOldCam != null)
			{
				mOldCam.SetActive(value: false);
			}
			if (AvAvatar.pObject != null)
			{
				mOldAVPos = AvAvatar.position;
				mOldAVRot = AvAvatar.mTransform.rotation;
				AvAvatar.SetPosition(_AvatarMarker.transform);
			}
		}
		if (mUpdatingRank && AvatarData.pIsReady && mParticleUp)
		{
			mUpdatingRank = false;
			mParticleUp = false;
			AvAvatar.PlayAnim("IdleFidget01", WrapMode.Loop);
			mWaitTimer = 2f;
			_Particle.SetActive(value: false);
		}
		if (mWaitTimer >= 0f)
		{
			mWaitTimer -= Time.deltaTime;
			if (mWaitTimer <= 0f)
			{
				mWaitTimer = 0f;
			}
			mLookTimer = _LookTime;
		}
		if (mLookTimer >= 0f)
		{
			mLookTimer -= Time.deltaTime;
			if (mLookTimer <= 0f)
			{
				mLookTimer = 0f;
				AvAvatar.PlayAnim("CadetIdle", WrapMode.Loop);
			}
		}
	}

	private void OnDisable()
	{
		UICursorManager.pCursorManager.HideCursor(t: false);
		AvAvatar.SetDisplayNameVisible(inVisible: true);
		AvAvatar.position = mOldAVPos;
		AvAvatar.mTransform.rotation = mOldAVRot;
		AvAvatar.PlayAnim("CadetIdle", WrapMode.Loop);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (mOldCam != null)
		{
			mOldCam.SetActive(value: true);
		}
		if (mEndMessageObj != null)
		{
			mEndMessageObj.SendMessage(mEndMessage);
		}
	}
}
