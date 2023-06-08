using UnityEngine;

namespace ReachableGames.AutoProbe;

public class ToggleOnAndOff : MonoBehaviour
{
	public float frequency = 1f;

	private void Start()
	{
		InvokeRepeating("Toggling", frequency, frequency);
	}

	public void Toggling()
	{
		base.gameObject.SetActive(Mathf.Sin(Time.time * frequency) > 0f);
	}
}
