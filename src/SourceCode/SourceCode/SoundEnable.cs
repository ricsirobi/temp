using UnityEngine;

public class SoundEnable : MonoBehaviour
{
	[SerializeField]
	private AudioSource m_AudioSource;

	private void OnEnable()
	{
		m_AudioSource.mute = false;
	}

	private void OnDisable()
	{
		m_AudioSource.mute = true;
	}
}
