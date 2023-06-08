using UnityEngine;
using UnityEngine.SceneManagement;

public class AvStateOverride : MonoBehaviour
{
	public AvAvatarSubState _State;

	public float _PreviousGravity;

	public float _PreviousJumpHeight = -1f;

	public float _PreviousMaxAirSpeed;

	public bool _RemoveOnLevelLoad = true;

	public bool _RunTimer;

	public float _Timeout = 30f;

	public float _EndTime = float.PositiveInfinity;

	public static AvStateOverride FindOverride(GameObject inAvatar, AvAvatarSubState inState)
	{
		if (inAvatar != null)
		{
			AvStateOverride[] components = inAvatar.GetComponents<AvStateOverride>();
			foreach (AvStateOverride avStateOverride in components)
			{
				if (avStateOverride != null && avStateOverride._State == inState)
				{
					return avStateOverride;
				}
			}
		}
		else
		{
			Debug.LogError("ERROR: AVSTATEOVERRIDE FIND CALLED WITH NULL AVATAR OBJECT!!");
		}
		return null;
	}

	public static AvStateOverride FindOrCreate(GameObject inAvatar, AvAvatarSubState inState)
	{
		AvStateOverride avStateOverride = FindOverride(inAvatar, inState);
		if (avStateOverride == null)
		{
			AvAvatarController component = inAvatar.GetComponent<AvAvatarController>();
			if (component != null)
			{
				avStateOverride = inAvatar.AddComponent<AvStateOverride>();
				avStateOverride._State = inState;
				AvAvatarStateData stateDataFromSubState = component.GetStateDataFromSubState(avStateOverride._State);
				avStateOverride._PreviousGravity = stateDataFromSubState._Gravity;
				avStateOverride._PreviousJumpHeight = stateDataFromSubState._JumpValues._MaxJumpHeight;
				avStateOverride._PreviousMaxAirSpeed = stateDataFromSubState._MaxAirSpeed;
			}
			else
			{
				Debug.LogError("ERROR: AVSTATEOVERRIDE CREATE CALLED WITH NULL AVATAR CONTROLLER: " + inAvatar.name);
			}
		}
		return avStateOverride;
	}

	public static AvStateOverride SetOrCreate(GameObject inAvatar, AvAvatarSubState inState, bool inRemoveOnLevelLoad, float inTimeout, float inGravity, float inJumpHeight, float inMaxAirSpeed)
	{
		AvStateOverride avStateOverride = FindOrCreate(inAvatar, inState);
		if (avStateOverride != null)
		{
			avStateOverride.Set(inRemoveOnLevelLoad, inTimeout, inGravity, inJumpHeight, inMaxAirSpeed);
		}
		return avStateOverride;
	}

	private void Update()
	{
		if (_RunTimer && !(Time.time < _EndTime))
		{
			OnRemove();
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (_RemoveOnLevelLoad)
		{
			OnRemove();
		}
	}

	public void Set(bool inRemoveOnLevelLoad, float inTimeout, float inGravity, float inJumpHeight, float inMaxAirSpeed)
	{
		_RunTimer = false;
		_EndTime = float.PositiveInfinity;
		_RemoveOnLevelLoad = inRemoveOnLevelLoad;
		_Timeout = inTimeout;
		AvAvatarController component = GetComponent<AvAvatarController>();
		if (component != null)
		{
			AvAvatarStateData stateDataFromSubState = component.GetStateDataFromSubState(_State);
			stateDataFromSubState._Gravity = inGravity;
			stateDataFromSubState._JumpValues._MinJumpHeight = inJumpHeight;
			stateDataFromSubState._JumpValues._MaxJumpHeight = inJumpHeight;
			stateDataFromSubState._MaxAirSpeed = inMaxAirSpeed;
		}
		else
		{
			Debug.LogError("ERROR: AVSTATEOVERRIDE SET CALLED WITH NULL AVATAR CONTROLLER!!");
		}
	}

	public void OnStartTimer()
	{
		_RunTimer = true;
		_EndTime = Time.time + _Timeout;
	}

	public void OnResetTimer()
	{
		_RunTimer = false;
		_EndTime = float.PositiveInfinity;
	}

	public void OnRemove()
	{
		AvAvatarController component = GetComponent<AvAvatarController>();
		if (component != null)
		{
			AvAvatarStateData stateDataFromSubState = component.GetStateDataFromSubState(_State);
			stateDataFromSubState._Gravity = _PreviousGravity;
			stateDataFromSubState._JumpValues._MinJumpHeight = _PreviousJumpHeight;
			stateDataFromSubState._JumpValues._MaxJumpHeight = _PreviousJumpHeight;
			stateDataFromSubState._MaxAirSpeed = _PreviousMaxAirSpeed;
		}
		Object.Destroy(this);
	}
}
