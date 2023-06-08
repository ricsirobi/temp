using UnityEngine;

public class SFXMapper : KAMonoBase
{
	public string[] AnimNames = new string[13]
	{
		"GlassPetting03", "BathShakeFur", "EatFoodBowl", "GlassDownPetBody", "GlassDownPetHead", "Refuse", "Run", "Scratch", "ShakeHeadChewToy01", "TugOWar",
		"walk", "WalkSad", "ShakeHeadChewToy02"
	};

	public AudioClip[] SoundFX;

	private void Start()
	{
		if (base.audio == null)
		{
			Debug.LogError("SFXMapper must attach to a gameobject with audio source component.");
		}
	}

	public void SetLooping(bool looping)
	{
		if ((bool)base.audio)
		{
			base.audio.loop = looping;
		}
	}

	public void PlayAnimSFX(string aname, bool looping)
	{
		if (base.audio == null)
		{
			return;
		}
		for (int i = 0; i < AnimNames.Length; i++)
		{
			if (aname == AnimNames[i])
			{
				if (SoundFX[i] != null)
				{
					base.audio.clip = SoundFX[i];
					base.audio.loop = looping;
					base.audio.Play();
				}
				else
				{
					base.audio.Stop();
				}
				return;
			}
		}
		base.audio.Stop();
	}
}
