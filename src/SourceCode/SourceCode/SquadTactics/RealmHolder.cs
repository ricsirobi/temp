using UnityEngine;

namespace SquadTactics;

public class RealmHolder : MonoBehaviour
{
	public string _StoreSceneName = "StoresDM";

	public static RealmHolder pInstance { get; private set; }

	public int pRealmIndex { get; set; }

	public int pRealmLevelIndex { get; set; }

	public bool pSaveLevel { get; set; }

	public bool pShowAgeUp { get; set; }

	public string pLastLevel { get; set; }

	private void Awake()
	{
		if (pInstance == null)
		{
			pInstance = this;
			Object.DontDestroyOnLoad(this);
		}
		else
		{
			Object.Destroy(this);
		}
	}

	internal void ResetRealmValue()
	{
		pRealmIndex = -1;
		pRealmLevelIndex = -1;
		pSaveLevel = false;
	}

	internal void DestroyRealmObj()
	{
		pInstance = null;
		Object.Destroy(base.gameObject);
	}

	private void OnStoreClosed()
	{
		pShowAgeUp = false;
		if (!UiDragonsAgeUp.pIsTicketPurchased)
		{
			if (LevelManager.pInstance != null)
			{
				LevelManager.pInstance._TeamSelection.OpenAgeUpUI();
			}
		}
		else if (LevelManager.pInstance != null)
		{
			LevelManager.pInstance._TeamSelection.OpenTeamSelectionTab();
		}
	}
}
