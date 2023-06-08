using UnityEngine;

public class ObEventSound : MonoBehaviour
{
	public SnSound _Sound;

	private void OnSoundEvent()
	{
		if (_Sound != null)
		{
			_Sound.Play();
		}
	}
}
