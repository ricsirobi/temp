using UnityEngine;

public class KAUIStoreBase : KAUI
{
	public KAStoreMenuItemData[] _MenuItemData;

	public AudioClip[] _IdleVOs;

	public void SetIdleVOs()
	{
		if (!(KAUIStoreCategory.pInstance == null))
		{
			AudioClip[] array = new AudioClip[_IdleVOs.Length + KAUIStoreCategory.pInstance._BaseIdleVOs.Length];
			int num = 0;
			for (int i = 0; i < KAUIStoreCategory.pInstance._BaseIdleVOs.Length; i++)
			{
				array[num] = KAUIStoreCategory.pInstance._BaseIdleVOs[i];
				num++;
			}
			for (int i = 0; i < _IdleVOs.Length; i++)
			{
				array[num] = _IdleVOs[i];
				num++;
			}
			KAUIStoreCategory.pInstance._IdleMgr.SetIdleVOs(array);
		}
	}

	public virtual void LoadSelectedStore()
	{
		if (!GetVisibility())
		{
			return;
		}
		SetIdleVOs();
		if (KAUIStoreCategory.pInstance != null)
		{
			KAUIStoreCategory.pInstance._IdleMgr.StartIdles();
			if (_MenuItemData != null && _MenuItemData.Length != 0)
			{
				KAUIStoreCategory.pInstance._Menu.SetCategories(_MenuItemData);
			}
		}
	}

	public virtual void SelectCategory(KAStoreMenuItemData pd)
	{
	}
}
