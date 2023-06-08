using UnityEngine;

public class RailGunProjectile : PrProjectile
{
	public AudioClip _FireSFX;

	public AudioClip _SFXWrongTargetHit;

	public GameObject _Hit3DScorePrefab;

	public override void Start()
	{
		base.Start();
		PlaySFX(_FireSFX);
	}

	public override void DestroyMe()
	{
		if ((bool)_CollideEffect)
		{
			Object.Instantiate(_CollideEffect, base.transform.position, base.transform.rotation);
		}
		Object.Destroy(base.gameObject);
	}

	protected override void OnCollisionEnter(Collision iCollision)
	{
		PlaySFX(_SFXWrongTargetHit);
		DestroyMe();
		if (GauntletRailShootManager.pInstance != null)
		{
			GauntletRailShootManager.pInstance.OnProjectileHit(isHitTarget: false, base.gameObject);
		}
	}

	public void Show3DTargetHitScore(Vector3 inPosition, int inScore)
	{
		TargetHit3DScore.Show3DHitScore(_Hit3DScorePrefab, inPosition, inScore);
	}

	public void HandleTargetHit(GameObject inTarget, int inScore, string inAnimName)
	{
		Show3DTargetHitScore(base.transform.position, inScore);
		if (GauntletRailShootManager.pInstance != null)
		{
			GauntletRailShootManager.pInstance.OnProjectileHit(inScore > 0, base.gameObject);
			GauntletRailShootManager.pInstance.AddScore(inScore);
		}
		if (inAnimName != null && inAnimName.Length > 0)
		{
			Component[] componentsInChildren = inTarget.GetComponentsInChildren<Animation>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Animation)componentsInChildren[i]).CrossFade(inAnimName, 0.2f);
			}
		}
	}

	public void PlayNegativeSFX()
	{
		PlaySFX(_SFXWrongTargetHit);
	}

	public void SetParticleColor(Color color)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].SetColor("_TintColor", color);
			}
		}
	}

	private void PlaySFX(AudioClip inClip)
	{
		if (inClip != null)
		{
			SnChannel.Play(inClip, "GSFire_Pool", inForce: true);
		}
	}
}
