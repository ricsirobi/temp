using UnityEngine;

public class CoConcertController : CoAnimController
{
	public AudioClip _MainSong;

	private void PlayDefaultAnimLoop(Transform trans)
	{
		Component[] componentsInChildren = trans.GetComponentsInChildren(typeof(Transform));
		foreach (Component component in componentsInChildren)
		{
			if (component != trans)
			{
				Animation component2 = ((Transform)component).GetComponent<Animation>();
				if (component2 != null && component2.clip != null)
				{
					component2[component2.clip.name].wrapMode = WrapMode.Loop;
					component2.Play(component2.clip.name);
				}
			}
		}
	}

	public void StartWithOffset(float animOffseinSecs, float sndOffseinSecs)
	{
		if (_MainSong != null)
		{
			SnChannel snChannel = (SnChannel)base.gameObject.GetComponent(typeof(SnChannel));
			if (snChannel != null)
			{
				SnChannel.Play(_MainSong, snChannel._Pool, inForce: true);
				snChannel.pAudioSource.time = sndOffseinSecs;
			}
		}
		if (!(base.animation.clip != null))
		{
			return;
		}
		base.animation[base.animation.clip.name].time = animOffseinSecs;
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Transform));
		foreach (Component component in componentsInChildren)
		{
			if (!(component != base.transform))
			{
				continue;
			}
			if ((CoAnimController)component.gameObject.GetComponent(typeof(CoAnimController)) != null)
			{
				Transform transform = (Transform)component;
				Animation component2 = transform.GetComponent<Animation>();
				if (component2 != null && component2.clip != null)
				{
					component2[component2.clip.name].time = animOffseinSecs;
					PlayDefaultAnimLoop(transform);
				}
			}
			if ((CoCameraControl)component.gameObject.GetComponent(typeof(CoCameraControl)) != null)
			{
				Animation component3 = ((Transform)component).GetComponent<Animation>();
				if (component3 != null && component3.clip != null)
				{
					component3[component3.clip.name].time = animOffseinSecs;
				}
			}
		}
	}
}
