using UnityEngine;

public class ChildActivator : MonoBehaviour
{
	public ActiveChildConroller parent;

	private void OnEnable()
	{
		parent.ActivateOne();
	}

	private void OnDisable()
	{
		parent.DeactivateOne();
	}
}
