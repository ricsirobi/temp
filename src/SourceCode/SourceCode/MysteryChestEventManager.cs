using SOD.Event;
using UnityEngine;

public class MysteryChestEventManager : MonoBehaviour
{
	public GameObject[] _MysteryChestManagers;

	public MysteryChestManager _ParentMysteryChestManager;

	private void Start()
	{
		EventManager activeEvent = EventManager.GetActiveEvent();
		MysteryChestManager currentEventChestManager = GetCurrentEventChestManager(activeEvent);
		if (!(currentEventChestManager == null))
		{
			currentEventChestManager.gameObject.SetActive(value: true);
			SetSpawnNodesForActiveManager(currentEventChestManager);
			SpawnChests(currentEventChestManager);
		}
	}

	private MysteryChestManager GetCurrentEventChestManager(EventManager eventManager)
	{
		MysteryChestManager component = _MysteryChestManagers[0].GetComponent<MysteryChestManager>();
		if ((bool)eventManager)
		{
			for (int i = 0; i < _MysteryChestManagers.Length; i++)
			{
				if (_MysteryChestManagers[i].GetComponent<MysteryChestManager>()._EventName == eventManager._EventName && eventManager.EventInProgress() && !eventManager.GracePeriodInProgress())
				{
					component = _MysteryChestManagers[i].GetComponent<MysteryChestManager>();
					break;
				}
			}
		}
		return component;
	}

	private void SetSpawnNodesForActiveManager(MysteryChestManager manager)
	{
		for (int i = 0; i < manager._ListOfChests.Length; i++)
		{
			manager._ListOfChests[i]._SpawnNodeList = _ParentMysteryChestManager._ListOfChests[i]._SpawnNodeList;
		}
	}

	private void SpawnChests(MysteryChestManager manager)
	{
		manager.SpawnChests();
	}
}
