using UnityEngine;

public class ObTriggerSFX : MonoBehaviour
{
	public bool _PlayerAvatarOnly;

	public SnSound _Sound;

	public SnSound _ExitSound;

	public GameObject _TargetObject;

	private bool mDefaultLoop;

	private SnChannel mChannel;

	private void Start()
	{
		mDefaultLoop = _Sound._Settings._Loop;
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!_PlayerAvatarOnly || AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			if (_Sound._AudioClip != null)
			{
				_Sound._Settings._Loop = mDefaultLoop;
				mChannel = _Sound.Play();
			}
			if (_TargetObject != null)
			{
				_TargetObject.SetActive(value: true);
			}
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if (_PlayerAvatarOnly && !AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			return;
		}
		if (mChannel != null)
		{
			mChannel.pAudioSource.loop = false;
		}
		if (_ExitSound != null && _ExitSound._AudioClip != null)
		{
			if (mChannel != null && _ExitSound._Settings._Pool == mChannel.pPool && (string.IsNullOrEmpty(_ExitSound._Settings._Channel) || _ExitSound._Settings._Channel == mChannel.pName))
			{
				mChannel.Stop();
			}
			_ExitSound.Play();
			mChannel = null;
		}
		if (_TargetObject != null)
		{
			_TargetObject.SetActive(value: false);
		}
	}
}
