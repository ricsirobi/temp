using UnityEngine;

public class GauntletTargetOrbBase : MonoBehaviour
{
	public GameObject _OrbPrefab;

	public float _ShootHeight;

	public float _FlyDuration;

	private bool mIsActive;

	private bool mIsHit;

	private GameObject mOrb;

	private Vector3 mStartPos = Vector3.zero;

	private Vector3 mEndPos = Vector3.zero;

	private float mTimer;

	private bool mIsOrbFalling;

	public void Start()
	{
		if (_OrbPrefab != null)
		{
			mOrb = Object.Instantiate(_OrbPrefab);
			mStartPos = base.transform.position;
			mEndPos = mStartPos + base.transform.up * _ShootHeight;
			mOrb.transform.position = mStartPos;
			mOrb.transform.rotation = base.transform.rotation;
			mOrb.transform.parent = base.transform;
		}
	}

	private void ActivateTarget()
	{
		if (base.gameObject.activeInHierarchy && mOrb != null)
		{
			mIsActive = true;
			mTimer = 0f;
			mIsOrbFalling = false;
		}
	}

	public void Update()
	{
		if (mOrb != null && mIsActive && !mIsHit)
		{
			mTimer += (float)((!mIsOrbFalling) ? 1 : (-1)) * Time.deltaTime;
			mOrb.transform.position = Vector3.Lerp(mStartPos, mEndPos, mTimer / _FlyDuration);
			if (mTimer < 0f || mTimer > _FlyDuration)
			{
				mTimer = (mIsOrbFalling ? 0f : _FlyDuration);
				mIsOrbFalling = !mIsOrbFalling;
			}
		}
	}
}
