using System.Collections;
using UnityEngine;

public class CrossbowWeapon : WeaponManager
{
	public Animation _Amimation;

	public string _ReloadAnim;

	public Renderer _DummyArrowRenderer;

	protected override void Awake()
	{
		base.Awake();
		base.pUserControlledWeapon = true;
	}

	public override bool Fire(Transform target, bool useDirection, Vector3 direction, float parentSpeed)
	{
		IsLocal = true;
		if (_Amimation != null)
		{
			_Amimation.Play(_ReloadAnim);
		}
		if (_DummyArrowRenderer != null)
		{
			StartCoroutine(SetDummyAmmoVisible(visible: false, GauntletRailShootManager.pInstance._GauntletController.pShootInterval));
		}
		return base.Fire(target, useDirection, direction, parentSpeed);
	}

	public IEnumerator SetDummyAmmoVisible(bool visible, float waitSecs)
	{
		_DummyArrowRenderer.enabled = visible;
		if (!visible)
		{
			yield return new WaitForSeconds(waitSecs);
			StartCoroutine(SetDummyAmmoVisible(visible: true, waitSecs));
		}
	}
}
