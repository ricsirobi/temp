using UnityEngine;

namespace VRPanorama;

public class AudioSyncWithVRCapture : MonoBehaviour
{
	public bool triggerAudio = true;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (triggerAudio)
		{
			GetComponent<AudioSource>().Play();
		}
		triggerAudio = false;
	}
}
