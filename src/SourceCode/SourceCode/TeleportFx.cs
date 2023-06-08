using UnityEngine;

public class TeleportFx : KAMonoBase
{
	public AudioClip _Sound;

	private bool mPlayed;

	private bool mSoundPlayed;

	private bool mShouldPlaySound = true;

	public static void PlayAt(Vector3 inPosition, bool inPlaySound = true, string inTelportFx = "PfTeleportFx")
	{
		if (!string.IsNullOrEmpty(inTelportFx))
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(inTelportFx);
			if (!(gameObject == null))
			{
				PlayAt(inPosition, inPlaySound, gameObject);
			}
		}
	}

	public static void PlayAt(Vector3 inPosition, bool inPlaySound, GameObject inTelportFx)
	{
		GameObject gameObject = Object.Instantiate(inTelportFx, inPosition, Quaternion.identity);
		if (gameObject != null)
		{
			TeleportFx component = gameObject.GetComponent<TeleportFx>();
			if (component != null)
			{
				component.mShouldPlaySound = inPlaySound;
			}
		}
	}

	private void Update()
	{
		if (mShouldPlaySound && !mSoundPlayed)
		{
			mSoundPlayed = true;
			SnChannel.Play(_Sound, "TELEPORT_FX_POOL", inForce: false);
		}
		if (base.animation != null)
		{
			if (base.animation.isPlaying)
			{
				mPlayed = true;
			}
			if (mPlayed && !base.animation.isPlaying)
			{
				AnimPlayed();
			}
		}
	}

	public virtual void AnimPlayed()
	{
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}
}
