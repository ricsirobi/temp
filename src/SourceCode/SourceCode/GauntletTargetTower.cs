using System.Collections.Generic;
using UnityEngine;

public class GauntletTargetTower : MonoBehaviour
{
	public AudioClip _SwingOutSFX;

	public AudioClip _SwingBackSFX;

	public GauntletTowerArmData[] _ArmData;

	public GauntletTowerArmSpawnDirection _SpawnDirection = GauntletTowerArmSpawnDirection.RANDOM;

	public string _ArmStillAnim = "TargetTowerArm@Still";

	public string _ArmLeftAnim = "TargetTowerArm@Left90Turn";

	public string _ArmRightAnim = "TargetTowerArm@Right90Turn";

	public float _ResetTime;

	private List<GameObject> mArms = new List<GameObject>();

	private float mResetTimer;

	private void InstantiateArm()
	{
		if (_ArmData == null || _ArmData.Length == 0)
		{
			return;
		}
		foreach (GameObject mArm in mArms)
		{
			Object.Destroy(mArm);
		}
		mArms.Clear();
		GauntletTowerArmData[] armData = _ArmData;
		foreach (GauntletTowerArmData gauntletTowerArmData in armData)
		{
			GameObject gameObject = Object.Instantiate(gauntletTowerArmData._Prefab);
			gameObject.transform.position = base.transform.position + new Vector3(0f, gauntletTowerArmData._SpawnOffsetY * base.transform.localScale.y, 0f);
			gameObject.transform.rotation = base.transform.rotation;
			gameObject.transform.localScale = base.transform.localScale;
			gameObject.transform.parent = base.transform;
			mArms.Add(gameObject);
		}
	}

	private void Update()
	{
		if (!(mResetTimer > 0f))
		{
			return;
		}
		mResetTimer -= Time.deltaTime;
		if (!(mResetTimer <= 0f))
		{
			return;
		}
		foreach (GameObject mArm in mArms)
		{
			PlayAnim(mArm, _ArmStillAnim);
		}
		PlayAudio(_SwingBackSFX);
	}

	private void ActivateTarget()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		mResetTimer = _ResetTime;
		InstantiateArm();
		foreach (GameObject mArm in mArms)
		{
			if (mArm.GetComponentInChildren<GauntletTarget>() != null)
			{
				bool flag = true;
				if (_SpawnDirection == GauntletTowerArmSpawnDirection.RIGHT)
				{
					flag = false;
				}
				else if (_SpawnDirection == GauntletTowerArmSpawnDirection.RANDOM)
				{
					flag = ((Random.Range(0, 100) % 2 == 0) ? true : false);
				}
				if (flag)
				{
					PlayAnim(mArm, _ArmLeftAnim);
				}
				else
				{
					PlayAnim(mArm, _ArmRightAnim);
				}
			}
		}
		PlayAudio(_SwingOutSFX);
	}

	private void PlayAnim(GameObject inObject, string inAnimationName)
	{
		if (inObject != null && inAnimationName.Length > 0)
		{
			Component[] componentsInChildren = inObject.GetComponentsInChildren<Animation>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Animation)componentsInChildren[i]).CrossFade(inAnimationName, 0.2f);
			}
		}
	}

	private void PlayAudio(AudioClip inClip)
	{
		if (inClip != null)
		{
			SnChannel.Play(inClip, "GSTargetTower_Pool", inForce: true);
		}
	}
}
