using UnityEngine;

public class ObjectAnim : KAMonoBase
{
	public string _InitAnimName = "Still";

	public string _AnimName = "Play";

	public float _OpenTime = 2f;

	public float _CloseTime = 3f;

	public AudioClip _Sound;

	public float _SoundRange;

	public string _SoundPool;

	private float mTime;

	private bool mIsOn;

	private void Start()
	{
		base.animation.Play(_InitAnimName);
		mTime = 0f;
	}

	private void Update()
	{
		mTime += Time.deltaTime;
		float num = ((!mIsOn) ? _OpenTime : _CloseTime);
		if (!(mTime > num))
		{
			return;
		}
		base.animation[_AnimName].speed = ((!mIsOn) ? 1 : (-1));
		mIsOn = !mIsOn;
		base.animation.Play(_AnimName);
		mTime = 0f;
		if (!mIsOn || !(AvAvatar.pObject != null))
		{
			return;
		}
		float num2 = Vector3.Distance(AvAvatar.position, base.gameObject.transform.position);
		if (num2 <= _SoundRange)
		{
			SnChannel snChannel = SnChannel.AcquireChannel(_SoundPool, inForce: true);
			if (snChannel != null)
			{
				snChannel.pClip = _Sound;
				snChannel.Play();
				snChannel.pVolume = (_SoundRange - num2) / _SoundRange;
			}
		}
	}
}
