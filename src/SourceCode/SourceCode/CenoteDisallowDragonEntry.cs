using System.Collections;
using UnityEngine;

public class CenoteDisallowDragonEntry : MonoBehaviour
{
	public GameObject _WalkingCollider;

	public GameObject _FlyingCollider;

	private AvAvatarController mAvatarController;

	public AvAvatarController pAvController
	{
		get
		{
			if (mAvatarController == null && AvAvatar.pObject != null)
			{
				mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			return mAvatarController;
		}
	}

	private void Start()
	{
		StartCoroutine(WaitForSanctuaryManager());
		if (pAvController != null)
		{
			pAvController.OnPetFlyingStateChanged += OnFlyingStateChanged;
		}
		if (_WalkingCollider != null)
		{
			_WalkingCollider.SetActive(value: false);
		}
		if (_FlyingCollider != null)
		{
			_FlyingCollider.SetActive(value: false);
		}
	}

	private IEnumerator WaitForSanctuaryManager()
	{
		while (SanctuaryManager.pInstance == null)
		{
			yield return null;
		}
		SanctuaryManager.pInstance.OnPetChanged += PetChanged;
	}

	private void OnFlyingStateChanged(FlyingState newState)
	{
		if (pAvController != null && !pAvController.pPlayerMounted)
		{
			return;
		}
		if (newState == FlyingState.Grounded)
		{
			if (_WalkingCollider != null)
			{
				_WalkingCollider.SetActive(value: true);
			}
			if (_FlyingCollider != null)
			{
				_FlyingCollider.SetActive(value: false);
			}
		}
		else
		{
			if (_FlyingCollider != null)
			{
				_FlyingCollider.SetActive(value: true);
			}
			if (_WalkingCollider != null)
			{
				_WalkingCollider.SetActive(value: false);
			}
		}
	}

	private void PetChanged(SanctuaryPet pet)
	{
		SanctuaryPet.AddMountEvent(pet, DragonMounted);
	}

	private void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mount)
		{
			if (!pAvController.IsFlyingOrGliding())
			{
				if (_WalkingCollider != null)
				{
					_WalkingCollider.SetActive(value: true);
				}
				if (_FlyingCollider != null)
				{
					_FlyingCollider.SetActive(value: false);
				}
			}
		}
		else
		{
			if (_WalkingCollider != null)
			{
				_WalkingCollider.SetActive(value: false);
			}
			if (_FlyingCollider != null)
			{
				_FlyingCollider.SetActive(value: false);
			}
		}
	}

	private void OnDisable()
	{
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.OnPetChanged -= PetChanged;
			SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		}
		if (pAvController != null)
		{
			pAvController.OnPetFlyingStateChanged -= OnFlyingStateChanged;
		}
	}
}
