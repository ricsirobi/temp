using UnityEngine;

public class GlowTransitionFx : KAMonoBase
{
	public AudioClip _PlaySfx;

	public static void PlayAt(Vector3 inPosition, bool inPlaySound = true, string inGlowTransitionFx = "PfGlowTransitionFx")
	{
		if (!string.IsNullOrEmpty(inGlowTransitionFx))
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(inGlowTransitionFx);
			if (!(gameObject == null))
			{
				PlayAt(inPosition, inPlaySound, gameObject);
			}
		}
	}

	public static void PlayAt(Vector3 inPosition, bool inPlaySound, GameObject inGlowTransitionFx)
	{
		Object.Instantiate(inGlowTransitionFx, inPosition, Quaternion.identity);
	}

	public void Start()
	{
		if (_PlaySfx != null)
		{
			SnChannel.Play(_PlaySfx, "SFX_Pool", inForce: true);
		}
	}
}
