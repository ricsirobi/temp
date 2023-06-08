using UnityEngine;

public class CoIdleManager : MonoBehaviour
{
	public bool _ShuffleVO = true;

	public AudioClip[] _VOs;

	public string _Pool = "VO_Pool";

	public int _Priority;

	public float _Interval = 30f;

	public bool _ResetOnKeyInput = true;

	public int _PlayProbabilty = 100;

	protected float mTimer = 30f;

	protected bool mStopped = true;

	protected int mNextPlayIdx;

	protected string mDoNotPlay = "";

	private void Start()
	{
		if (_VOs.Length > 1 && _ShuffleVO)
		{
			ShuffleIdles();
		}
	}

	public virtual void ShuffleIdles()
	{
		int num = _VOs.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = Random.Range(0, num);
			int num3 = Random.Range(0, num);
			if (num2 != num3)
			{
				AudioClip audioClip = _VOs[num2];
				_VOs[num2] = _VOs[num3];
				_VOs[num3] = audioClip;
			}
		}
	}

	public void SetIdleVOs(AudioClip[] vos)
	{
		_VOs = vos;
	}

	public void StartIdles()
	{
		ResetIdles();
		mStopped = false;
	}

	public void StopIdles()
	{
		mStopped = true;
	}

	public virtual void OnHelpPressed(string doNotPlayAudio)
	{
		mDoNotPlay = doNotPlayAudio;
		OnIdlePlay();
	}

	public virtual void OnIdlePlay()
	{
		if (_VOs.Length == 0)
		{
			return;
		}
		if (mNextPlayIdx >= _VOs.Length)
		{
			mNextPlayIdx = 0;
		}
		if (mDoNotPlay.Length > 0 && mDoNotPlay == _VOs[mNextPlayIdx].name)
		{
			mDoNotPlay = "";
			mNextPlayIdx++;
			if (mNextPlayIdx == _VOs.Length)
			{
				mNextPlayIdx = 0;
			}
			OnIdlePlay();
			return;
		}
		if (Random.Range(1, 100) <= _PlayProbabilty)
		{
			SnChannel.Play(_VOs[mNextPlayIdx], _Pool, _Priority, inForce: false, null);
			mNextPlayIdx++;
			if (mNextPlayIdx == _VOs.Length)
			{
				mNextPlayIdx = 0;
			}
		}
		ResetIdles();
	}

	public void ResetIdles()
	{
		mTimer = _Interval;
	}

	private void Update()
	{
		if (mStopped || RsResourceManager.pLevelLoading || _VOs.Length == 0)
		{
			return;
		}
		if (_ResetOnKeyInput && Input.anyKey)
		{
			ResetIdles();
			return;
		}
		mTimer -= Time.deltaTime;
		if (mTimer <= 0f)
		{
			OnIdlePlay();
		}
	}
}
