using UnityEngine;

public class RateApp : MonoBehaviour
{
	public int[] _MissionIDs;

	public bool _RateOnEggHatch;

	private static RateApp mInstance;

	private bool mRegisterEvents;

	public static RateApp pInstance => mInstance;

	public void Awake()
	{
		if (mInstance == null && UniRate.Instance != null && !UniRate.Instance.ratedAnyVersion)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			mInstance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (!(inObject is Mission mission) || inEvent != MissionEvent.MISSION_COMPLETE)
		{
			return;
		}
		int[] missionIDs = _MissionIDs;
		for (int i = 0; i < missionIDs.Length; i++)
		{
			if (missionIDs[i] == mission.MissionID)
			{
				RateGame();
				break;
			}
		}
	}

	public void Update()
	{
		if (!mRegisterEvents && MissionManager.pIsReady && mInstance != null)
		{
			MissionManager.AddMissionEventHandler(OnMissionEvent);
			mRegisterEvents = true;
		}
	}

	public void RateOnEggHatch()
	{
		if (_RateOnEggHatch)
		{
			RateGame();
		}
	}

	private void RateGame()
	{
	}

	public void OnDestroy()
	{
		if (mRegisterEvents)
		{
			MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		}
	}
}
