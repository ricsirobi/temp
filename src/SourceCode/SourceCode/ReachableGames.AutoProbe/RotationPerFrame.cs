using UnityEngine;

namespace ReachableGames.AutoProbe;

public class RotationPerFrame : MonoBehaviour
{
	public Vector3 rotPerFrame = Vector3.zero;

	private void Update()
	{
		base.transform.Rotate(rotPerFrame);
	}
}
