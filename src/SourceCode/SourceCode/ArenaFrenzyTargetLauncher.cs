using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaFrenzyTargetLauncher : ArenaFrenzyMapElement
{
	public override void Init(ArenaFrenzyGame inGameManager)
	{
	}

	public void OnTriggerEnter(Collider other)
	{
		ObAmmo component = other.GetComponent<ObAmmo>();
		if (component != null)
		{
			component.PlayHitParticle(base.transform.position, null);
			component.DeActivate();
			LaunchTarget();
		}
	}

	protected virtual void LaunchTarget()
	{
	}
}
